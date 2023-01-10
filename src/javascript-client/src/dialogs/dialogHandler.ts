import { InputConsole } from "../inputConsole";
import { Client } from "../client";

export class DialogHandler {
    private dialogContainer: HTMLDivElement;
    private dialogMask: HTMLElement;
    private dialogHandlers: Record<string, { (dialogOptions: object): Promise<object> }> = {};

    constructor(client: Client) {
        this.dialogContainer = <HTMLDivElement>document.getElementById('modal-dialog-container');
        this.dialogMask = <HTMLElement>document.getElementById('modal-dialog-mask');

        InputConsole.ensureCapturedEvents(this.dialogContainer);
        window.addEventListener('resize', () => {
            this.resizeDialog();
        });
        client.registerDialogHandler((dialogId, dialogOptions) => {
            return new Promise<object>(async (resolve, reject) => {
                this.showDialogs();

                // show individual dialog
                let dialogElementId = `modal-dialog-${dialogId}`;
                let dialogElement = <HTMLElement>document.getElementById(dialogElementId);
                dialogElement.style.display = 'block';

                this.resizeDialog();

                // do work
                let dialogHandler = this.dialogHandlers[dialogId]; // TODO: what if the handler wasn't found?
                try {
                    const result = await dialogHandler(dialogOptions);
                    resolve(result);
                }
                catch (_reason) {
                    // dialog was cancelled
                    reject();
                }
                finally {
                    dialogElement.style.display = 'none';
                    this.hideDialogs();
                }
            });
        });
    }

    showDialogs() {
        this.dialogContainer.style.display = 'block';
        this.dialogMask.style.display = 'block';
    }

    hideDialogs() {
        this.dialogContainer.style.display = 'none';
        this.dialogMask.style.display = 'none';
    }

    private resizeDialog() {
        this.dialogContainer.style.left = `${(window.innerWidth / 2) - (this.dialogContainer.clientWidth / 2)}px`;
        this.dialogContainer.style.top = `${(window.innerHeight / 2) - (this.dialogContainer.clientHeight / 2)}px`;
    }

    registerDialogHandler(dialogId: string, dialogHandler: { (dialogOptions: object): Promise<object> }) {
        this.dialogHandlers[dialogId] = dialogHandler;
    }
}
