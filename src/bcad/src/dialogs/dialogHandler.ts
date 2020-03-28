import { Client } from "../client";

export class DialogHandler {
    private dialogContainer: HTMLDivElement;
    private dialogHandlers: Record<string, {(dialogOptions: object): Promise<object>}> = {};

    constructor(client: Client) {
        this.dialogContainer = <HTMLDivElement>document.getElementById('modal-dialog-container');
        window.addEventListener('resize', () => {
            this.resizeDialog();
        });
        client.registerDialogHandler(async (dialogId, dialogOptions) => {
            // show modal blocker
            let dialogMask = document.getElementById('modal-dialog-mask');
            dialogMask.style.display = 'block';

            // show dialog container
            this.dialogContainer.style.display = 'block';

            // show individual dialog
            let dialogElementId = `modal-dialog-${dialogId}`;
            let dialogElement = document.getElementById(dialogElementId);
            dialogElement.style.display = 'block';

            this.resizeDialog();

            // do work
            let dialogHandler = this.dialogHandlers[dialogId]; // TODO: what if the handler wasn't found?
            try {
                const result = await dialogHandler(dialogOptions);
                return result;
            }
            catch (_reason) {
                // dialog was cancelled
            }
            finally {
                dialogElement.style.display = 'none';
                this.dialogContainer.style.display = 'none';
                dialogMask.style.display = 'none';
            }
        });
    }

    private resizeDialog() {
        this.dialogContainer.style.left = `${(window.innerWidth / 2) - (this.dialogContainer.clientWidth / 2)}px`;
        this.dialogContainer.style.top = `${(window.innerHeight / 2) - (this.dialogContainer.clientHeight / 2)}px`;
    }

    registerDialogHandler(dialogId: string, dialogHandler: {(dialogOptions: object): Promise<object>}) {
        this.dialogHandlers[dialogId] = dialogHandler;
    }
}
