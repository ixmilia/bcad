import { DialogHandler } from "./dialogHandler";

export abstract class DialogBase {
    constructor(dialogHandler: DialogHandler, dialogId: string) {
        dialogHandler.registerDialogHandler(dialogId, (dialogOptions) => {
            this.dialogShowing(dialogOptions);
            let promise = new Promise<object>(
                (resolve, reject) => {
                    var okButton = <HTMLButtonElement>document.getElementById('dialog-ok');
                    okButton.addEventListener('click', () => {
                        let result = this.dialogOk();
                        resolve(result);
                    });

                    var cancelButton = <HTMLButtonElement>document.getElementById('dialog-cancel');
                    cancelButton.addEventListener('click', () => {
                        this.dialogCancel();
                        reject();
                    });

                    const closeButton = document.getElementById('dialog-close');
                    closeButton?.addEventListener('click', () => {
                        this.dialogCancel();
                        reject();
                    });
                });
            return promise;
        });
    }

    abstract dialogShowing(dialogOptions: object): void;
    abstract dialogOk(): object;
    abstract dialogCancel(): void;
}
