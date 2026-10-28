using System.Net.Mail;
using System.Text.Json.Serialization;

namespace JeremyMoore.Api.Agent;

/// <summary>What the agent collects when a visitor asks Jeremy to get in touch.</summary>
public sealed record VisitorMessage(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("company")] string? Company = null)
{
    public IReadOnlyList<string> Problems()
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100)
        {
            problems.Add("a name (up to 100 characters)");
        }

        if (string.IsNullOrWhiteSpace(Email) || Email.Length > 200 || !MailAddress.TryCreate(Email, out _))
        {
            problems.Add("a valid email address");
        }

        if (string.IsNullOrWhiteSpace(Message) || Message.Length > 2000)
        {
            problems.Add("a message (up to 2,000 characters)");
        }

        if (Company is { Length: > 100 })
        {
            problems.Add("a shorter company name");
        }

        return problems;
    }
}
