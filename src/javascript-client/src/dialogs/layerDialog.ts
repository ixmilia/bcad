import { LogWriter } from '../logWriter';
import { ColorPicker } from '../controls/colorPicker';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';
import { Client } from '../client';
import {
    ClientChangedLayer,
    ClientLayer,
    ClientLayerParameters,
    ClientLayerResult,
} from '../contracts.generated';

export class LayerDialog extends DialogBase {
    private tableBody: HTMLDivElement;
    private lineTypeNames: string[] = [];

    constructor(dialogHandler: DialogHandler, client: Client) {
        super(dialogHandler, "layer");
        this.tableBody = <HTMLDivElement>document.getElementById('dialog-layer-list');
        document.getElementById('dialog-layer-add')!.addEventListener('click', () => {
            this.addLayerRow({
                Name: "NewLayer",
                Color: undefined,
                IsVisible: true,
                LineTypeName: undefined,
                LineTypeScale: undefined,
            });
        });
        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate.Drawing?.LineTypes) {
                this.lineTypeNames = clientUpdate.Drawing.LineTypes;
            }
        });
    }

    dialogShowing(dialogOptions: object) {
        const layerDialogOptions = <ClientLayerParameters>dialogOptions;
        this.tableBody.innerHTML = '';
        for (const layer of layerDialogOptions.Layers) {
            this.addLayerRow(layer);
        }
    }

    dialogTitle(dialogOptions: object): string {
        return 'Layers';
    }

    dialogOk(): object {
        const changedLayers: ClientChangedLayer[] = [];
        for (const element of this.tableBody.children) {
            const row = <HTMLTableRowElement>element;
            const nameInput = <HTMLInputElement>row.children[0].children[0];
            const colorInput = <HTMLDivElement>row.children[1].children[0];
            const visibleInput = <HTMLInputElement>row.children[2].children[0];
            const lineTypeNameInput = <HTMLSelectElement>row.children[3].children[0];
            const lineTypeScaleInput = <HTMLSelectElement>row.children[4].children[0];
            const changed: ClientChangedLayer = {
                OldLayerName: nameInput.getAttribute('data-original-value')!,
                NewLayerName: nameInput.value,
                Color: colorInput.getAttribute('data-color') || undefined,
                IsVisible: visibleInput.checked,
                LineTypeName: lineTypeNameInput.selectedIndex === 0 ? undefined : lineTypeNameInput.value,
                LineTypeScale: parseFloat(lineTypeScaleInput.value) || 1.0,
            };
            changedLayers.push(changed);
        }
        const result: ClientLayerResult = {
            ChangedLayers: changedLayers,
        };

        LogWriter.write(`LAYER-DIALOG: OK, returning ${JSON.stringify(result)}`);
        return result;
    }

    dialogCancel() {
        // noop
    }

    private addLayerRow(layer: ClientLayer) {
        // each row will look like this:
        // <div class="dialog-layer-row">
        //   <div class="dialog-layer-column-1"><input type="text" value="0" data-original-value="0"></input></div>
        //   <div class="dialog-layer-column-2"><div data-color="#FFFFFF">...</div></div>
        //   <div class="dialog-layer-column-3"><input type="checkbox" id="..."></input><label for="...">&nbsp;</label></div>
        //   <div class="dialog-layer-column-4"><select... /><label for="...">&nbsp;</label></div>
        //   <div class="dialog-layer-column-5"><input type="text" id="..."></input><label for="...">&nbsp;</label></div>
        //   <div class="dialog-layer-column-6"><button>Del</button></div>
        // </div>
        LogWriter.write(`LAYER-DIALOG: adding layer ${JSON.stringify(layer)}`);
        const row = document.createElement('div');
        row.classList.add('dialog-layer-row');

        // name
        const layerInput = document.createElement('input');
        layerInput.style.width = '100%';
        layerInput.setAttribute('type', 'text');
        layerInput.setAttribute('value', layer.Name);
        layerInput.setAttribute('data-original-value', layer.Name);
        const nameColumn = document.createElement('div');
        nameColumn.classList.add('dialog-layer-column-1');
        nameColumn.appendChild(layerInput);
        row.appendChild(nameColumn);
        // color
        const colorContainer = document.createElement('div');
        if (layer.Color) {
            colorContainer.setAttribute('data-color', layer.Color);
        }
        const _colorPicker = new ColorPicker(colorContainer, {
            initialColor: layer.Color,
            onColorChanged: (_color, colorAsHex) => {
                LogWriter.write(`LAYER-DIALOG: color changed to ${colorAsHex}`);
                if (colorAsHex) {
                    colorContainer.setAttribute('data-color', colorAsHex);
                } else {
                    colorContainer.removeAttribute('data-color');
                }
            },
        });
        const colorColumn = document.createElement('div');
        colorColumn.classList.add('dialog-layer-column-2');
        colorColumn.appendChild(colorContainer);
        row.appendChild(colorColumn);
        // visibility
        const visibleInput = document.createElement('input');
        visibleInput.setAttribute('type', 'checkbox');
        visibleInput.setAttribute('id', `layer-visible-${layer.Name}`);
        if (layer.IsVisible) {
            visibleInput.setAttribute('checked', 'checked');
        }
        const visibleInputLabel = document.createElement('label');
        visibleInputLabel.setAttribute('for', `layer-visible-${layer.Name}`);
        visibleInputLabel.innerHTML = '&nbsp;';
        const visibleColumn = document.createElement('div');
        visibleColumn.classList.add('dialog-layer-column-3');
        visibleColumn.appendChild(visibleInput);
        visibleColumn.appendChild(visibleInputLabel);
        row.appendChild(visibleColumn);
        // line type name
        const lineTypeNameSelect = document.createElement('select');
        lineTypeNameSelect.style.width = '100%';
        const autoOption = document.createElement('option');
        autoOption.value = '(Auto)';
        autoOption.innerText = '(Auto)';
        if (!layer.LineTypeName) {
            autoOption.selected = true;
        }
        lineTypeNameSelect.appendChild(autoOption);
        for (const lineTypeName of this.lineTypeNames) {
            const option = document.createElement('option');
            option.value = lineTypeName;
            option.innerText = lineTypeName;
            if (layer.LineTypeName === lineTypeName) {
                option.selected = true;
            }
            lineTypeNameSelect.appendChild(option);
        }
        const lineTypeNameColumn = document.createElement('div');
        lineTypeNameColumn.classList.add('dialog-layer-column-4');
        lineTypeNameColumn.appendChild(lineTypeNameSelect);
        row.appendChild(lineTypeNameColumn);
        // line type scale
        const lineTypeScaleInput = document.createElement('input');
        lineTypeScaleInput.setAttribute('type', 'text');
        lineTypeScaleInput.setAttribute('value', layer.LineTypeScale?.toString() || '');
        lineTypeScaleInput.style.width = '100%';
        const lineTypeScaleColumn = document.createElement('div');
        lineTypeScaleColumn.classList.add('dialog-layer-column-5');
        lineTypeScaleColumn.appendChild(lineTypeScaleInput);
        row.appendChild(lineTypeScaleColumn);
        // delete
        const deleteButton = document.createElement('button');
        deleteButton.innerText = "Delete";
        deleteButton.addEventListener('click', () => {
            row.remove();
        });
        const commandColumn = document.createElement('div');
        commandColumn.classList.add('dialog-layer-column-6');
        commandColumn.appendChild(deleteButton);
        row.appendChild(commandColumn);

        this.tableBody.appendChild(row);
    }
}
