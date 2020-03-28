import { Client } from "./client";

export class DialogHandler {
    private dialogHandlers: Record<string, {(dialogOptions: object): Promise<object>}> = {};

    constructor(client: Client) {
        client.registerDialogHandler(async (dialogId, dialogOptions) => {
            // show modal blocker
            let dialogContainer = document.getElementById('modal-dialog-container');
            dialogContainer.style.display = 'block';

            // show dialog
            let dialogElementId = `modal-dialog-${dialogId}`;
            let dialogElement = document.getElementById(dialogElementId);
            dialogElement.style.display = 'block';

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
                dialogContainer.style.display = 'none';
            }
        });
    }

    registerDialogHandler(dialogId: string, dialogHandler: {(dialogOptions: object): Promise<object>}) {
        this.dialogHandlers[dialogId] = dialogHandler;
    }
}
