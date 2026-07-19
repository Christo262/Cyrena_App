const { app, BrowserWindow, Menu, session, desktopCapturer } = require('electron');
const path = require('path');
const fs = require('fs');

// 1. Replicate your C# AppArgs record parsing logic
function parseAppArgs(args) {
    const result = {
        title: "Cyréna",
        url: "http://localhost:8000",
        width: 800,
        height: 600
    };

    for (let i = 0; i < args.length; i++) {
        const next = (i + 1 < args.length) ? args[i + 1] : null;

        switch (args[i]) {
            case '--title':
                if (next) { result.title = next; i++; }
                break;
            case '--url':
                if (next) { result.url = next; i++; }
                break;
            case '--width':
                if (next && !isNaN(parseInt(next))) { result.width = parseInt(next); i++; }
                break;
            case '--height':
                if (next && !isNaN(parseInt(next))) { result.height = parseInt(next); i++; }
                break;
        }
    }
    return result;
}

// Global window reference to avoid garbage collection
let mainWindow;
const appArgs = parseAppArgs(process.argv);

function createWindow() {
    // 2. Hide top application menu entirely (File|Edit|View)
    Menu.setApplicationMenu(null);

    // 3. Create the window mapping Photino defaults
    mainWindow = new BrowserWindow({
        title: appArgs.title,
        width: appArgs.width,
        height: appArgs.height,
        icon: path.join(__dirname, 'favicon.png'), // Photino Linux fallback
        useContentSize: false,
        center: true, 
        webPreferences: {
            nodeIntegration: false,    // Safe architecture for web layers
            contextIsolation: true,
            devTools: false            // Equivalent to SetDevToolsEnabled(false)
        }
    });

    // Disable system menu backup shortcuts completely
    mainWindow.setMenuBarVisibility(false);

    // 4. Implement your window resizing preservation layer
    let renderCount = 0;
    mainWindow.on('resize', () => {
        try {
            // Replicating your layout bootstrap shim count
            if (renderCount < 5) {
                renderCount++;
                return;
            }
            const [width, height] = mainWindow.getSize();
            const jsonStr = JSON.stringify({ width, height });
            fs.writeFileSync('./photino.json', jsonStr, 'utf-8');
        } catch (err) {
            // Silent catch to mirror Photino try-catch wrapper
        }
    });

    // 5. Automatically resolve screen share requests for localhost
    session.defaultSession.setDisplayMediaRequestHandler(async (request, callback) => {
        try {
            const sources = await desktopCapturer.getSources({ types: ['screen', 'window'] });
            const primarySource = sources[0]; // Resolves to the main desktop workspace

            if (primarySource) {
                callback({ 
                    video: primarySource,
                    audio: 'loopback' 
                });
            } else {
                callback(new Error('No desktop capture sources found.'));
            }
        } catch (error) {
            callback(error);
        }
    });

    // 6. Connect to your background host service
    mainWindow.loadURL(appArgs.url);
}

// Single instance lock to prevent launching multiple shells against the same port
const gotTheLock = app.requestSingleInstanceLock();
if (!gotTheLock) {
    app.quit();
} else {
    app.on('second-instance', () => {
        if (mainWindow) {
            if (mainWindow.isMinimized()) mainWindow.restore();
            mainWindow.focus();
        }
    });

    app.whenReady().then(createWindow);
}

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit();
});
