using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JeremyMoore.Api.Agent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Functions;

public sealed class AgentFunctions(
    IOptions<AgentOptions> options,
    SessionLimiter limiter,
    ElevenLabsClient elevenLabs,
    IMessageSender messages,
    ILogger<AgentFunctions> logger)
{
    /// <summary>The site asks this before showing the "Talk to my AI agent" button.</summary>
    [Function("AgentStatus")]
    public async Task<HttpResponseData> Status(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "agent/status")] HttpRequestData request) =>
        await Json(request, HttpStatusCode.OK, new { available = options.Value.IsConfigured });

    /// <summary>Starts a voice session: returns a single-use WebRTC token, never the API key.</summary>
    [Function("AgentSession")]
    public async Task<HttpResponseData> Session(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agent/session")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        if (!options.Value.IsConfigured)
        {
            return await Json(request, HttpStatusCode.ServiceUnavailable, new { error = "unavailable" });
        }

        if (!limiter.TryAcquire(ClientAddress(request)))
        {
            return await Json(request, HttpStatusCode.TooManyRequests, new { error = "busy" });
        }

        try
        {
            var token = await elevenLabs.GetConversationTokenAsync(cancellationToken);
            return await Json(request, HttpStatusCode.OK, new { token });
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "ElevenLabs refused a conversation token ({Status}).", exception.StatusCode);
            return await Json(request, HttpStatusCode.BadGateway, new { error = "unavailable" });
        }
    }

    /// <summary>Webhook tool the agent calls to pass a visitor's message to Jeremy.</summary>
    [Function("AgentSendMessage")]
    public async Task<HttpResponseData> SendMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "agent/tools/send-message")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        if (!HasToolSecret(request, options.Value.ToolSecret))
        {
            return request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        VisitorMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<VisitorMessage>(request.Body, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            message = null;
        }

        // Tool responses are read by the agent, so they're written as instructions it can relay.
        var problems = message?.Problems() ?? ["the visitor's name, email and message"];
        if (problems.Count > 0)
        {
            return await Json(request, HttpStatusCode.OK, new
            {
                sent = false,
                result = $"Not sent. Ask the visitor for {string.Join(", ", problems)}, then try again.",
            });
        }

        if (!messages.IsConfigured)
        {
            logger.LogWarning("A visitor message arrived but email isn't configured.");
            return await Json(request, HttpStatusCode.OK, new
            {
                sent = false,
                result = "Not sent: messaging is offline. Suggest emailing jeremy@jeremymmoore.com instead.",
            });
        }

        try
        {
            await messages.SendAsync(message!, cancellationToken);
            return await Json(request, HttpStatusCode.OK, new
            {
                sent = true,
                result = "Sent. Tell the visitor Jeremy has it and will reply by email.",
            });
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Sending a visitor message failed ({Status}).", exception.StatusCode);
            return await Json(request, HttpStatusCode.OK, new
            {
                sent = false,
                result = "Not sent: email failed. Suggest emailing jeremy@jeremymmoore.com instead.",
            });
        }
    }

    /// <summary>Static Web Apps puts the caller's address in x-azure-clientip; x-forwarded-for is the fallback.</summary>
    public static string ClientAddress(HttpRequestData request)
    {
        if (request.Headers.TryGetValues("x-azure-clientip", out var direct) && direct.FirstOrDefault() is { Length: > 0 } ip)
        {
            return ip;
        }

        if (request.Headers.TryGetValues("x-forwarded-for", out var forwarded)
            && forwarded.FirstOrDefault()?.Split(',')[0].Trim() is { Length: > 0 } first)
        {
            return first;
        }

        return "unknown";
    }

    public static bool HasToolSecret(HttpRequestData request, string? expected)
    {
        if (string.IsNullOrEmpty(expected) || !request.Headers.TryGetValues("x-agent-secret", out var values))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(values.FirstOrDefault() ?? ""),
            Encoding.UTF8.GetBytes(expected));
    }

    private static async Task<HttpResponseData> Json<T>(HttpRequestData request, HttpStatusCode status, T body)
    {
        var response = request.CreateResponse();
        await response.WriteAsJsonAsync(body);
        response.StatusCode = status;
        response.Headers.Add("Cache-Control", "no-store");
        return response;
    }
}
