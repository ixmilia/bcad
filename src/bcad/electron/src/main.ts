import * as path from 'path';

import { app, BrowserWindow } from "electron";
import { Arguments } from './client/args';
import { StdioCadServerTransport } from "./StdioCadServerTransport";
import { CadMenu } from './CadMenu';

let mainWindow: Electron.BrowserWindow;

function createWindow() {
    var args = new Arguments(process.argv);

    var [width, height] = args.isDebug
        ? [1600, 800]
        : [1280, 720];

    // Create the browser window.
    mainWindow = new BrowserWindow({
        icon: path.join(__dirname, "bcad.ico"),
        width: width,
        height: height,
        webPreferences: {
            preload: path.join(__dirname, "preload.js"),
            additionalArguments: args.getArgList(),
        },
    });

    // prepare stdio listener
    const stdioTransport = new StdioCadServerTransport(args);
    stdioTransport.prepareHandlers(mainWindow);
    stdioTransport.registerReadyCallback(() => console.log('got ready notification from client'));

    new CadMenu(mainWindow, stdioTransport);

    // and load the index.html of the app.
    const htmlPath = path.join(__dirname, "..", "src", "client", "resources", "index.html");
    // const htmlContent = fs.readFileSync(htmlPath).toString('utf-8');
    // const uriContent = `data:text/html;base64,${Buffer.from(htmlContent).toString('base64')}`;
    // mainWindow.loadURL(uriContent);
    mainWindow.loadFile(htmlPath);

    // Open the DevTools.
    if (args.isDevTools) {
        mainWindow.webContents.openDevTools();
    }

    // Emitted when the window is closed.
    mainWindow.on("closed", () => {
    });
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on("ready", createWindow);

// Quit when all windows are closed.
app.on("window-all-closed", () => {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== "darwin") {
        app.quit();
    }
});

app.on("activate", () => {
    // On OS X it"s common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (mainWindow === null) {
        createWindow();
    }
});

// In this file you can include the rest of your app"s specific main process
// code. You can also put them in separate files and require them here.
