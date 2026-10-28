using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Agent;

/// <summary>Talks to ElevenLabs with the server-side API key, so the browser only ever sees a single-use token.</summary>
public sealed class ElevenLabsClient(HttpClient http, IOptions<AgentOptions> options)
{
    /// <summary>Single-use WebRTC token for a voice call.</summary>
    public async Task<string> GetConversationTokenAsync(CancellationToken cancellationToken)
    {
        var body = await GetAsync<TokenResponse>("v1/convai/conversation/token", cancellationToken);
        return body?.Token ?? throw new InvalidOperationException("ElevenLabs returned no conversation token.");
    }

    /// <summary>Signed WebSocket URL for a text chat.</summary>
    public async Task<string> GetSignedUrlAsync(CancellationToken cancellationToken)
    {
        var body = await GetAsync<SignedUrlResponse>("v1/convai/conversation/get-signed-url", cancellationToken);
        return body?.SignedUrl ?? throw new InvalidOperationException("ElevenLabs returned no signed URL.");
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{path}?agent_id={Uri.EscapeDataString(settings.AgentId!)}");
        request.Headers.Add("xi-api-key", settings.ElevenLabsApiKey);

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    private sealed record TokenResponse([property: JsonPropertyName("token")] string Token);

    private sealed record SignedUrlResponse([property: JsonPropertyName("signed_url")] string SignedUrl);
}
