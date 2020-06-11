import * as path from 'path';
import * as vscode from 'vscode';
import { StdioCadServerTransport } from './StdioCadServerTransport';
import { fstat } from 'fs';

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
        // nothing to do
        return Promise.resolve();
    }

    async backupCustomDocument(document: vscode.CustomDocument, context: vscode.CustomDocumentBackupContext, cancellation: vscode.CancellationToken): Promise<vscode.CustomDocumentBackup> {
        // ensure backup directory exists
        const parsedPath = path.parse(context.destination.fsPath);
        const dir = vscode.Uri.file(parsedPath.dir);
        await vscode.workspace.fs.createDirectory(dir);

        // save file to location with appropriate extension
        const drawingPath = path.parse(document.uri.fsPath);
        const finalBackupLocation = vscode.Uri.file(`${context.destination.fsPath}${drawingPath.ext}`);
        await this.saveDrawing(document, finalBackupLocation);
        return {
            id: finalBackupLocation.fsPath,
            delete: () => {
                vscode.workspace.fs.delete(finalBackupLocation);
            }
        };
    }

    async openCustomDocument(uri: vscode.Uri, openContext: vscode.CustomDocumentOpenContext, token: vscode.CancellationToken): Promise<vscode.CustomDocument> {
        const filePath = openContext.backupId ?? uri.fsPath;
        const drawingUri = vscode.Uri.file(filePath);
        const buffer = Buffer.from(await vscode.workspace.fs.readFile(drawingUri));
        const fileContents = buffer.toString('base64');
        let document = new StdioCadServerTransport(uri, this.dotnetPath, this.serverPath);
        let isEditorReady = false;
        let ignoreNextUpdate = false;
        let isFirstUpdate = true;
        document.registerReadyCallback(() => {
            ignoreNextUpdate = true;
            isEditorReady = true;
            document.parseFile(fileContents);
        });

        document.subscribeToClientUpdates(clientUpdate => {
            const shouldIgnoreThisUpdate = ignoreNextUpdate;
            ignoreNextUpdate = false;
            if (shouldIgnoreThisUpdate) {
                return;
            }

            if (clientUpdate.Drawing && isEditorReady) {
                // skip the first update that's just populating the drawing
                if (isFirstUpdate) {
                    isFirstUpdate = false;
                    return;
                }

                this.onDidChangeCustomDocumentEventEmitter.fire({
                    document,
                    undo: () => {
                        ignoreNextUpdate = true;
                        document.undo();
                    },
                    redo: () => {
                        ignoreNextUpdate = true;
                        document.redo();
                    }
                });
            }
        });
        return document;
    }

    async resolveCustomEditor(document: vscode.CustomDocument, webviewPanel: vscode.WebviewPanel, token: vscode.CancellationToken): Promise<void> {
        webviewPanel.webview.options = {
            enableScripts: true
        };
        webviewPanel.webview.html = await this.getHtml(webviewPanel.webview);
        const cadDocument = <StdioCadServerTransport>document;
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
