import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

export class SaveChangesDialog extends DialogBase {
    private saveChanges: HTMLInputElement;

    constructor(dialogHandler: DialogHandler) {
        super(dialogHandler, "saveChanges");
        this.saveChanges = <HTMLInputElement>document.getElementById('save-changes-save');
    }

    dialogShowing(dialogOptions: object) {
        this.saveChanges.checked = true;
    }

    dialogTitle(dialogOptions: object): string {
        return 'Save changes?';
    }

    dialogOk(): object {
        const result = this.saveChanges.checked ? 'save' : 'discard';
        return {
            result,
        };
    }

    dialogCancel() {
        // noop
    }
}
