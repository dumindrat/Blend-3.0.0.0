const cancelHandlers = new WeakMap();

export function show(dialog, callback, cancelMethod) {
    if (!dialog.open) {
        const cancel = event => {
            event.preventDefault();
            callback.invokeMethodAsync(cancelMethod);
        };
        cancelHandlers.set(dialog, cancel);
        dialog.addEventListener("cancel", cancel);
        dialog.showModal();
    }
}

export function close(dialog) {
    const cancel = cancelHandlers.get(dialog);
    if (cancel) {
        dialog.removeEventListener("cancel", cancel);
        cancelHandlers.delete(dialog);
    }
    if (dialog.open) {
        dialog.close();
    }
}
