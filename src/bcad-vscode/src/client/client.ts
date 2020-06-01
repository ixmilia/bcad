import { VSCode } from "./vscodeInterface";
import { ClientUpdate, DialogOptions, MouseButton } from "./contracts";

export class Client {
    private clientUpdateSubscriptions: Array<{(clientUpdate: ClientUpdate): void}> = [];
    private currentDialogHandler: {(dialogId: string, dialogOptions: object): Promise<any>} = async (_dialogId, _dialogOptions) => {};
    private nextId: number = 1;

    constructor(private vscode: VSCode) {
        this.prepareEventHandlers();
    }

    private prepareEventHandlers() {
        window.addEventListener('message', (messageWrapper: any) => {
            const message = messageWrapper.data;
            console.log(message);
            switch (message.method) {
                case 'ClientUpdate':
                    const clientUpdate = <ClientUpdate>message.params;
                    for (let sub of this.clientUpdateSubscriptions) {
                        sub(clientUpdate);
                    }
                    break;
            }
        });
    }

    private postNotification(method: string, params: any) {
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

    cancel() {
        this.postNotification('Cancel', {});
    }

    changeCurrentLayer(layerName: string) {
        this.postNotification('ChangeCurrentLayer', { layerName });
    }

    mouseDown(button: MouseButton, cursorX: number, cursorY: number) {
        this.postNotification('MouseDown', { button, cursorX, cursorY });
    }

    mouseUp(button: MouseButton, cursorX: number, cursorY: number) {
        this.postNotification('MouseUp', { button, cursorX, cursorY });
    }

    mouseMove(cursorX: number, cursorY: number) {
        this.postNotification('MouseMove', { cursorX, cursorY });
    }

    pan(dx: number, dy: number) {
        this.postNotification('Pan', { dx, dy });
    }

    parseFile() {
        this.postNotification('ParseFile', {});
    }

    ready(width: number, height: number) {
        this.postNotification('Ready', { width, height });
    }

    resize(width: number, height: number) {
        this.postNotification('Resize', { width, height });
    }

    submitInput(value: string) {
        this.postNotification('SubmitInput', { value });
    }

    zoom(cursorX: number, cursorY: number, delta: number) {
        this.postNotification('Zoom', { cursorX, cursorY, delta });
    }

    zoomIn(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, 1);
    }

    zoomOut(cursorX: number, cursorY: number) {
        this.zoom(cursorX, cursorY, -1);
    }
}
