import { Client } from "./client";
import { LayerSelector } from "./layerSelector";
import { InputConsole } from "./inputConsole";
import { OutputConsole } from "./outputConsole";
import { Ribbon } from "./ribbons/ribbon";
import { ViewControl } from "./viewControl";
import { DialogHandler } from "./dialogs/dialogHandler";
import { LayerDialog } from "./dialogs/layerDialog";
import { FileSettingsDialog } from "./dialogs/fileSettingsDialog";
import { Arguments } from "./args";

enum HostType {
    Electron = 1,
    WebView = 2,
    VSCode = 3,
}

function getHostType(): HostType | undefined {
    // @ts-ignore
    if (typeof invoke === 'function') {
        return HostType.WebView;
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
    }
}

export async function start(argArray: string[]): Promise<void> {
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
    bindServerMessage(hostType, message => client.handleMessage(message));

    new LayerSelector(client);
    new InputConsole(client);
    new OutputConsole(client);
    new Ribbon(client);
    new ViewControl(client);

    let dialogHandler = new DialogHandler(client);
    new FileSettingsDialog(dialogHandler);
    new LayerDialog(dialogHandler);
}
