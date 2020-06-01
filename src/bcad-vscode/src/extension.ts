import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';
import { BCadEditorProvider } from './BCadEditorProvider';

export async function activate(context: vscode.ExtensionContext) {
    // get .NET runtime
    const config = vscode.workspace.getConfiguration('ixmilia-bcad');
    const minimumRuntimeVersion = config.get<string>('minimumRuntimeVersion');
    const commandResult = await vscode.commands.executeCommand<{ dotnetPath: string }>('dotnet.acquire', { version: minimumRuntimeVersion });
    const dotnetPath = commandResult!.dotnetPath;

    // get server
    let serverPath: string;
    if (fs.existsSync(path.join(context.extensionPath, 'local-sentinel.txt'))) {
        // running locally
        serverPath = path.join(context.extensionPath, '../../artifacts/bin/IxMilia.BCad.Server/Debug/netcoreapp3.1');
    } else {
        // running as installed, download and extract to global storage
        // TODO: check for existing version
        // TODO: if not present, download and extract
        serverPath = 'TODO: implement this';
    }

    context.subscriptions.push(BCadEditorProvider.register(context, dotnetPath, serverPath));
}

export function deactivate() {
}
