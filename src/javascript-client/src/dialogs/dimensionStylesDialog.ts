import { LogWriter } from '../logWriter';
import { ColorPicker } from '../controls/colorPicker';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';
import { Client } from '../client';
import {
    DimensionStylesDialogEntry,
    DimensionStylesDialogParameters,
} from '../contracts.generated';

export class DimensionStylesDialog extends DialogBase {
    private selector: HTMLSelectElement;
    private name: HTMLInputElement;
    private originalNameRow: HTMLTableRowElement;
    private originalName: HTMLSpanElement;
    private arrowSize: HTMLInputElement;
    private tickSize: HTMLInputElement;
    private extensionLineOffset: HTMLInputElement;
    private extensionLineExtension: HTMLInputElement;
    private textHeight: HTMLInputElement;
    private lineGap: HTMLInputElement;
    private lineColor: HTMLDivElement;
    private textColor: HTMLDivElement;

    private lineColorPicker: ColorPicker;
    private textColorPicker: ColorPicker;

    private parameter: DimensionStylesDialogParameters;

    constructor(dialogHandler: DialogHandler, client: Client) {
        super(dialogHandler, "dimension-styles");
        this.selector = <HTMLSelectElement>document.getElementById('dimension-style-dialog-selector');
        this.name = <HTMLInputElement>document.getElementById('dimension-style-dialog-name');
        this.originalNameRow = <HTMLTableRowElement>document.getElementById('dimension-style-dialog-original-name-row');
        this.originalName = <HTMLSpanElement>document.getElementById('dimension-style-dialog-original-name');
        this.arrowSize = <HTMLInputElement>document.getElementById('dimension-style-dialog-arrow-size');
        this.tickSize = <HTMLInputElement>document.getElementById('dimension-style-dialog-tick-size');
        this.extensionLineOffset = <HTMLInputElement>document.getElementById('dimension-style-dialog-extension-line-offset');
        this.extensionLineExtension = <HTMLInputElement>document.getElementById('dimension-style-dialog-extension-line-extension');
        this.textHeight = <HTMLInputElement>document.getElementById('dimension-style-dialog-text-height');
        this.lineGap = <HTMLInputElement>document.getElementById('dimension-style-dialog-line-gap');
        this.lineColor = <HTMLDivElement>document.getElementById('dimension-style-dialog-line-color');
        this.textColor = <HTMLDivElement>document.getElementById('dimension-style-dialog-text-color');
        this.parameter = {
            CurrentDimensionStyleName: '',
            DimensionStyles: [],
        };

        this.lineColorPicker = new ColorPicker(this.lineColor, {
            initialColor: undefined,
            onColorChanged: (color, _colorAsHex) => {
                const style = this.getDimStyle(this.selector.value);
                style.LineColor = color;
            }
        });
        this.textColorPicker = new ColorPicker(this.textColor, {
            initialColor: undefined,
            onColorChanged: (color, _colorAsHex) => {
                const style = this.getDimStyle(this.selector.value);
                style.TextColor = color;
            }
        });

        this.selector.addEventListener('change', () => {
            this.parameter.CurrentDimensionStyleName = this.selector.value;
            this.populateValues(this.selector.value);
        });
        this.name.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.Name = this.name.value;
            this.populateValues(this.selector.value);
        });
        this.arrowSize.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.ArrowSize = parseFloat(this.arrowSize.value);
            this.populateValues(this.selector.value);
        });
        this.tickSize.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.TickSize = parseFloat(this.tickSize.value);
            this.populateValues(this.selector.value);
        });
        this.extensionLineOffset.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.ExtensionLineOffset = parseFloat(this.extensionLineOffset.value);
            this.populateValues(this.selector.value);
        });
        this.extensionLineExtension.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.ExtensionLineExtension = parseFloat(this.extensionLineExtension.value);
            this.populateValues(this.selector.value);
        });
        this.textHeight.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.TextHeight = parseFloat(this.textHeight.value);
            this.populateValues(this.selector.value);
        });
        this.lineGap.addEventListener('change', () => {
            const style = this.getDimStyle(this.selector.value);
            style.LineGap = parseFloat(this.lineGap.value);
            this.populateValues(this.selector.value);
        });

        [this.name, this.arrowSize, this.tickSize, this.extensionLineOffset, this.extensionLineExtension, this.textHeight, this.lineGap].forEach(input => {
            input.addEventListener('focus', () => {
                input.select();
            });
        });

        document.getElementById('dimension-style-dialog-add')!.addEventListener('click', () => {
            const newName = 'NEW_DIMENSION_STYLE';
            this.parameter.DimensionStyles.push({
                IsDeleted: false,
                Name: newName,
                OriginalName: '',
                ArrowSize: 0.18,
                TickSize: 0.0,
                ExtensionLineOffset: 0.0625,
                ExtensionLineExtension: 0.18,
                TextHeight: 0.18,
                LineGap: 0.09,
            });
            this.parameter.CurrentDimensionStyleName = newName;
            this.refreshSelector();
        });
        document.getElementById('dimension-style-dialog-delete')!.addEventListener('click', () => {
            const dimStyle = this.getDimStyle(this.selector.value);
            dimStyle.IsDeleted = true;
            const newSelected = this.parameter.DimensionStyles.find(ds => !ds.IsDeleted)!;
            this.parameter.CurrentDimensionStyleName = newSelected.Name;
            this.refreshSelector();
        });
    }

    dialogShowing(dialogOptions: object) {
        const dialogParameters = <DimensionStylesDialogParameters>dialogOptions;
        this.parameter = dialogParameters;
        this.refreshSelector();
    }

    dialogTitle(dialogOptions: object): string {
        return 'Dimension Styles';
    }

    dialogOk(): object {
        const result = this.parameter;

        LogWriter.write(`DIM-STYLES-DIALOG: OK, returning ${JSON.stringify(result)}`);
        return result;
    }

    dialogCancel() {
        // noop
    }

    private refreshSelector() {
        // update selector
        const selectOptions: HTMLOptionElement[] = [];
        for (const dimStyle of this.parameter.DimensionStyles.filter(ds => !ds.IsDeleted)) {
            const option = document.createElement('option');
            option.innerText = dimStyle.Name;
            option.value = dimStyle.OriginalName;
            if (dimStyle.Name === this.parameter.CurrentDimensionStyleName) {
                option.selected = true;
            }

            selectOptions.push(option);
        }
        this.selector.replaceChildren(...selectOptions);
        this.populateValues(this.selector.value);
    }

    private populateValues(selectedName: string) {
        // set individual values
        const style = this.getDimStyle(selectedName);
        this.name.value = style.Name;
        this.originalName.innerText = style.OriginalName;
        this.originalNameRow.hidden = style.OriginalName === style.Name;
        this.arrowSize.value = style.ArrowSize.toString();
        this.tickSize.value = style.TickSize.toString();
        this.extensionLineOffset.value = style.ExtensionLineOffset.toString();
        this.extensionLineExtension.value = style.ExtensionLineExtension.toString();
        this.textHeight.value = style.TextHeight.toString();
        this.lineGap.value = style.LineGap.toString();
        this.lineColorPicker.color = style.LineColor;
        this.textColorPicker.color = style.TextColor;
    }

    private getDimStyle(name: string): DimensionStylesDialogEntry {
        return this.parameter.DimensionStyles.find(s => s.OriginalName === name) || {
            IsDeleted: false,
            Name: name,
            OriginalName: name,
            ArrowSize: 0,
            TickSize: 0,
            ExtensionLineOffset: 0,
            ExtensionLineExtension: 0,
            TextHeight: 0,
            LineGap: 0,
        };
    }
}
