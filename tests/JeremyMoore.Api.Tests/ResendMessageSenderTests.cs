using System.Net;
using JeremyMoore.Api.Agent;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Tests;

public class ResendMessageSenderTests
{
    [Fact]
    public async Task A_pasted_key_with_a_trailing_newline_still_authenticates()
    {
        var handler = new RecordingHandler();
        var sender = new ResendMessageSender(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") },
            Options.Create(new AgentOptions { ResendApiKey = "re_test_key\n" }));

        await sender.SendAsync(new VisitorMessage("Ada", "ada@example.com", "TEST - please disregard"), CancellationToken.None);

        Assert.Equal("Bearer re_test_key", handler.Authorization);
        Assert.Equal("https://api.resend.com/emails", handler.Url);
        Assert.Contains("\"reply_to\":\"ada@example.com\"", handler.Body);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string? Authorization { get; private set; }
        public string? Url { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            Url = request.RequestUri?.ToString();
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
