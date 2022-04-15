import { LogWriter } from '../logWriter';
import { ColorPicker } from '../controls/colorPicker';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

interface LayerInfo {
    Name: string;
    Color: string | undefined;
    IsVisible: boolean;
}

interface LayerDialogOptions {
    Layers: LayerInfo[],
}

interface ChangedLayer {
    OldLayerName: string,
    NewLayerName: string,
    Color: string | null,
    IsVisible: boolean,
}

interface LayerDialogResult {
    ChangedLayers: ChangedLayer[],
}

export class LayerDialog extends DialogBase {
    private tableBody: HTMLDivElement;

    constructor(dialogHandler: DialogHandler) {
        super(dialogHandler, "layer");
        this.tableBody = <HTMLDivElement>document.getElementById('dialog-layer-list');
        document.getElementById('dialog-layer-add')!.addEventListener('click', () => {
            this.addLayerRow({
                Name: "NewLayer",
                Color: undefined,
                IsVisible: true,
            });
        });
    }

    dialogShowing(dialogOptions: object) {
        let layerDialogOptions = <LayerDialogOptions>dialogOptions;
        this.tableBody.innerHTML = '';
        for (let layer of layerDialogOptions.Layers) {
            this.addLayerRow(layer);
        }
    }

    dialogTitle(dialogOptions: object): string {
        return 'Layers';
    }

    dialogOk(): object {
        let changedLayers: ChangedLayer[] = [];
        for (let element of this.tableBody.children) {
            let row = <HTMLTableRowElement>element;
            let nameInput = <HTMLInputElement>row.children[0].children[0];
            let colorInput = <HTMLDivElement>row.children[1].children[0];
            let visibleInput = <HTMLInputElement>row.children[2].children[0];
            let changed = {
                OldLayerName: nameInput.getAttribute('data-original-value')!,
                NewLayerName: nameInput.value,
                Color: colorInput.getAttribute('data-color'),
                IsVisible: visibleInput.checked,
            };
            changedLayers.push(changed);
        }
        let result: LayerDialogResult = {
            ChangedLayers: changedLayers,
        };

        LogWriter.write(`LAYER-DIALOG: OK, returning ${JSON.stringify(result)}`);
        return result;
    }

    dialogCancel() {
        // noop
    }

    private addLayerRow(layer: LayerInfo) {
        // each row will look like this:
        // <div class="dialog-layer-row">
        //   <div class="dialog-layer-name-column"><input type="text" class="dialog-layer-list-layer-name" value="0" data-original-value="0"></input></div>
        //   <div class="dialog-layer-color-column"><div data-color="#FFFFFF">...</div></div>
        //   <div class="dialog-layer-visible-column"><input type="checkbox" class="dialog-layer-list-layer-visible" id="..."></input><label for="...">&nbsp;</label></div>
        //   <div class="dialog-layer-command-column"><button>Del</button></div>
        // </div>
        LogWriter.write(`LAYER-DIALOG: adding layer ${JSON.stringify(layer)}`);
        const row = document.createElement('div');
        row.classList.add('dialog-layer-row');

        // name
        const layerInput = document.createElement('input');
        layerInput.classList.add('dialog-layer-list-layer-name');
        layerInput.setAttribute('type', 'text');
        layerInput.setAttribute('value', layer.Name);
        layerInput.setAttribute('data-original-value', layer.Name);
        const nameColumn = document.createElement('div');
        nameColumn.classList.add('dialog-layer-name-column');
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
        colorColumn.classList.add('dialog-layer-color-column');
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
        visibleColumn.classList.add('dialog-layer-visible-column');
        visibleColumn.appendChild(visibleInput);
        visibleColumn.appendChild(visibleInputLabel);
        row.appendChild(visibleColumn);
        // delete
        const deleteButton = document.createElement('button');
        deleteButton.innerText = "Delete";
        deleteButton.addEventListener('click', () => {
            row.remove();
        });
        const commandColumn = document.createElement('div');
        commandColumn.classList.add('dialog-layer-command-column');
        commandColumn.appendChild(deleteButton);
        row.appendChild(commandColumn);

        this.tableBody.appendChild(row);
    }
}
