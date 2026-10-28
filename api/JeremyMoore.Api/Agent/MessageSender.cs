using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace JeremyMoore.Api.Agent;

public interface IMessageSender
{
    bool IsConfigured { get; }

    Task SendAsync(VisitorMessage message, CancellationToken cancellationToken);
}

/// <summary>Emails a visitor's message to Jeremy through Resend, with Reply-To set to the visitor.</summary>
public sealed class ResendMessageSender(HttpClient http, IOptions<AgentOptions> options) : IMessageSender
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ResendApiKey);

    public async Task SendAsync(VisitorMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var company = string.IsNullOrWhiteSpace(message.Company) ? "" : $" ({message.Company})";

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new
            {
                from = settings.MessageFrom,
                to = new[] { settings.MessageTo },
                reply_to = message.Email,
                subject = $"Site agent: {message.Name}{company}",
                text = $"""
                    {message.Message}

                    — {message.Name}{company}
                    {message.Email}

                    Taken by the voice agent on jeremymmoore.com.
                    """,
            }),
        };
        request.Headers.Authorization = new("Bearer", settings.ResendApiKey);

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
