import * as path from 'path';
import * as vscode from 'vscode';
import { StdioCadServerTransport } from './StdioCadServerTransport';

export class BCadEditorProvider implements vscode.CustomEditorProvider {
    private static viewType = 'ixmilia-bcad';
    private readonly onDidChangeCustomDocumentEventEmitter = new vscode.EventEmitter<vscode.CustomDocumentEditEvent<vscode.CustomDocument>>();

    public static register(context: vscode.ExtensionContext, dotnetPath: string, serverPath: string): vscode.Disposable {
        let provider = new BCadEditorProvider(context, dotnetPath, serverPath);
        let registration = vscode.window.registerCustomEditorProvider(BCadEditorProvider.viewType, provider);
        return registration;
    }

    private constructor(private context: vscode.ExtensionContext, private dotnetPath: string, private serverPath: string) {
    }

    onDidChangeCustomDocument: vscode.Event<vscode.CustomDocumentEditEvent<vscode.CustomDocument>> = this.onDidChangeCustomDocumentEventEmitter.event;

    saveCustomDocument(document: vscode.CustomDocument, cancellation: vscode.CancellationToken): Promise<void> {
        return this.saveDrawing(document, document.uri);
    }

    saveCustomDocumentAs(document: vscode.CustomDocument, destination: vscode.Uri, cancellation: vscode.CancellationToken): Promise<void> {
        return this.saveDrawing(document, destination);
    }

    private async saveDrawing(document: vscode.CustomDocument, uri: vscode.Uri): Promise<void> {
        const drawing = <StdioCadServerTransport>document;
        const contents = await drawing.getDrawingContents(uri);
        if (contents !== null) {
            const buffer = Buffer.from(contents, 'base64');
            await vscode.workspace.fs.writeFile(uri, buffer);
        }
    }

    revertCustomDocument(document: vscode.CustomDocument, cancellation: vscode.CancellationToken): Promise<void> {
        throw new Error("Method not implemented.");
    }

    backupCustomDocument(document: vscode.CustomDocument, context: vscode.CustomDocumentBackupContext, cancellation: vscode.CancellationToken): Promise<vscode.CustomDocumentBackup> {
        throw new Error("Method not implemented.");
    }

    async openCustomDocument(uri: vscode.Uri, openContext: vscode.CustomDocumentOpenContext, token: vscode.CancellationToken): Promise<vscode.CustomDocument> {
        let document = new StdioCadServerTransport(uri, this.dotnetPath, this.serverPath);
        return document;
    }

    async resolveCustomEditor(document: vscode.CustomDocument, webviewPanel: vscode.WebviewPanel, token: vscode.CancellationToken): Promise<void> {
        webviewPanel.webview.options = {
            enableScripts: true
        };
        webviewPanel.webview.html = await this.getHtml(webviewPanel.webview);

        const buffer = Buffer.from(await vscode.workspace.fs.readFile(document.uri));
        const fileContents = buffer.toString('base64');
        const cadDocument = <StdioCadServerTransport>document;
        cadDocument.registerReadyCallback(() => {
            cadDocument.parseFile(fileContents);
        })
        this.context.subscriptions.push(cadDocument.prepareHandlers(webviewPanel));
    }

    private async getHtml(webview: vscode.Webview): Promise<string> {
        const buffer = Buffer.from(await vscode.workspace.fs.readFile(vscode.Uri.file(path.join(this.context.extensionPath, 'media', 'index.html'))));
        let html = buffer.toString('utf-8');

        const styleUri = webview.asWebviewUri(vscode.Uri.file(path.join(this.context.extensionPath, 'media', 'style.css')));
        html = html.replace('$STYLE-URI$', styleUri.toString());

        const jsUri = webview.asWebviewUri(vscode.Uri.file(path.join(this.context.extensionPath, 'out', 'client.js')));
        html = html.replace('$JS-URI$', jsUri.toString());
        return html;
    }
}
