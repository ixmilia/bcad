import { ClientAgent, ClientUpdate } from "./contracts.generated";

export class Client extends ClientAgent {
    private functionHandlers: Map<string, ((args: any[]) => void)[]> = new Map();
    private clientUpdateSubscriptions: Array<{ (clientUpdate: ClientUpdate): void }> = [];
    private currentDialogHandler: { (dialogId: string, dialogOptions: object): Promise<any> } = async (_dialogId, _dialogOptions) => { };
    private invocationCallbacks: Map<number, (result: any) => void> = new Map();
    private nextId: number = 1;

    constructor(private postMessage: (message: any) => void) {
        super();
    }

    addFunctionHandler(handlerName: string, handler: (args: any[]) => void) {
        const handlers = this.functionHandlers.get(handlerName) ?? [];
        handlers.push(handler);
        this.functionHandlers.set(handlerName, handlers);
    }

    handleMessage(message: any) {
        if (message) {
            if (typeof message.method === 'string') {
                const handlers = this.functionHandlers.get(message.method);
                if (handlers) {
                    for (const handler of handlers) {
                        handler(message.params);
                    }
                }
            }

            switch (message.method) {
                case 'ClientUpdate':
                    const clientUpdate = <ClientUpdate>message.params[0];
                    for (let sub of this.clientUpdateSubscriptions) {
                        sub(clientUpdate);
                    }
                    break;
                case 'ShowDialog':
                    const id: string = message.params[0].id;
                    const parameter: object = message.params[0].parameter;
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
