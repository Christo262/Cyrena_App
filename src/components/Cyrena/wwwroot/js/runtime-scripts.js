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
    const dropHandlers = new Map();

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
            const files = e.clipboardData?.files;

            if (files && files.length > 0) {
                e.preventDefault();

                for (const file of files) {
                    readAndSendFile(file, dotNetHelper, 'pasted');
                }

                return;
            }

            // Optional fallback for pasted screenshots/images where files is empty
            const items = e.clipboardData?.items;
            if (!items) return;

            for (let i = 0; i < items.length; i++) {
                const item = items[i];

                if (item.kind === 'file') {
                    const file = item.getAsFile();
                    if (!file) continue;

                    e.preventDefault();

                    readAndSendFile(file, dotNetHelper, 'pasted');
                }
            }
        };

        textarea.addEventListener('paste', handler);
        pasteHandlers.set(textarea, handler);
        //console.log('Cyrena.Runtime: Paste handler registered for textarea.');
    };

    function readAndSendFile(file, dotNetHelper, source) {
        const reader = new FileReader();

        reader.onload = function (event) {
            dotNetHelper.invokeMethodAsync(
                'OnFilePasted',
                event.target.result,
                file.name || `${source}-file`,
                file.type || 'application/octet-stream',
                file.size
            ).catch(err => console.error(`Cyrena.Runtime: Error invoking OnFilePasted from ${source}:`, err));
        };

        reader.readAsDataURL(file);
    };

    window.Cyrena.Runtime.registerChatDropHandler = function (element, dotNetHelper) {
        if (!element) {
            console.error('Cyrena.Runtime: element is required for registerChatDropHandler.');
            return;
        }

        if (dropHandlers.has(element)) {
            console.warn('Cyrena.Runtime: Drop handler already registered for this element.');
            return;
        }

        const dragOverHandler = function (e) {
            if (!e.dataTransfer?.types?.includes('Files')) return;

            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
            element.classList.add('cyrena-file-drag-over');
        };

        const dragLeaveHandler = function (e) {
            if (!element.contains(e.relatedTarget)) {
                element.classList.remove('cyrena-file-drag-over');
            }
        };

        const dropHandler = function (e) {
            const files = e.dataTransfer?.files;

            if (!files || files.length === 0) return;

            e.preventDefault();
            element.classList.remove('cyrena-file-drag-over');

            for (const file of files) {
                readAndSendFile(file, dotNetHelper, 'dropped');
            }
        };

        element.addEventListener('dragover', dragOverHandler);
        element.addEventListener('dragleave', dragLeaveHandler);
        element.addEventListener('drop', dropHandler);

        dropHandlers.set(element, {
            dragOverHandler,
            dragLeaveHandler,
            dropHandler
        });
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
        //console.log('Cyrena.Runtime: Paste handler unregistered for textarea.');
    };

    window.Cyrena.Runtime.unregisterChatDropHandler = function (element) {
        if (!element) return;

        const handlers = dropHandlers.get(element);
        if (!handlers) return;

        element.removeEventListener('dragover', handlers.dragOverHandler);
        element.removeEventListener('dragleave', handlers.dragLeaveHandler);
        element.removeEventListener('drop', handlers.dropHandler);

        element.classList.remove('cyrena-file-drag-over');
        dropHandlers.delete(element);
    };

})();

(function () {
    'use strict';

    // ── Inline SVGs (no external icon font needed) ──
    const COPY_SVG = '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16" style="vertical-align:text-bottom;margin-right:4px;"><path d="M4 1.5a.5.5 0 0 1 .5-.5h6a.5.5 0 0 1 .5.5v1.5h1.5a.5.5 0 0 1 .5.5v10a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V3.5a.5.5 0 0 1 .5-.5H4V1.5zM5 2v1h6V2H5z"/><path d="M3.5 3a.5.5 0 0 0-.5.5v10a.5.5 0 0 0 .5.5h9a.5.5 0 0 0 .5-.5v-10a.5.5 0 0 0-.5-.5h-1.5v1.5a.5.5 0 0 1-.5.5h-7a.5.5 0 0 1-.5-.5V3h-1.5z"/></svg>';
    const CHECK_SVG = '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" fill="currentColor" viewBox="0 0 16 16" style="vertical-align:text-bottom;margin-right:4px;"><path d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-3.99 4.99a.75.75 0 0 1-1.08.02L4.324 8.384a.75.75 0 1 1 1.06-1.06l2.094 2.093 3.473-4.425a.267.267 0 0 1 .02-.022z"/></svg>';

    /**
     * Attaches a copy button to a single <pre><code> block.
     */
    function attachCopyButton(codeBlock) {
        const pre = codeBlock.parentElement;
        if (!pre || pre.querySelector('.btn-copy-code')) return; // already done

        // Ensure the <pre> is positioned relatively so the absolute button anchors to it
        if (!pre.classList.contains('position-relative')) {
            pre.style.position = 'relative';
        }

        // ── Create the button ──
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn-copy-code';
        btn.innerHTML = COPY_SVG + 'Copy';
        btn.setAttribute('aria-label', 'Copy code to clipboard');

        // ── Click handler ──
        btn.addEventListener('click', async function (e) {
            e.stopPropagation();
            const text = codeBlock.textContent || '';
            try {
                await navigator.clipboard.writeText(text);
                btn.innerHTML = CHECK_SVG + 'Copied!';
                btn.style.borderColor = 'rgba(86, 204, 157, 0.8)';
                btn.style.color = '#56cc9d';
                setTimeout(() => {
                    btn.innerHTML = COPY_SVG + 'Copy';
                    btn.style.borderColor = 'rgba(255,255,255,0.3)';
                    btn.style.color = 'rgba(255,255,255,0.8)';
                }, 2000);
            } catch (err) {
                console.error('Failed to copy code:', err);
                btn.textContent = 'Error';
            }
        });

        pre.appendChild(btn);
    }

    /**
     * Initializes copy buttons within a root element (defaults to document).
     */
    function initCodeCopyButtons(root) {
        root = root || document;
        root.querySelectorAll('pre code').forEach(attachCopyButton);
    }

    // ── Run on DOM ready ──
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => initCodeCopyButtons());
    } else {
        initCodeCopyButtons();
    }

    // ── Watch for dynamically added code blocks (Blazor, streaming, etc.) ──
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            mutation.addedNodes.forEach(function (node) {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    if (node.matches && node.matches('pre code')) {
                        attachCopyButton(node);
                    } else if (node.querySelectorAll) {
                        initCodeCopyButtons(node);
                    }
                }
            });
        });
    });
    observer.observe(document.body, { childList: true, subtree: true });

    // ── Expose globally for manual triggering ──
    window.Cyrena = window.Cyrena || {};
    window.Cyrena.CodeCopy = {
        init: initCodeCopyButtons,
        attach: attachCopyButton
    };
})();