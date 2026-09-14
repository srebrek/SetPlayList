// Set up event handlers
const reconnectModal = document.getElementById("components-reconnect-modal");
reconnectModal.addEventListener("components-reconnect-state-changed", handleReconnectStateChanged);

const retryButton = document.getElementById("components-reconnect-button");
retryButton.addEventListener("click", retry);

const resumeButton = document.getElementById("components-resume-button");
resumeButton.addEventListener("click", resume);

// Firefox closes the WebSocket the moment a navigation starts, so Blazor reports a
// dropped connection while the old page is still on screen and the dialog flashes up
// mid-navigation. See https://github.com/dotnet/aspnetcore/issues/40425
//
// "beforeunload" fires just before that happens (measured: ~6ms earlier), so it tells
// a navigation apart from a real outage. The short delay on top covers the case where
// no unload event arrives at all.
const showDelayMs = 1000;
let showTimer;
let navigatingAway = false;

function markNavigating() {
    navigatingAway = true;
    clearTimeout(showTimer);
    showTimer = undefined;
    // A navigation can still be cancelled, leaving this document alive; stop
    // suppressing the dialog after a while so real outages are still reported.
    setTimeout(() => { navigatingAway = false; }, 10000);
}

window.addEventListener("beforeunload", markNavigating);
window.addEventListener("pagehide", markNavigating);

function handleReconnectStateChanged(event) {
    if (event.detail.state === "show" || event.detail.state === "retrying") {
        // "retrying" repeats, so only arm the timer once.
        if (navigatingAway || showTimer !== undefined || reconnectModal.open) {
            return;
        }
        showTimer = setTimeout(() => {
            if (!navigatingAway) {
                reconnectModal.showModal();
            }
        }, showDelayMs);
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
