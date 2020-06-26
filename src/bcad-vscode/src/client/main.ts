import { Client } from "./client";
import { VSCode } from "./vscodeInterface";
import { LayerSelector } from "./layerSelector";
import { InputConsole } from "./inputConsole";
import { OutputConsole } from "./outputConsole";
import { Ribbon } from "./ribbons/ribbon";
import { ViewControl } from "./viewControl";
import { DialogHandler } from "./dialogs/dialogHandler";
import { LayerDialog } from "./dialogs/layerDialog";
import { FileSettingsDialog } from "./dialogs/fileSettingsDialog";

function getPostMessage(): ((message: any) => void) | undefined {
    // @ts-ignore
    let externalInvoke = window.external?.invoke;
    if (typeof externalInvoke === 'function') {
        return message => externalInvoke(JSON.stringify(message));
    }

    // @ts-ignore
    if (typeof acquireVsCodeApi === 'function') {
        // @ts-ignore
        const rawVsCode = acquireVsCodeApi();
        const vscode = <VSCode>rawVsCode;
        return vscode.postMessage;
    }

    return undefined;
}

try {
    let postMessage = getPostMessage();
    if (!postMessage) {
        throw new Error('Unable to determine client communication');
    }

    const client = new Client(postMessage);
    new LayerSelector(client);
    new InputConsole(client);
    new OutputConsole(client);
    new Ribbon(client);
    new ViewControl(client);

    let dialogHandler = new DialogHandler(client);
    new FileSettingsDialog(dialogHandler);
    new LayerDialog(dialogHandler);
} catch (err) {
    console.error(`error: [${err}]: ${JSON.stringify(err)}`);
}
