(() => {
    let pendingSelector = null;

    const applyModule = (selector) => {
        const storageKey = selector.dataset.moduleStorageKey;
        let selectedModuleId = selector.value;
        if (storageKey) {
            const savedModuleId = localStorage.getItem(storageKey);
            if (savedModuleId && Array.from(selector.options).some(option => option.value === savedModuleId)) {
                selectedModuleId = savedModuleId;
            }
        }

        selector.value = selectedModuleId;
        selector.dataset.previousModuleId = selectedModuleId;
        document.querySelectorAll("[data-module-id]").forEach(element => {
            element.hidden = element.dataset.moduleId !== selectedModuleId;
        });

        document.querySelectorAll("[data-module-group]").forEach(group => {
            group.hidden = !group.querySelector("a[data-module-id]:not([hidden])");
        });
    };

    const confirmationDialog = () => document.getElementById("module-switch-confirmation");
    const cancelModuleChange = () => {
        if (pendingSelector) {
            pendingSelector.value = pendingSelector.dataset.previousModuleId;
            pendingSelector = null;
        }
    };

    document.addEventListener("change", event => {
        const selector = event.target.closest("[data-module-selector]");
        if (!selector || selector.value === selector.dataset.previousModuleId) {
            return;
        }

        pendingSelector = selector;
        const destinationModule = selector.selectedOptions[0]?.textContent?.trim() ?? "the selected module";
        document.getElementById("module-switch-destination").textContent = destinationModule;
        confirmationDialog().showModal();
    });

    document.addEventListener("click", event => {
        if (event.target.closest("[data-module-switch-cancel]")) {
            confirmationDialog().close();
            cancelModuleChange();
            return;
        }

        if (!event.target.closest("[data-module-switch-confirm]") || !pendingSelector) {
            return;
        }

        const storageKey = pendingSelector.dataset.moduleStorageKey;
        if (storageKey) {
            localStorage.setItem(storageKey, pendingSelector.value);
        }
        pendingSelector.dataset.previousModuleId = pendingSelector.value;
        confirmationDialog().close();
        pendingSelector = null;
        window.location.assign("/");
    });

    document.addEventListener("cancel", event => {
        if (event.target.id === "module-switch-confirmation") {
            cancelModuleChange();
        }
    }, true);

    const initialize = () => document.querySelectorAll("[data-module-selector]").forEach(applyModule);
    document.addEventListener("DOMContentLoaded", initialize);
    document.addEventListener("enhancedload", initialize);
    initialize();
})();
