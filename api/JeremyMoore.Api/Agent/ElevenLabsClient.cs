using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Agent;

/// <summary>Talks to ElevenLabs with the server-side API key, so the browser only ever sees a single-use token.</summary>
public sealed class ElevenLabsClient(HttpClient http, IOptions<AgentOptions> options)
{
    public async Task<string> GetConversationTokenAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"v1/convai/conversation/token?agent_id={Uri.EscapeDataString(settings.AgentId!)}");
        request.Headers.Add("xi-api-key", settings.ElevenLabsApiKey);

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        return body?.Token ?? throw new InvalidOperationException("ElevenLabs returned no conversation token.");
    }

    private sealed record TokenResponse([property: JsonPropertyName("token")] string Token);
}
