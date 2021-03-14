import * as cp from 'child_process';
import * as os from 'os';
import * as path from 'path';
import * as rpc from 'vscode-jsonrpc';
import { BrowserWindow, ipcMain } from 'electron';
import { Arguments } from './client/args';
import { ClientUpdate } from './client/contracts.generated';

export class StdioCadServerTransport {
    private childProcess: cp.ChildProcessWithoutNullStreams;
    private clientUpdateSubscriptions: Array<{ (clientUpdate: ClientUpdate): void }> = [];
    private connection: rpc.MessageConnection;
    private nextId: number = 1;
    private postMessage: { (message: any): void } = _ => { };
    private onReadyCallbacks: Array<{ (): void }> = [];
    private replyHandlers: Map<number, { (payload: any): void }> = new Map<number, { (payload: any): void }>();

    constructor(args: Arguments) {
        const serverAssembly = os.platform() == "win32"
            ? "IxMilia.BCad.Server.exe"
            : "IxMilia.BCad.Server";
        const serverSubPath = args.isLocal
            ? '../../../../artifacts/bin/IxMilia.BCad.Server/Debug/netcoreapp3.1'
            : '../../publish';
        const serverPath = path.join(__dirname, serverSubPath, serverAssembly);
        this.childProcess = cp.spawn(serverPath);
        this.childProcess.on('exit', (code: number, signal: string) => {
            console.log(`process exited with ${code}: ${signal}`);
        });
        console.log('server pid ' + this.childProcess.pid);
        let logger: rpc.Logger = {
            error: console.log,
            warn: console.log,
            info: console.log,
            log: console.log,
        };
        this.connection = rpc.createMessageConnection(
            new rpc.StreamMessageReader(this.childProcess.stdout),
            new rpc.StreamMessageWriter(this.childProcess.stdin),
            logger);
        this.connection.onRequest((method, params) => {
            // request from the server, forward
            return new Promise<any>((resolve, reject) => {
                const id = this.nextId++;
                this.replyHandlers.set(id, resolve);
                this.postMessage({
                    method,
                    params,
                    id
                });
            });
        });
        this.connection.onNotification((method, params) => {
            if (method === 'ClientUpdate') {
                const clientUpdate = <ClientUpdate>params;
                for (let sub of this.clientUpdateSubscriptions) {
                    sub(clientUpdate);
                }
            }
            // all client notifications are forwarded with no changes
            this.postMessage({
                method,
                params
            });
        });
        this.connection.listen();
    }

    prepareHandlers(mainWindow: BrowserWindow) {
        ipcMain.on('post-message', (_event, message) => {
            // message from the client, forward to the server
            this.handleMessageFromClient(message);
            if (message?.method === 'Ready') {
                for (let onReady of this.onReadyCallbacks) {
                    onReady();
                }

                this.onReadyCallbacks = [];
            }
        });

        this.postMessage = message => mainWindow.webContents.send('post-message', message);
    }

    registerReadyCallback(onReady: { (): void }) {
        this.onReadyCallbacks.push(onReady);
    }

    subscribeToClientUpdates(subscription: { (clientUpdate: ClientUpdate): void }) {
        this.clientUpdateSubscriptions.push(subscription);
    }

    undo() {
        this.connection.sendNotification('Undo', {});
    }

    redo() {
        this.connection.sendNotification('Redo', {});
    }

    private async handleMessageFromClient(message: any) {
        if (message?.method !== 'MouseMove') {
            console.log(JSON.stringify(message));
        }
        if (message.result !== undefined && message.id) {
            // response to a server-side request, find the appropriate reply channel and send it back
            const handler = this.replyHandlers.get(message.id);
            if (handler) {
                this.replyHandlers.delete(message.id);
                handler(message.result);
            }
        } else if (message.id) {
            // request from client, reply expected
            const serverResult = await this.connection.sendRequest(message.method, message.params);
            this.postMessage({
                result: serverResult,
                id: message.id
            });
        } else {
            this.connection.sendNotification(message.method, message.params);
        }
    }

    parseFile(filePath: string, data: string) {
        this.connection.sendNotification('ParseFile', { filePath, data });
    }

    async getDrawingContents(filePath: string, preserveSettings: boolean): Promise<string | null> {
        const fileContents = await this.connection.sendRequest<string | null>('GetDrawingContents', { filePath, preserveSettings });
        return fileContents;
    }

    dispose() {
        this.childProcess.kill();
    }
}
