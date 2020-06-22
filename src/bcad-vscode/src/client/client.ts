import { VSCode } from "./vscodeInterface";
import { ClientAgent, ClientUpdate } from "./contracts.generated";

export class Client extends ClientAgent {
    private clientUpdateSubscriptions: Array<{(clientUpdate: ClientUpdate): void}> = [];
    private currentDialogHandler: {(dialogId: string, dialogOptions: object): Promise<any>} = async (_dialogId, _dialogOptions) => {};
    private nextId: number = 1;

    constructor(private vscode: VSCode) {
        super();
        this.prepareEventHandlers();
    }

    private prepareEventHandlers() {
        window.addEventListener('message', (messageWrapper: any) => {
            const message = messageWrapper.data;
            switch (message.method) {
                case 'ClientUpdate':
                    const clientUpdate = <ClientUpdate>message.params;
                    for (let sub of this.clientUpdateSubscriptions) {
                        sub(clientUpdate);
                    }
                    break;
                case 'ShowDialog':
                    const id: string = message.params.id;
                    const parameter: object = message.params.parameter;
                    this.currentDialogHandler(id, parameter).then(result => {
                        this.vscode.postMessage({
                            id: message.id,
                            result
                        });
                    }).catch(reason => {
                        this.vscode.postMessage({
                            id: message.id,
                            result: null
                        });
                    });
                    break;
            }
        });
    }

    postNotification(method: string, params: any) {
        this.vscode.postMessage({
            method,
            params
        });
    }

    executeCommand(command: string) {
        this.postNotification('ExecuteCommand', { command });
        // this.vscode.postMessage({
        //     method: 'ExecuteCommand',
        //     params: { command },
        //     id: 1
        // });
    }

    registerDialogHandler(dialogHandler: {(dialogId: string, dialogOptions: object): Promise<object>}) {
        this.currentDialogHandler = dialogHandler;
    }

    subscribeToClientUpdates(subscription: {(clientUpdate: ClientUpdate): void}) {
        this.clientUpdateSubscriptions.push(subscription);
    }

    zoomIn(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, 1);
    }

    zoomOut(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, -1);
    }
}
