import { Client } from "./client";
import { LayerSelector } from "./layerSelector";
import { InputConsole } from "./inputConsole";
import { OutputConsole } from "./outputConsole";
import { Ribbon } from "./ribbons/ribbon";
import { ViewControl } from "./viewControl";
import { DialogHandler } from "./dialogs/dialogHandler";
import { LayerDialog } from "./dialogs/layerDialog";
import { ShortcutHandler } from "./shortcutHandler";
import { FileSettingsDialog } from "./dialogs/fileSettingsDialog";
import { PlotDialog } from "./dialogs/plotDialog";
import { Arguments } from "./args";
import { SaveChangesDialog } from "./dialogs/saveChangesDialog";

enum HostType {
    Electron = 1,
    WebView = 2,
    WebView2 = 3,
    VSCode = 4,
}

function getHostType(): HostType | undefined {
    // @ts-ignore
    if (typeof invoke === 'function') {
        return HostType.WebView;
    }

    // @ts-ignore
    if (typeof window?.external?.sendMessage === 'function') {
        return HostType.WebView2;
    }

    // @ts-ignore
    if (typeof acquireVsCodeApi === 'function') {
        return HostType.VSCode;
    }

    // @ts-ignore
    if (typeof ipcRenderer === 'object') {
        return HostType.Electron;
    }

    return undefined;
}

function getPostMessage(hostType: HostType): ((message: any) => void) {
    switch (hostType) {
        case HostType.Electron:
            // @ts-ignore
            return message => ipcRenderer.send('post-message', message);
        case HostType.VSCode:
            // @ts-ignore
            return acquireVsCodeApi().postMessage;
        case HostType.WebView:
            // @ts-ignore
            return message => invoke(message);
        case HostType.WebView2:
            return message => {
                try {
                    message['jsonrpc'] = '2.0';
                    // @ts-ignore
                    window.external.sendMessage(JSON.stringify(message));
                } catch (err) {
                    alert(`error sending message: ${err}`);
                }
            };
    }
}

function bindServerMessage(hostType: HostType, callback: (message: any) => void) {
    switch (hostType) {
        case HostType.Electron:
            // @ts-ignore
            ipcRenderer.on('post-message', (_event, arg) => callback(arg));
            break;
        case HostType.VSCode:
            window.addEventListener('message', (messageWrapper: any) => {
                const message = messageWrapper.data;
                callback(message);
            });
            break;
        case HostType.WebView:
            window.addEventListener('message', function (message: any) {
                callback(message.detail);
            });
            break;
        case HostType.WebView2:
            // @ts-ignore
            window.external.receiveMessage((message: string) => {
                try {
                    const objMessage = JSON.parse(message);
                    callback(objMessage);
                } catch (err) {
                    console.error(`error parsing message: ${err}.  Message was "${message}"`);
                }
            });
    }
}

async function start(argArray: string[]): Promise<void> {
    const args = new Arguments(argArray);

    const hostType = getHostType();
    if (!hostType) {
        throw new Error('Unable to determine client communication');
    }

    if (args.isDebug) {
        await new Promise(resolve => setTimeout(resolve, 5000));
    }

    let postMessage = getPostMessage(hostType);
    const client = new Client(postMessage);
    bindServerMessage(hostType, message => {
        client.handleMessage(message);
    });

    const shortcutHandler = new ShortcutHandler(client);
    new LayerSelector(client);
    new InputConsole(client, shortcutHandler);
    new OutputConsole(client);
    new Ribbon(client);
    new ViewControl(client);

    let dialogHandler = new DialogHandler(client);
    new FileSettingsDialog(dialogHandler);
    new LayerDialog(dialogHandler);
    new PlotDialog(client, dialogHandler);
    new SaveChangesDialog(dialogHandler);
}

window.addEventListener("DOMContentLoaded", () => {
    const appendMessage = (message: string) => {
        const output = <HTMLTextAreaElement>document.getElementById("outputConsole");
        if (output.value.length > 0) {
            output.value += '\n';
        }
        output.value += message;
        output.scrollTop = output.scrollHeight;
    };
    start([]).catch((err: any) => {
        const errorMessage = `error: ${err}`;
        console.error(errorMessage);
        appendMessage(errorMessage);
    });
});
