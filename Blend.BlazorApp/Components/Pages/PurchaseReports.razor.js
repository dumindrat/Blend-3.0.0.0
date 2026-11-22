const tabs = new Map();
let nextTabId = 0;

export function bindGenerate(button, callback, generateMethod, blockedMethod) {
    let pending = false;
    button.addEventListener("click", async () => {
        if (button.disabled || pending) return;
        const tab = window.open("about:blank", "_blank");
        if (!tab) {
            try { await callback.invokeMethodAsync(blockedMethod); }
            catch (error) { console.error(error); alert("The report tab was blocked. Allow pop-ups and try again."); }
            return;
        }
        tab.opener = null;
        const id = ++nextTabId;
        tabs.set(id, tab);
        pending = true;
        try {
            await callback.invokeMethodAsync(generateMethod, id);
        } catch (error) {
            console.error(error);
            alert("The report could not be generated. Please try again.");
        } finally {
            const unusedTab = tabs.get(id);
            if (unusedTab) { unusedTab.close(); tabs.delete(id); }
            pending = false;
        }
    });
}

export function showPdf(id, bytes) {
    const tab = tabs.get(id);
    if (!tab || tab.closed) throw new Error("The report tab was closed before the PDF was ready.");
    const url = URL.createObjectURL(new Blob([bytes], { type: "application/pdf" }));
    try {
        tab.location.replace(url);
        tabs.delete(id);
        setTimeout(() => URL.revokeObjectURL(url), 600000);
    } catch (error) {
        URL.revokeObjectURL(url);
        throw error;
    }
}
