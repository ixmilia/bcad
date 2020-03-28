import { DialogHandler } from "./dialogHandler";

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

export class LayerDialog {
    private tableBody: HTMLTableSectionElement;

    constructor(dialogHandler: DialogHandler) {
        this.tableBody = <HTMLTableSectionElement>document.getElementById('dialog-layer-list-body');
        document.getElementById('dialog-layer-add').addEventListener('click', () => {
            this.addLayerRow({
                Name: "NewLayer",
                Color: "",
                IsVisible: true,
            });
        });
        dialogHandler.registerDialogHandler("layer", (dialogOptions) => {
            let layerDialogOptions = <LayerDialogOptions>dialogOptions;
            this.bindDialog(layerDialogOptions);
            let promise = new Promise<LayerDialogResult>(
                (resolve, reject) => {
                    var okButton = <HTMLButtonElement>document.getElementById('dialog-layer-ok');
                    okButton.addEventListener('click', () => {
                        let result = this.gatherResults();
                        resolve(result);
                    });

                    var cancelButton = <HTMLButtonElement>document.getElementById('dialog-layer-cancel');
                    cancelButton.addEventListener('click', () => {
                        reject();
                    });
                });
            return promise;
        });
    }

    private addLayerRow(layer: LayerInfo) {
        // each row will look like this:
        // <tr>
        //   <td><input type="text" class="dialog-layer-list-layer-name" value="0" data-original-value="0"></input></td>
        //   <td><input type="color" class="dialog-layer-list-layer-color" value="#FFFFFF"></input></td>
        //   <td><input type="checkbox" class="dialog-layer-list-layer-color-auto"></input></td>
        //   <td><input type="checkbox" class="dialog-layer-list-layer-visible"></input></td>
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
        colorAutoInput.checked = layer.Color === '';
        colorAutoInput.addEventListener('change', () => {
            colorInput.disabled = colorAutoInput.checked;
        });
        let tdColorAuto = document.createElement('td');
        tdColorAuto.appendChild(colorAutoInput);
        row.appendChild(tdColorAuto);
        // visibility
        let visibleInput = document.createElement('input');
        visibleInput.classList.add('dialog-layer-list-layer-visible');
        visibleInput.setAttribute('type', 'checkbox');
        if (layer.IsVisible) {
            visibleInput.setAttribute('checked', 'checked');
        }
        let tdVisible = document.createElement('td');
        tdVisible.appendChild(visibleInput);
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

    private bindDialog(options: LayerDialogOptions) {
        this.tableBody.innerHTML = '';
        for (let layer of options.Layers) {
            this.addLayerRow(layer);
        }
    }

    private gatherResults(): LayerDialogResult {
        let changedLayers: ChangedLayer[] = [];
        for (let element of this.tableBody.children) {
            let row = <HTMLTableRowElement>element;
            let nameInput = <HTMLInputElement>row.children[0].children[0];
            let colorInput = <HTMLInputElement>row.children[1].children[0];
            let colorAutoInput = <HTMLInputElement>row.children[2].children[0];
            let visibleInput = <HTMLInputElement>row.children[3].children[0];
            let changed = {
                OldLayerName: nameInput.getAttribute('data-original-value'),
                NewLayerName: nameInput.value,
                Color: colorAutoInput.checked ? "" : colorInput.value,
                IsVisible: visibleInput.checked,
            };
            changedLayers.push(changed);
        }
        return {
            ChangedLayers: changedLayers,
        };
    }
}
