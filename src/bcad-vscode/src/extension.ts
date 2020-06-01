import * as vscode from 'vscode';
import { BCadEditorProvider } from './BCadEditorProvider';

export function activate(context: vscode.ExtensionContext) {
    // https://github.com/microsoft/vscode-extension-samples/tree/master/custom-editor-sample
    context.subscriptions.push(BCadEditorProvider.register(context));
}

export function deactivate() {
}
