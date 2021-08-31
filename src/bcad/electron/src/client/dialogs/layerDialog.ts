import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

interface LayerInfo {
    Name: string,
    Color: string,
    IsVisible: boolean,
}

interface LayerDialogOptions {
    Layers: LayerInfo[],
}

interface ChangedLayer {
    OldLayerName: string,
    NewLayerName: string,
    Color: string,
    IsVisible: boolean,
}

interface LayerDialogResult {
    ChangedLayers: ChangedLayer[],
}

export class LayerDialog extends DialogBase {
    private tableBody: HTMLTableSectionElement;

    constructor(dialogHandler: DialogHandler) {
        super(dialogHandler, "layer");
        this.tableBody = <HTMLTableSectionElement>document.getElementById('dialog-layer-list-body');
        document.getElementById('dialog-layer-add')!.addEventListener('click', () => {
            this.addLayerRow({
                Name: "NewLayer",
                Color: "",
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

    dialogOk(): object {
        let changedLayers: ChangedLayer[] = [];
        for (let element of this.tableBody.children) {
            let row = <HTMLTableRowElement>element;
            let nameInput = <HTMLInputElement>row.children[0].children[0];
            let colorInput = <HTMLInputElement>row.children[1].children[0];
            let colorAutoInput = <HTMLInputElement>row.children[2].children[0];
            let visibleInput = <HTMLInputElement>row.children[3].children[0];
            let changed = {
                OldLayerName: nameInput.getAttribute('data-original-value')!,
                NewLayerName: nameInput.value,
                Color: colorAutoInput.checked ? "" : colorInput.value,
                IsVisible: visibleInput.checked,
            };
            changedLayers.push(changed);
        }
        let result: LayerDialogResult = {
            ChangedLayers: changedLayers,
        };
        return result;
    }

    dialogCancel() {
        // noop
    }

    private addLayerRow(layer: LayerInfo) {
        // each row will look like this:
        // <tr>
        //   <td><input type="text" class="dialog-layer-list-layer-name" value="0" data-original-value="0"></input></td>
        //   <td><input type="color" class="dialog-layer-list-layer-color" value="#FFFFFF"></input></td>
        //   <td><input type="checkbox" class="dialog-layer-list-layer-color-auto" id="..."></input><label for="...">&nbsp;</label></td>
        //   <td><input type="checkbox" class="dialog-layer-list-layer-visible" id="..."></input><label for="...">&nbsp;</label></td>
        //   <td><button>Del</button></td>
        // </tr>
        let row = document.createElement('tr');

        // name
        let layerInput = document.createElement('input');
        layerInput.classList.add('dialog-layer-list-layer-name');
        layerInput.setAttribute('type', 'text');
        layerInput.setAttribute('value', layer.Name);
        layerInput.setAttribute('data-original-value', layer.Name);
        let tdName = document.createElement('td');
        tdName.appendChild(layerInput);
        row.appendChild(tdName);
        // color
        let colorInput = document.createElement('input');
        colorInput.classList.add('dialog-layer-list-layer-color');
        colorInput.setAttribute('type', 'color');
        colorInput.setAttribute('value', layer.Color);
        colorInput.disabled = layer.Color === '';
        let tdColor = document.createElement('td');
        tdColor.appendChild(colorInput);
        row.appendChild(tdColor);
        // color auto
        let colorAutoInput = document.createElement('input');
        colorAutoInput.classList.add('dialog-layer-list-layer-color-auto');
        colorAutoInput.setAttribute('type', 'checkbox');
        colorAutoInput.setAttribute('id', `layer-color-auto-${layer.Name}`);
        colorAutoInput.checked = layer.Color === '';
        colorAutoInput.addEventListener('change', () => {
            colorInput.disabled = colorAutoInput.checked;
        });
        let colorAutoInputLabel = document.createElement('label');
        colorAutoInputLabel.setAttribute('for', `layer-color-auto-${layer.Name}`);
        colorAutoInputLabel.innerHTML = '&nbsp;';
        let tdColorAuto = document.createElement('td');
        tdColorAuto.appendChild(colorAutoInput);
        tdColorAuto.appendChild(colorAutoInputLabel);
        row.appendChild(tdColorAuto);
        // visibility
        let visibleInput = document.createElement('input');
        visibleInput.classList.add('dialog-layer-list-layer-visible');
        visibleInput.setAttribute('type', 'checkbox');
        visibleInput.setAttribute('id', `layer-visible-${layer.Name}`);
        if (layer.IsVisible) {
            visibleInput.setAttribute('checked', 'checked');
        }
        let visibleInputLabel = document.createElement('label');
        visibleInputLabel.setAttribute('for', `layer-visible-${layer.Name}`);
        visibleInputLabel.innerHTML = '&nbsp;';
        let tdVisible = document.createElement('td');
        tdVisible.appendChild(visibleInput);
        tdVisible.appendChild(visibleInputLabel);
        row.appendChild(tdVisible);
        // delete
        let deleteButton = document.createElement('button');
        deleteButton.innerText = "Del";
        deleteButton.addEventListener('click', () => {
            row.remove();
        });
        let tdDelete = document.createElement('td');
        tdDelete.appendChild(deleteButton);
        row.appendChild(tdDelete);

        this.tableBody.appendChild(row);
    }
}
