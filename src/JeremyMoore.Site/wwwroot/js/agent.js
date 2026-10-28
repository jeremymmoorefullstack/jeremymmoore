// Glue between AskMyAi.razor and the ElevenLabs SDK. The SDK is pinned and only loads on first use.
const sdkUrl = "https://cdn.jsdelivr.net/npm/@elevenlabs/client@1.28.0/+esm";

let conversation = null;

export async function available() {
    try {
        const response = await fetch("api/agent/status", { cache: "no-store" });
        return response.ok && (await response.json()).available === true;
    } catch {
        return false;
    }
}

const connectTimeoutMs = 20000;
const silentMicMs = 8000;

// Each start* returns "live", or why it couldn't: "microphone", "busy", "timeout" or "unavailable".
export async function startText(dotnet, context) {
    const session = await requestSession("text");
    if (session.error) return session.error;

    const typing = "The visitor is typing in a chat window, not talking. Write normally: short paragraphs, "
        + "and give the email as jeremy@jeremymmoore.com.";

    return await open(dotnet, "text", {
        signedUrl: session.signedUrl,
        connectionType: "websocket",
        overrides: { conversation: { textOnly: true } },
    }, context ? `${typing}\n\nEarlier in this conversation:\n${context}` : typing);
}

export async function startVoice(dotnet, context) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach(track => track.stop());
    } catch {
        return "microphone";
    }

    const session = await requestSession("voice");
    if (session.error) return session.error;

    return await open(dotnet, "voice", { conversationToken: session.token, connectionType: "webrtc" },
        context && `Earlier in this conversation (typed):\n${context}`);
}

export function send(text) {
    conversation?.sendUserMessage(text);
}

export async function stop() {
    const current = conversation;
    conversation = null;
    await current?.endSession();
}

export function scrollToEnd(element) {
    if (element) element.scrollTop = element.scrollHeight;
}

async function requestSession(kind) {
    try {
        const response = await fetch(`api/agent/session${kind === "text" ? "?mode=text" : ""}`, { method: "POST" });
        if (response.status === 429) return { error: "busy" };
        if (!response.ok) return { error: "unavailable" };
        return await response.json();
    } catch {
        return { error: "unavailable" };
    }
}

async function open(dotnet, kind, options, context) {
    await stop();

    // In a typed chat the visitor has already asked a question, so the spoken-style greeting is skipped.
    let greeted;
    const greeting = new Promise(resolve => { greeted = resolve; });
    let skipGreeting = kind === "text";

    try {
        const { Conversation } = await import(sdkUrl);
        const starting = Conversation.startSession({
            ...options,
            // Typed messages are already on screen; only spoken ones need a transcript line.
            onMessage: ({ source, message }) => {
                if (skipGreeting && source === "ai") {
                    skipGreeting = false;
                    greeted();
                    return;
                }
                if (source === "ai" || kind === "voice") dotnet.invokeMethodAsync("OnMessage", source, withoutVoiceCues(message));
            },
            onModeChange: ({ mode: speaking }) => dotnet.invokeMethodAsync("OnModeChanged", speaking),
            onDisconnect: () => {
                conversation = null;
                dotnet.invokeMethodAsync("OnEnded", kind);
            },
            onError: message => console.warn("Agent:", message),
        });

        // Never leave the visitor on "Connecting" forever; a late connection is closed as soon as it lands.
        const started = await Promise.race([starting, delay(connectTimeoutMs).then(() => null)]);
        if (!started) {
            starting.then(late => late.endSession()).catch(() => {});
            return "timeout";
        }
        conversation = started;

        if (kind === "voice") watchMicrophone(dotnet, started);

        if (kind === "text") {
            await Promise.race([greeting, new Promise(resolve => setTimeout(resolve, 4000))]);
            skipGreeting = false;
        }

        // A reconnect starts a fresh session, so it gets the conversation so far.
        if (context) conversation.sendContextualUpdate(context);
        return "live";
    } catch (error) {
        console.warn("Agent failed to start:", error);
        conversation = null;
        return "unavailable";
    }
}

// The browser can grant the mic and still send silence (wrong input device, OS-level block, muted headset).
// If nothing registers soon after connecting, tell the visitor instead of letting the agent talk to no one.
function watchMicrophone(dotnet, current) {
    const startedAt = Date.now();
    const timer = setInterval(async () => {
        if (conversation !== current) return clearInterval(timer);
        try {
            if (await current.getInputVolume() > 0.05) return clearInterval(timer);
        } catch {
            return clearInterval(timer);
        }
        if (Date.now() - startedAt > silentMicMs) {
            clearInterval(timer);
            dotnet.invokeMethodAsync("OnNoMicrophoneInput");
        }
    }, 500);
}

// Expressive voices get tone cues like "[warm]" or "[friendly]"; they shape the audio, not the transcript.
function withoutVoiceCues(text) {
    return text.replace(/\[[a-z][a-z \-]{0,24}\]\s*/gi, "").trim();
}

function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}
