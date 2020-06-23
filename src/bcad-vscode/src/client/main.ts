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

// @ts-ignore
const rawVsCode = acquireVsCodeApi();
console.log('got vscode');

const vscode = <VSCode>rawVsCode;
const client = new Client(vscode);
new LayerSelector(client);
new InputConsole(client);
new OutputConsole(client);
new Ribbon(client);
new ViewControl(client);

let dialogHandler = new DialogHandler(client);
new FileSettingsDialog(dialogHandler);
new LayerDialog(dialogHandler);
console.log('finished main');
