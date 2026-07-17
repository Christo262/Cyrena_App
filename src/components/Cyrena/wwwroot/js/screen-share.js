/**
 * Cyrena.ScreenShare - Browser screen capture interop (stateless adapter)
 *
 * ARCHITECTURE
 *   State lives in .NET (Blazor). JS is a thin adapter over the browser
 *   MediaDevices API, holding the only real references to live MediaStreams
 *   in a Map keyed by opaque tokens. .NET generates no assumptions about
 *   what a "screen" is -- it just tracks tokens, metadata, and chat
 *   associations.
 *
 *   One document, many concurrent streams: chat A can be sharing monitor 2
 *   while chat B shares a tab, each addressed independently by its token.
 *
 * TOKEN MODEL
 *   A token is a string (UUID v4 from crypto.randomUUID(), with a
 *   Math.random fallback for non-secure contexts). Tokens are how .NET
 *   addresses a specific stream when calling back. The browser has no
 *   concept of a stable screen ID across the getDisplayMedia boundary --
 *   the only way to get a screen is to ask, and what comes back is opaque
 *   to JS. The token is JS's contribution to making that opaque handle
 *   addressable.
 *
 * ENUMERATION
 *   The browser does NOT expose screen enumeration without user gesture.
 *   There is no getScreens() API, enumerateDevices() returns cameras/mics
 *   only, and window.screen gives one viewport number. The OS picker is
 *   the only enumeration UI. We surface the user's previous picks as
 *   labels via listStreams() so .NET can render a re-pick menu; actually
 *   narrowing the picker to a specific surface (e.g. "monitor only") is
 *   done with the displaySurface hint, which the browser may or may not
 *   honor.
 *
 * .NET SURFACE (callable via IJSRuntime):
 *
 *   Cyrena.ScreenShare.isSupported()
 *       -> boolean
 *
 *   Cyrena.ScreenShare.requestShare(dotNetRef, preferences?)
 *       -> { success, token, label, displaySurface, width, height, cancelled?, error? }
 *     preferences: optional { displaySurface?: 'monitor'|'window'|'browser', audio?: boolean }
 *     Always shows the OS picker. The picked stream is stored in the Map
 *     under a fresh token. Does NOT capture. Fires OnStreamPicked on
 *     dotNetRef with the token.
 *
 *     Convenience wrapper around startShareSync + awaitPending. The
 *     two-step variant exists for hosts where the call originates in a
 *     user-activation stack frame (e.g. a Blazor button click inside
 *     Photino/WebView2); the wrapper is fine for hosts that don't
 *     enforce transient user activation on the getDisplayMedia call.
 *
 *   Cyrena.ScreenShare.startShareSync(dotNetRef, preferences?)
 *       -> { success, token } | { success: false, error }
 *     SYNCHRONOUS. Call via IJSInProcessRuntime from a user-gesture
 *     handler. Fires getDisplayMedia() in the same task as the gesture,
 *     stores the picker promise in _pendingPicks, returns immediately
 *     with a token. The token is the input to awaitPending().
 *
 *   Cyrena.ScreenShare.awaitPending(token)
 *       -> { success, token, label, displaySurface, width, height, cancelled?, error? }
 *     Awaits the picker promise stored by startShareSync. Same shape
 *     as requestShare. Call via IJSRuntime (async) after the sync
 *     startShareSync call returns.
 *
 *   Cyrena.ScreenShare.awaitPendingWithTimeout(token, timeoutMs)
 *       -> same shape as awaitPending, or { success: false, cancelled: true }
 *     Same as awaitPending but with a hard timeout. Defaults to 60s.
 *
 *   Cyrena.ScreenShare.captureStream(dotNetRef, token)
 *       -> { success, token, dataUrl, fileName, mimeType, size, label, displaySurface, sourceLost?, error? }
 *     Captures one frame from the stream identified by `token`. Fires
 *     OnStreamCaptured on dotNetRef with the same payload. If the stream
 *     has ended (user revoked, source went away), returns sourceLost:true
 *     and the token is already removed from the Map.
 *
 *   Cyrena.ScreenShare.changeStream(dotNetRef, token, preferences?)
 *       -> { success, token, ... }   (same shape as requestShare)
 *     Stops the stream for the given token, then opens the picker. The
 *     NEW stream gets a NEW token; .NET is expected to replace the
 *     old token with the new one in its state. The old token is
 *     invalidated (Map entry removed, track stopped).
 *
 *   Cyrena.ScreenShare.releaseStream(dotNetRef, token)
 *       -> { released: boolean }
 *     Stops the track and removes the Map entry. Safe to call with an
 *     unknown token (returns released:false). Fires nothing on dotNetRef --
 *     caller is expected to update its own state. Use when .NET knows
 *     it's done (component disposed, user clicked "Stop sharing" in app).
 *
 *   Cyrena.ScreenShare.releaseAllStreams()
 *       -> { released: number }
 *     Bulk teardown. No .NET callbacks fired. Use on page unload if you
 *     care; browsers clean up the OS share indicator automatically anyway.
 *
 *   Cyrena.ScreenShare.listStreams()
 *       -> [ { token, label, displaySurface, width, height, readyState } ]
 *     Snapshot of currently-held streams. Use for state reconciliation
 *     after a Blazor Server reconnect, or to seed a re-pick UI.
 *
 * CALLBACKS (invoked on the dotNetRef passed to the originating call):
 *
 *   OnStreamPicked(token, label, displaySurface, width, height)
 *       Fires once per successful requestShare/changeStream. The "current"
 *       dotNetRef is whichever .NET caller most recently invoked any
 *       method on this module. If .NET needs per-chat routing it should
 *       pass the chat's own DotNetObjectReference on every call.
 *
 *   OnStreamCaptured(token, dataUrl, fileName, mimeType, size, label, displaySurface)
 *       Fires after a successful captureStream. Mirrors the
 *       paste/drop file contract.
 *
 *   OnStreamEnded(token)
 *       Fires when a stream's track fires 'ended' (user clicked the
 *       browser's "Stop sharing" indicator, source went away). Token is
 *       already removed from the Map at this point.
 *
 * LIFETIME
 *   The Map survives for the lifetime of the document. Blazor navigation
 *   that doesn't unload the doc (SPA route change) does NOT kill streams;
 *   this is what lets the same screen share span multiple chat views.
 *   .NET is responsible for calling releaseStream() when its component
 *   disposes, or releaseAllStreams() on permanent teardown.
 */
(function () {
    'use strict';

    if (!window.Cyrena) {
        window.Cyrena = {};
    }

    if (!window.Cyrena.ScreenShare) {
        window.Cyrena.ScreenShare = {};
    }

    // ----- module-scope state ---------------------------------------------

    /**
     * The Map of active streams. Keyed by token (string).
     * Value shape:
     *   {
     *     stream:        MediaStream
     *     track:         MediaStreamTrack
     *     imageCapture:  ImageCapture | null
     *     label:         string   -- track.label as reported by the browser
     *     displaySurface: 'monitor'|'window'|'browser'|'unknown'
     *     width:         number
     *     height:        number
     *     dotNetRef:     DotNetObjectReference | null
     *   }
     */
    const _streams = new Map();

    /**
     * Pending picker promises keyed by token. Populated by
     * startShareSync() when getDisplayMedia() is called synchronously
     * (inside the user-gesture handler on the Blazor/Photino side).
     * Resolved (awaited) by awaitPending() on a later JS interop call
     * that crosses the .NET await boundary. This split exists so the
     * getDisplayMedia call happens in the user-activation stack frame
     * and not after a microtask yield, which is what Chromium requires
     * for WebView2/Photino (and a strict reading of the spec requires
     * for browser tab captures). .NET should call startShareSync
     * through IJSInProcessRuntime (synchronous) and awaitPending
     * through IJSRuntime (async).
     */
    const _pendingPicks = new Map();

    /**
     * Most-recently-passed DotNetObjectReference. Used as a fallback for
     * the OnStreamEnded callback when the originating .NET caller is
     * already gone (component disposed but stream still alive in the
     * browser for a moment before its track fires 'ended'). .NET should
     * still pass its own ref on every call if it wants per-chat routing.
     */
    let _lastDotNetRef = null;

    // ----- public API -----------------------------------------------------

    window.Cyrena.ScreenShare.isSupported = function () {
        return !!(navigator.mediaDevices && navigator.mediaDevices.getDisplayMedia);
    };

    /**
     * Prompts the OS picker, registers the resulting stream in the Map
     * under a fresh token, fires OnStreamPicked. Does NOT capture.
     *
     * Async convenience wrapper: equivalent to calling
     *   startShareSync(preferences) -> awaitPending(token)
     * in two steps. Use the two-step variant from .NET when the
     * request originates from a real user gesture (Blazor button click
     * inside Photino/WebView2), because the await inside this single
     * async function will cross a microtask boundary and lose the
     * user-activation that getDisplayMedia requires.
     */
    window.Cyrena.ScreenShare.requestShare = async function (dotNetRef, preferences) {
        if (!window.Cyrena.ScreenShare.isSupported()) {
            return {
                success: false,
                error: 'Screen capture is not supported in this browser.'
            };
        }

        if (dotNetRef) {
            _lastDotNetRef = dotNetRef;
        }

        const sync = window.Cyrena.ScreenShare.startShareSync(dotNetRef, preferences);
        if (!sync.success) {
            return sync;
        }
        return await window.Cyrena.ScreenShare.awaitPending(sync.token);
    };

    /**
     * Synchronous entry point. MUST be called via IJSInProcessRuntime
     * (synchronous JS interop) from a handler that is itself running
     * inside a user-activation stack frame (a Blazor OnClick handler,
     * for example). The function is intentionally non-async: it calls
     * navigator.mediaDevices.getDisplayMedia() synchronously, stores
     * the returned promise in _pendingPicks keyed by a fresh token,
     * and returns { success, token } to .NET immediately. The promise
     * is awaited later via awaitPending() on a separate JS interop
     * call that crosses the .NET await boundary.
     *
     * Why: getDisplayMedia() must be invoked inside a task that has
     * transient user activation. When C# awaits InvokeAsync, the
     * dispatcher yields and the activation is gone by the time the
     * JS resumes. Splitting the call into a sync start + later await
     * keeps the getDisplayMedia invocation in the same task as the
     * original click.
     *
     * Returns:
     *   { success: true,  token }    on a successful start (user has
     *                                not yet picked; the picker is up)
     *   { success: false, error }    if the browser doesn't support
     *                                getDisplayMedia, or the immediate
     *                                pre-check fails. The .NET side
     *                                should surface this as a failure.
     *   { success: false, cancelled: true, token }   if a synchronous
     *         pre-flight check rejects (rare; usually the picker is
     *         what surfaces cancel). Kept for symmetry.
     *
     * NOTE: any pre-call work that needs an await (e.g. an async
     * capability probe) must happen before this function is invoked
     * -- it cannot happen inside.
     */
    window.Cyrena.ScreenShare.startShareSync = function (dotNetRef, preferences) {
        if (!window.Cyrena.ScreenShare.isSupported()) {
            return {
                success: false,
                error: 'Screen capture is not supported in this browser.'
            };
        }

        if (dotNetRef) {
            _lastDotNetRef = dotNetRef;
        }

        const token = generateToken();

        // Fire getDisplayMedia() synchronously. Do NOT await before this
        // call -- the call itself has to land in the same task as the
        // user-activation event for Chromium to consider the gesture
        // live. The returned promise is stored; the picker UI appears
        // as soon as the browser processes the request.
        let streamPromise;
        try {
            const video = (preferences && typeof preferences === 'object') ? {
                frameRate: 1,
                // Hint, not filter. Browser may ignore; picker still
                // shows everything. Useful for "monitor only" re-pick
                // menus: the picker pre-selects the monitor category.
                displaySurface: preferences.displaySurface || undefined
            } : {
                frameRate: 1,
                displaySurface: 'monitor'
            };

            const audio = !!(preferences && preferences.audio);

            streamPromise = navigator.mediaDevices.getDisplayMedia({
                video: video,
                audio: audio
            });
        } catch (err) {
            // Synchronous throw (rare -- e.g. permissions-policy block
            // before the picker is shown). Surface immediately.
            return {
                success: false,
                error: (err && err.message) ? err.message : 'getDisplayMedia threw synchronously.'
            };
        }

        // Wrap the raw stream promise into the same shape the old
        // pickStream produced. We do this so awaitPending returns the
        // exact same payload as requestShare.
        const pickPromise = streamPromise
            .then(function (stream) {
                return finalizePick(token, stream, preferences, dotNetRef);
            })
            .catch(function (err) {
                if (err && (err.name === 'NotAllowedError' || err.name === 'AbortError' || err.name === 'NotAllowed')) {
                    return { success: false, token: token, cancelled: true };
                }
                console.error('Cyrena.ScreenShare: getDisplayMedia rejected:', err);
                return {
                    success: false,
                    token: token,
                    error: (err && err.message) ? err.message : 'Unknown capture error.'
                };
            })
            .finally(function () {
                _pendingPicks.delete(token);
            });

        _pendingPicks.set(token, pickPromise);

        return { success: true, token: token };
    };

    /**
     * Resolves the picker promise stored by startShareSync. Returns the
     * same shape as the old requestShare result:
     *   { success, token, label, displaySurface, width, height,
     *     cancelled?, error? }
     *
     * .NET calls this on a separate InvokeAsync after the synchronous
     * startShareSync call returns. The picker is already up by the time
     * .NET makes this call; awaitPending just blocks until the user
     * picks or dismisses.
     */
    window.Cyrena.ScreenShare.awaitPending = async function (token) {
        const promise = _pendingPicks.get(token);
        if (!promise) {
            return {
                success: false,
                error: 'No pending screen share for the given token. Was startShareSync called?'
            };
        }
        return await promise;
    };

    /**
     * Resolves the picker promise stored by startShareSync with a hard
     * timeout. Used by .NET to avoid leaving the user staring at a
     * dangling "capturing..." spinner if the OS picker never resolves
     * (e.g. the user closed the underlying WebView window). The pending
     * entry is cleared either way; the user can re-pick by calling
     * startShareSync again.
     *
     * Returns the same shape as awaitPending. On timeout, returns
     * { success: false, token, cancelled: true, error: 'Picker timed out...' }.
     */
    window.Cyrena.ScreenShare.awaitPendingWithTimeout = async function (token, timeoutMs) {
        const promise = _pendingPicks.get(token);
        if (!promise) {
            return {
                success: false,
                error: 'No pending screen share for the given token. Was startShareSync called?'
            };
        }
        const ms = (typeof timeoutMs === 'number' && timeoutMs > 0) ? timeoutMs : 60000;
        let timer;
        const timeoutPromise = new Promise(function (resolve) {
            timer = setTimeout(function () {
                resolve({
                    success: false,
                    token: token,
                    cancelled: true,
                    error: 'Picker timed out after ' + ms + 'ms.'
                });
            }, ms);
        });
        try {
            const result = await Promise.race([promise, timeoutPromise]);
            return result;
        } finally {
            clearTimeout(timer);
        }
    };

    /**
     * Captures one frame from the stream identified by `token`. Fires
     * OnStreamCaptured on dotNetRef with the same payload.
     */
    window.Cyrena.ScreenShare.captureStream = async function (dotNetRef, token) {
        if (!window.Cyrena.ScreenShare.isSupported()) {
            return {
                success: false,
                error: 'Screen capture is not supported in this browser.'
            };
        }

        if (dotNetRef) {
            _lastDotNetRef = dotNetRef;
        }

        const entry = _streams.get(token);
        if (!entry) {
            return {
                success: false,
                token: token,
                sourceLost: true,
                error: 'No active stream for the given token.'
            };
        }

        if (entry.track.readyState === 'ended') {
            // Track died between the Map check and now. Clean up and bail.
            // We do NOT fire OnStreamEnded here -- the browser's own 'ended'
            // event will fire (or already did) and that path handles it.
            // We only get here on a race; the state is consistent either way.
            removeStream(token);
            return {
                success: false,
                token: token,
                sourceLost: true,
                error: 'The shared source has ended.'
            };
        }

        let blob;
        try {
            blob = await grabFrame(entry.stream, entry.track, entry.imageCapture);
        } catch (err) {
            console.error('Cyrena.ScreenShare: captureStream failed:', err);
            return {
                success: false,
                token: token,
                error: (err && err.message) ? err.message : 'Frame capture failed.'
            };
        }

        let dataUrl;
        try {
            dataUrl = await blobToDataUrl(blob);
        } catch (err) {
            console.error('Cyrena.ScreenShare: blobToDataUrl failed:', err);
            return {
                success: false,
                token: token,
                error: (err && err.message) ? err.message : 'Failed to read captured frame.'
            };
        }

        const fileName = `screenshot-${formatTimestamp(new Date())}.png`;
        const mimeType = blob.type || 'image/png';
        const size = blob.size;

        if (dotNetRef) {
            try {
                await dotNetRef.invokeMethodAsync(
                    'OnStreamCaptured',
                    token,
                    dataUrl,
                    fileName,
                    mimeType,
                    size,
                    entry.label || '',
                    entry.displaySurface || 'unknown'
                );
            } catch (err) {
                console.error('Cyrena.ScreenShare: OnStreamCaptured invocation failed:', err);
            }
        }

        return {
            success: true,
            token: token,
            dataUrl: dataUrl,
            fileName: fileName,
            mimeType: mimeType,
            size: size,
            label: entry.label || null,
            displaySurface: entry.displaySurface || 'unknown'
        };
    };

    /**
     * Stops the stream for `token`, opens the picker for a new one.
     * Returns a NEW token. .NET replaces the old token with the new one.
     */
    window.Cyrena.ScreenShare.changeStream = async function (dotNetRef, token, preferences) {
        // Release the old stream first. We do this synchronously (not in
        // a finally) so the old track is gone before the user picks a
        // replacement -- some browsers complain if you ask for a new
        // getDisplayMedia while the old one is still tracked.
        if (token && _streams.has(token)) {
            removeStream(token);
        }

        if (dotNetRef) {
            _lastDotNetRef = dotNetRef;
        }

        return await window.Cyrena.ScreenShare.requestShare(dotNetRef, preferences);
    };

    /**
     * Stops the track and removes the Map entry. Safe with unknown token.
     * Does not fire OnStreamEnded -- the caller (usually .NET) initiated
     * the release and should update its own state.
     */
    window.Cyrena.ScreenShare.releaseStream = function (dotNetRef, token) {
        if (dotNetRef) {
            _lastDotNetRef = dotNetRef;
        }
        if (!token || !_streams.has(token)) {
            return { released: false };
        }
        removeStream(token);
        return { released: true };
    };

    /**
     * Bulk teardown. No callbacks fired. Use on page unload.
     */
    window.Cyrena.ScreenShare.releaseAllStreams = function () {
        const n = _streams.size;
        // Snapshot the tokens first; removeStream mutates the Map.
        const tokens = Array.from(_streams.keys());
        for (let i = 0; i < tokens.length; i++) {
            removeStream(tokens[i]);
        }
        return { released: n };
    };

    /**
     * Snapshot of currently-held streams, for state reconciliation
     * (e.g. after a Blazor Server reconnect) and for re-pick UI seeding.
     */
    window.Cyrena.ScreenShare.listStreams = function () {
        const out = [];
        _streams.forEach(function (entry, token) {
            out.push({
                token: token,
                label: entry.label || null,
                displaySurface: entry.displaySurface || 'unknown',
                width: entry.width || 0,
                height: entry.height || 0,
                readyState: entry.track ? entry.track.readyState : 'ended'
            });
        });
        return out;
    };

    // ----- internals ------------------------------------------------------

    /**
     * Post-pick finalization. Called by startShareSync() once the
     * getDisplayMedia promise resolves with a MediaStream. Stores the
     * stream in the Map, fires OnStreamPicked, returns the result
     * envelope. Kept separate from startShareSync so the synchronous
     * user-gesture path doesn't have to await before getDisplayMedia.
     */
    async function finalizePick(token, stream, preferences, dotNetRef) {
        try {
            const track = stream.getVideoTracks()[0];
            if (!track) {
                stream.getTracks().forEach(function (t) { t.stop(); });
                return { success: false, token: token, error: 'No video track in capture stream.' };
            }

            const settings = track.getSettings();
            const label = track.label || null;
            const displaySurface = classifyDisplaySurface(settings, label);
            const width = settings.width || 0;
            const height = settings.height || 0;

            const imageCapture = (typeof ImageCapture !== 'undefined')
                ? new ImageCapture(track)
                : null;

            const entry = {
                stream: stream,
                track: track,
                imageCapture: imageCapture,
                label: label,
                displaySurface: displaySurface,
                width: width,
                height: height,
                dotNetRef: dotNetRef || null
            };

            _streams.set(token, entry);

            // The track fires 'ended' on user revocation (browser's
            // "Stop sharing" indicator) or when the source itself goes
            // away (closed window, unplugged monitor). The browser has
            // already stopped the track; we just clean up and notify.
            track.addEventListener('ended', function () {
                handleTrackEnded(token);
            });

            if (dotNetRef) {
                try {
                    await dotNetRef.invokeMethodAsync(
                        'OnStreamPicked',
                        token,
                        label || '',
                        displaySurface,
                        width,
                        height
                    );
                } catch (err) {
                    console.error('Cyrena.ScreenShare: OnStreamPicked invocation failed:', err);
                }
            }

            return {
                success: true,
                token: token,
                label: label,
                displaySurface: displaySurface,
                width: width,
                height: height
            };
        } catch (err) {
            if (err && (err.name === 'NotAllowedError' || err.name === 'AbortError')) {
                return { success: false, token: token, cancelled: true };
            }
            console.error('Cyrena.ScreenShare: finalizePick failed:', err);
            return {
                success: false,
                token: token,
                error: (err && err.message) ? err.message : 'Unknown capture error.'
            };
        }
    }

    /**
     * Removes a stream from the Map and stops its track. Idempotent.
     * Does NOT fire OnStreamEnded -- that's only for browser-initiated
     * revocation. .NET-initiated releases should use releaseStream().
     */
    function removeStream(token) {
        const entry = _streams.get(token);
        if (!entry) {
            return;
        }
        try {
            entry.stream.getTracks().forEach(function (t) {
                try { t.stop(); } catch (_) { /* ignore */ }
            });
        } catch (_) { /* ignore */ }
        _streams.delete(token);
    }

    /**
     * Fired when the user revokes via the browser chrome. We fire
     * OnStreamEnded with the token so .NET can look up its chat
     * association and update UI. The Map entry is already gone
     * (the track's own 'ended' stopped it, and removeStream was
     * called in the handler path).
     */
    function handleTrackEnded(token) {
        const entry = _streams.get(token);
        if (!entry) {
            return;
        }
        const dotNetRef = entry.dotNetRef || _lastDotNetRef;
        removeStream(token);
        if (dotNetRef) {
            try {
                dotNetRef.invokeMethodAsync('OnStreamEnded', token);
            } catch (err) {
                console.error('Cyrena.ScreenShare: OnStreamEnded invocation failed:', err);
            }
        }
    }

    /**
     * Heuristic: displaySurface in track.getSettings() is authoritative
     * when the browser reports it (Chrome/Edge do). When it's missing
     * (Firefox, some Safari builds), we fall back to sniffing the label.
     */
    function classifyDisplaySurface(settings, label) {
        if (settings && settings.displaySurface) {
            switch (settings.displaySurface) {
                case 'monitor': return 'monitor';
                case 'window': return 'window';
                case 'browser': return 'browser';
                default: return 'unknown';
            }
        }
        if (label) {
            const l = label.toLowerCase();
            if (l.indexOf('screen') !== -1
                || l.indexOf('display') !== -1
                || l.indexOf('monitor') !== -1) {
                return 'monitor';
            }
            if (l.indexOf('tab') !== -1) {
                return 'browser';
            }
            return 'window';
        }
        return 'unknown';
    }

    /**
     * Grabs a single frame. Prefers ImageCapture; falls back to a hidden
     * <video>+<canvas> pipeline.
     */
    async function grabFrame(stream, track, imageCapture) {
        if (imageCapture) {
            try {
                return await imageCapture.takePhoto();
            } catch (e) {
                // Some Chromium versions throw on takePhoto() for certain
                // source types (e.g. tabs with hardware-protected video).
                // Fall through to the video path.
                console.warn('Cyrena.ScreenShare: ImageCapture.takePhoto failed, falling back to <video>:', e);
            }
        }
        return await captureViaVideoElement(stream);
    }

    /**
     * Fallback frame-grab. Created fresh per call to avoid the readyState
     * dance with a long-lived element.
     */
    async function captureViaVideoElement(stream) {
        const video = document.createElement('video');
        video.srcObject = stream;
        video.muted = true;
        video.playsInline = true;

        await new Promise(function (resolve, reject) {
            video.onloadedmetadata = function () { resolve(); };
            video.onerror = function () { reject(new Error('Video metadata load failed.')); };
        });

        // play() can reject on autoplay-restricted contexts; the frame
        // is still accessible via drawImage() once metadata is loaded.
        await video.play().catch(function () { /* ignore */ });

        // One rAF so the frame is actually decoded.
        await new Promise(function (resolve) { requestAnimationFrame(resolve); });

        const track = stream.getVideoTracks()[0];
        const settings = track ? track.getSettings() : {};
        const width = settings.width || video.videoWidth || 1280;
        const height = settings.height || video.videoHeight || 720;

        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        if (!ctx) {
            throw new Error('Unable to acquire 2D canvas context.');
        }
        ctx.drawImage(video, 0, 0, width, height);

        return await new Promise(function (resolve, reject) {
            canvas.toBlob(function (blob) {
                if (!blob) reject(new Error('Canvas toBlob returned null.'));
                else resolve(blob);
            }, 'image/png');
        });
    }

    /**
     * UUID v4 via crypto.randomUUID when available, fallback for
     * non-secure contexts (where crypto.randomUUID is undefined).
     * The fallback is NOT cryptographically strong but is unique enough
     * for our purposes -- these are session-scoped keys, not security
     * tokens, and we control the Map they're indexing.
     */
    function generateToken() {
        if (typeof crypto !== 'undefined' && crypto.randomUUID) {
            return crypto.randomUUID();
        }
        // RFC4122 v4 shape, Math.random()-filled. 36 chars incl hyphens.
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3) | 0x8;
            return v.toString(16);
        });
    }

    function blobToDataUrl(blob) {
        return new Promise(function (resolve, reject) {
            const reader = new FileReader();
            reader.onload = function () { resolve(reader.result); };
            reader.onerror = function () { reject(new Error('FileReader failed.')); };
            reader.readAsDataURL(blob);
        });
    }

    function formatTimestamp(d) {
        const pad = function (n) { return n < 10 ? '0' + n : '' + n; };
        return d.getFullYear()
            + pad(d.getMonth() + 1)
            + pad(d.getDate())
            + '-'
            + pad(d.getHours())
            + pad(d.getMinutes())
            + pad(d.getSeconds());
    }

})();
