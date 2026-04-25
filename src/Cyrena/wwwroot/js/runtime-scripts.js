/**
 * Cyrena.Components.Runtime - JavaScript Interop
 * Provides runtime initialization, updates, disposal, and clipboard paste handling for Blazor components.
 */
(function () {
    'use strict';

    if (!window.Cyrena) {
        window.Cyrena = {};
    }

    if (!window.Cyrena.Runtime) {
        window.Cyrena.Runtime = {};
    }

    const instances = new Map();
    const pasteHandlers = new Map();

    /**
     * Initializes a runtime instance for the given element.
     * @param {string} elementId - The DOM element ID to bind to.
     * @param {DotNetObjectReference} dotNetRef - The .NET object reference for callbacks.
     */
    window.Cyrena.Runtime.initializeRuntime = function (elementId, dotNetRef) {
        if (!elementId) {
            console.error('Cyrena.Runtime: elementId is required.');
            return;
        }

        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Cyrena.Runtime: Element with id '${elementId}' not found.`);
            return;
        }

        if (instances.has(elementId)) {
            console.warn(`Cyrena.Runtime: Instance for '${elementId}' already exists. Disposing old instance.`);
            window.Cyrena.Runtime.disposeRuntime(elementId);
        }

        instances.set(elementId, {
            element: element,
            dotNetRef: dotNetRef,
            data: null
        });

        console.log(`Cyrena.Runtime: Initialized for element '${elementId}'.`);
    };

    /**
     * Updates the runtime instance with new data and notifies .NET.
     * @param {string} elementId - The DOM element ID.
     * @param {object} data - The data payload to send to .NET.
     */
    window.Cyrena.Runtime.updateRuntime = function (elementId, data) {
        const instance = instances.get(elementId);
        if (!instance) {
            console.error(`Cyrena.Runtime: No instance found for element '${elementId}'.`);
            return;
        }

        instance.data = data;

        if (instance.dotNetRef) {
            instance.dotNetRef.invokeMethodAsync('OnRuntimeUpdated', data)
                .catch(err => console.error(`Cyrena.Runtime: Error invoking OnRuntimeUpdated for '${elementId}':`, err));
        }
    };

    /**
     * Disposes the runtime instance and cleans up resources.
     * @param {string} elementId - The DOM element ID.
     */
    window.Cyrena.Runtime.disposeRuntime = function (elementId) {
        const instance = instances.get(elementId);
        if (!instance) {
            console.warn(`Cyrena.Runtime: No instance to dispose for element '${elementId}'.`);
            return;
        }

        if (instance.dotNetRef) {
            instance.dotNetRef.dispose();
        }

        instances.delete(elementId);
        console.log(`Cyrena.Runtime: Disposed instance for element '${elementId}'.`);
    };

    /**
     * Registers a paste handler on a textarea to capture pasted images (e.g. screenshots).
     * @param {HTMLElement} textarea - The textarea element to attach the handler to.
     * @param {DotNetObjectReference} dotNetHelper - The .NET object reference for callbacks.
     */
    window.Cyrena.Runtime.registerChatPasteHandler = function (textarea, dotNetHelper) {
        if (!textarea) {
            console.error('Cyrena.Runtime: textarea element is required for registerChatPasteHandler.');
            return;
        }

        if (pasteHandlers.has(textarea)) {
            console.warn('Cyrena.Runtime: Paste handler already registered for this textarea.');
            return;
        }

        const handler = async function (e) {
            const items = e.clipboardData?.items;
            if (!items) return;

            for (let i = 0; i < items.length; i++) {
                const item = items[i];
                if (item.type.indexOf('image') === 0) {
                    e.preventDefault();
                    const blob = item.getAsFile();
                    if (!blob) continue;

                    const reader = new FileReader();
                    reader.onload = function (event) {
                        const base64 = event.target.result;
                        dotNetHelper.invokeMethodAsync('OnImagePasted', base64, item.type)
                            .catch(err => console.error('Cyrena.Runtime: Error invoking OnImagePasted:', err));
                    };
                    reader.readAsDataURL(blob);
                }
            }
        };

        textarea.addEventListener('paste', handler);
        pasteHandlers.set(textarea, handler);
        console.log('Cyrena.Runtime: Paste handler registered for textarea.');
    };

    /**
     * Unregisters the paste handler from a textarea.
     * @param {HTMLElement} textarea - The textarea element to remove the handler from.
     */
    window.Cyrena.Runtime.unregisterChatPasteHandler = function (textarea) {
        if (!textarea) return;

        const handler = pasteHandlers.get(textarea);
        if (!handler) {
            console.warn('Cyrena.Runtime: No paste handler to unregister for this textarea.');
            return;
        }

        textarea.removeEventListener('paste', handler);
        pasteHandlers.delete(textarea);
        console.log('Cyrena.Runtime: Paste handler unregistered for textarea.');
    };

})();
