# Voice agent

"Talk to my AI agent" on jeremymmoore.com. Everything here runs on personal accounts.

| File | What |
|------|------|
| `system-prompt.md` | The agent's instructions and first message |
| `knowledge.md` | Generated from `ProfileContent.cs`; don't edit by hand. Regenerate with `dotnet run --project tools/JeremyMoore.Prerender -- agent agent` (a test fails if it's stale). |
| `notes.md` | Hand-written answers the site doesn't cover |

## How it fits together

1. The site calls `GET /api/agent/status`. The button only shows when the API is configured.
2. Typed questions call `POST /api/agent/session?mode=text` (a signed WebSocket URL, text-only override);
   the mic button calls `POST /api/agent/session` (a single-use WebRTC token). The C# function rate-limits
   by visitor either way, and the API key stays server-side.
3. The browser loads `@elevenlabs/client` (pinned) and starts the session with that token.
4. When a visitor wants to reach Jeremy, the agent calls the `send_message` webhook tool:
   `POST https://jeremymmoore.com/api/agent/tools/send-message` with header `x-agent-secret`.
   The function validates the fields and emails jeremy@jeremymmoore.com through Resend, Reply-To the visitor.

## ElevenLabs setup (personal account)

- Agent: authentication **on** (private), max conversation duration **300 s**, the **Text only**
  override allowed (the site hides the spoken greeting in typed chats), end after **120 s** of silence, first message and prompt from
  `system-prompt.md`, knowledge base `knowledge.md` + `notes.md`.
- Allowed origins: `jeremymmoore.com` only.
- Workspace spend cap set in billing.
- Webhook tool `send_message`: POST to the URL above, header `x-agent-secret` from a secret, JSON body
  `name`, `email`, `message` (required) and `company` (optional), each with a short description.

## Static Web App settings (Azure portal → jeremymmoore → Environment variables)

| Name | Value |
|------|-------|
| `Agent__ElevenLabsApiKey` | ElevenLabs API key (scoped to Agents) |
| `Agent__AgentId` | `agent_…` |
| `Agent__ToolSecret` | Long random string, same as the tool's header secret |
| `Agent__ResendApiKey` | Resend API key (sending domain jeremymmoore.com) |

Optional: `Agent__SessionsPerVisitorPerHour` (default 6), `Agent__SessionsPerDay` (default 40).
