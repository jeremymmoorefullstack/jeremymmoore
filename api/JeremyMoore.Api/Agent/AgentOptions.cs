namespace JeremyMoore.Api.Agent;

/// <summary>Bound from app settings under <c>Agent__*</c>. Secrets live in the Static Web App's settings, never in the repo.</summary>
public sealed class AgentOptions
{
    public const string Section = "Agent";

    public string? ElevenLabsApiKey { get; set; }
    public string? AgentId { get; set; }

    /// <summary>Shared secret the ElevenLabs webhook tools send in the <c>x-agent-secret</c> header.</summary>
    public string? ToolSecret { get; set; }

    public string? ResendApiKey { get; set; }
    public string MessageTo { get; set; } = "jeremy@jeremymmoore.com";
    public string MessageFrom { get; set; } = "jeremymmoore.com agent <agent@jeremymmoore.com>";

    // Text chats reconnect after idle gaps, so a visitor can need more than one session per conversation.
    public int SessionsPerVisitorPerHour { get; set; } = 6;
    public int SessionsPerDay { get; set; } = 40;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ElevenLabsApiKey) && !string.IsNullOrWhiteSpace(AgentId);
}
