// Voice agent glue for TalkToAgent.razor. The ElevenLabs SDK is pinned and only loaded when someone clicks.
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

// Returns "live", or why it couldn't start: "microphone", "busy" or "unavailable".
export async function start(dotnet) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach(track => track.stop());
    } catch {
        return "microphone";
    }

    let token;
    try {
        const response = await fetch("api/agent/session", { method: "POST" });
        if (response.status === 429) return "busy";
        if (!response.ok) return "unavailable";
        ({ token } = await response.json());
    } catch {
        return "unavailable";
    }

    try {
        const { Conversation } = await import(sdkUrl);
        conversation = await Conversation.startSession({
            conversationToken: token,
            connectionType: "webrtc",
            onModeChange: ({ mode }) => dotnet.invokeMethodAsync("OnModeChanged", mode),
            onDisconnect: () => {
                conversation = null;
                dotnet.invokeMethodAsync("OnEnded");
            },
            onError: message => console.warn("Voice agent:", message),
        });
        return "live";
    } catch (error) {
        console.warn("Voice agent failed to start:", error);
        return "unavailable";
    }
}

export async function stop() {
    const current = conversation;
    conversation = null;
    await current?.endSession();
}
