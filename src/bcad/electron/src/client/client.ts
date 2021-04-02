import { ClientAgent, ClientDownload, ClientUpdate } from "./contracts.generated";

export class Client extends ClientAgent {
    private clientUpdateSubscriptions: Array<{ (clientUpdate: ClientUpdate): void }> = [];
    private currentDialogHandler: { (dialogId: string, dialogOptions: object): Promise<any> } = async (_dialogId, _dialogOptions) => { };
    private invocationCallbacks: Map<number, (result: any) => void> = new Map();
    private nextId: number = 1;

    constructor(private postMessage: (message: any) => void) {
        super();
    }

    handleMessage(message: any) {
        if (message) {
            switch (message.method) {
                case 'ClientUpdate':
                    const clientUpdate = <ClientUpdate>message.params;
                    for (let sub of this.clientUpdateSubscriptions) {
                        sub(clientUpdate);
                    }
                    break;
                case 'DownloadFile':
                    const fileData = <ClientDownload>message.params;
                    const element = document.createElement('a');
                    element.href = 'data:image/svg+xml;base64,' + fileData.Data;
                    element.download = fileData.Filename;
                    element.click();
                    break;
                case 'ShowDialog':
                    const id: string = message.params.id;
                    const parameter: object = message.params.parameter;
                    this.currentDialogHandler(id, parameter).then(result => {
                        this.postMessage({
                            id: message.id,
                            result
                        });
                    }).catch(reason => {
                        this.postMessage({
                            id: message.id,
                            result: null
                        });
                    });
                    break;
                default:
                    if (typeof message.id === 'number') {
                        const callback = this.invocationCallbacks.get(message.id);
                        if (callback) {
                            this.invocationCallbacks.delete(message.id);
                            callback(message.result);
                        }
                    }
                    break;
            }
        }
    }

    postNotification(method: string, params: any) {
        this.postMessage({
            method,
            params
        });
    }

    invoke(method: string, params: any): Promise<any> {
        return new Promise<any>(resolve => {
            const id = this.nextId++;
            this.invocationCallbacks.set(id, resolve);
            this.postMessage({
                id,
                method,
                params,
            });
        });
    }

    registerDialogHandler(dialogHandler: { (dialogId: string, dialogOptions: object): Promise<object> }) {
        this.currentDialogHandler = dialogHandler;
    }

    subscribeToClientUpdates(subscription: { (clientUpdate: ClientUpdate): void }) {
        this.clientUpdateSubscriptions.push(subscription);
    }

    zoomIn(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, 1);
    }

    zoomOut(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, -1);
    }
}
