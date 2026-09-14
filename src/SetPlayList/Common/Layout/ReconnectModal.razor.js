// Set up event handlers
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

// Firefox closes the WebSocket the moment a navigation starts, so the beginning of
// a navigation is indistinguishable from a dropped connection and the dialog flashes
// on screen for a few dozen milliseconds. See https://github.com/dotnet/aspnetcore/issues/40425
// Delaying the dialog filters that out: a navigation replaces the page well before the
// delay elapses, while a genuine drop is still unresolved.
const showDelayMs = 1000;
let showTimer;

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show") {
        // "retrying" follows "show" immediately, so only "hide" may cancel the timer.
        if (showTimer === undefined) {
            showTimer = setTimeout(() => reconnectModal.showModal(), showDelayMs);
        }
    } else if (event.detail.state === "hide") {
        clearTimeout(showTimer);
        showTimer = undefined;
        reconnectModal.close();
    } else if (event.detail.state === "failed") {
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    } else if (event.detail.state === "rejected") {
        location.reload();
    }
}

async function retry() {
    document.removeEventListener("visibilitychange", retryWhenDocumentBecomesVisible);

    try {
        // Reconnect will asynchronously return:
        // - true to mean success
        // - false to mean we reached the server, but it rejected the connection (e.g., unknown circuit ID)
        // - exception to mean we didn't reach the server (this can be sync or async)
        const successful = await Blazor.reconnect();
        if (!successful) {
            // We have been able to reach the server, but the circuit is no longer available.
            // We'll reload the page so the user can continue using the app as quickly as possible.
            const resumeSuccessful = await Blazor.resumeCircuit();
            if (!resumeSuccessful) {
                location.reload();
            } else {
                reconnectModal.close();
            }
        }
    } catch (err) {
        // We got an exception, server is currently unavailable
        document.addEventListener("visibilitychange", retryWhenDocumentBecomesVisible);
    }
}

async function resume() {
    try {
        const successful = await Blazor.resumeCircuit();
        if (!successful) {
            location.reload();
        }
    } catch {
        if (reconnectModal.classList.contains("components-reconnect-paused")) {
            reconnectModal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
        } else if (reconnectModal.classList.contains("components-pause")) {
            reconnectModal.classList.replace("components-pause", "components-resume-failed");
        } else {
            reconnectModal.classList.add("components-reconnect-resume-failed");
        }
    }
}

async function retryWhenDocumentBecomesVisible() {
    if (document.visibilityState === "visible") {
        await retry();
    }
}
