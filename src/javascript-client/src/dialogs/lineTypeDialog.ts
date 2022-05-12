import { LogWriter } from '../logWriter';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

interface LineTypeInfo {
    Name: string;
    Pattern: number[];
    Description: string;
}

interface LineTypeDialogOptions {
    LineTypes: LineTypeInfo[],
}

interface ChangedLineType {
    OldLineTypeName: string,
    NewLineTypeName: string,
    Pattern: number[],
    Description: string;
}

interface LineTypeDialogResult {
    ChangedLineTypes: ChangedLineType[],
}

export class LineTypeDialog extends DialogBase {
    private tableBody: HTMLDivElement;

    constructor(dialogHandler: DialogHandler) {
        super(dialogHandler, "line-type");
        this.tableBody = <HTMLDivElement>document.getElementById('dialog-line-type-list');
        document.getElementById('dialog-line-type-add')!.addEventListener('click', () => {
            this.addLineTypeRow({
                Name: 'NewLineType',
                Pattern: [],
                Description: '',
            });
        });
    }

    dialogShowing(dialogOptions: object) {
        let lineTypeDialogOptions = <LineTypeDialogOptions>dialogOptions;
        this.tableBody.innerHTML = '';
        for (let lineType of lineTypeDialogOptions.LineTypes) {
            this.addLineTypeRow(lineType);
        }
    }

    dialogTitle(dialogOptions: object): string {
        return 'Line Types';
    }

    dialogOk(): object {
        const changedLineTypes: ChangedLineType[] = [];
        for (const element of this.tableBody.children) {
            const row = <HTMLTableRowElement>element;
            const nameInput = <HTMLInputElement>row.children[0].children[0];
            const patternInput = <HTMLInputElement>row.children[1].children[0];
            const descriptionInput = <HTMLInputElement>row.children[2].children[0];
            const pattern = patternInput.value.split(',').map(parseFloat); // TODO: make this robust
            const changed = {
                OldLineTypeName: nameInput.getAttribute('data-original-value')!,
                NewLineTypeName: nameInput.value,
                Pattern: pattern,
                Description: descriptionInput.value,
            };
            changedLineTypes.push(changed);
        }
        const result: LineTypeDialogResult = {
            ChangedLineTypes: changedLineTypes,
        };

        LogWriter.write(`LINE-TYPE-DIALOG: OK, returning ${JSON.stringify(result)}`);
        return result;
    }

    dialogCancel() {
        // noop
    }

    private addLineTypeRow(lineType: LineTypeInfo) {
        // each row will look like this:
        // <div class="dialog-line-type-row">
        //   <div class="dialog-line-type-name-column"><input type="text" class="dialog-line-type-list-line-type-name" value="0" data-original-value="0"></input></div>
        //   <div class="dialog-line-type-color-column"><div data-color="#FFFFFF">...</div></div>
        //   <div class="dialog-line-type-visible-column"><input type="checkbox" class="dialog-line-yype-list-line-type-visible" id="..."></input><label for="...">&nbsp;</label></div>
        //   <div class="dialog-line-type-command-column"><button>Del</button></div>
        // </div>
        LogWriter.write(`LINE-TYPE-DIALOG: adding LineType ${JSON.stringify(lineType)}`);
        const row = document.createElement('div');
        row.classList.add('dialog-line-type-row');

        // name
        const lineTypeInput = document.createElement('input');
        lineTypeInput.classList.add('dialog-line-type-list-line-type-name');
        lineTypeInput.setAttribute('type', 'text');
        lineTypeInput.setAttribute('value', lineType.Name);
        lineTypeInput.setAttribute('data-original-value', lineType.Name);
        const nameColumn = document.createElement('div');
        nameColumn.classList.add('dialog-line-type-name-column');
        nameColumn.appendChild(lineTypeInput);
        row.appendChild(nameColumn);
        // pattern
        const patternInput = document.createElement('input');
        patternInput.classList.add('dialog-line-type-list-line-type-pattern');
        patternInput.setAttribute('type', 'text');
        patternInput.setAttribute('value', lineType.Pattern.join(','));
        const patternColumn = document.createElement('div');
        patternColumn.classList.add('dialog-line-type-pattern-column');
        patternColumn.appendChild(patternInput);
        row.appendChild(patternColumn);
        // description
        const descriptionInput = document.createElement('input');
        descriptionInput.classList.add('dialog-line-type-list-line-type-description');
        descriptionInput.setAttribute('type', 'text');
        descriptionInput.setAttribute('value', lineType.Description);
        const descriptionColumn = document.createElement('div');
        descriptionColumn.classList.add('dialog-line-type-description-column');
        descriptionColumn.appendChild(descriptionInput);
        row.appendChild(descriptionColumn);
        // delete
        const deleteButton = document.createElement('button');
        deleteButton.innerText = "Delete";
        deleteButton.addEventListener('click', () => {
            row.remove();
        });
        const commandColumn = document.createElement('div');
        commandColumn.classList.add('dialog-line-type-command-column');
        commandColumn.appendChild(deleteButton);
        row.appendChild(commandColumn);

        this.tableBody.appendChild(row);
    }
}
