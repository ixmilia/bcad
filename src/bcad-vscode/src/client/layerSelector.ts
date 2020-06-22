import { Client } from './client';
import { ClientUpdate } from './contracts.generated';

export class LayerSelector {
    private listenToEvents: boolean = true;
    private selector: HTMLSelectElement;

    constructor(client: Client) {
        this.selector = <HTMLSelectElement>document.getElementById('current-layer-selector');
        this.selector.addEventListener('change', (_ev) => {
            if (this.listenToEvents) {
                let index = this.selector.selectedIndex;
                if (index >= 0) {
                    let item = this.selector.options[index];
                    let layerName = item.innerText;
                    client.changeCurrentLayer(layerName);
                }
            }
        });
        client.subscribeToClientUpdates(clientUpdate => this.update(clientUpdate));
    }

    private update(clientUpdate: ClientUpdate) {
        if (clientUpdate.Drawing !== undefined) {
            this.buildSelector(clientUpdate.Drawing.CurrentLayer!, clientUpdate.Drawing.Layers);
        }
    }

    private buildSelector(currentLayer: string, layers: string[]) {
        this.listenToEvents = false;
        for (let i = this.selector.options.length; i >= 0; i--) {
            this.selector.options.remove(i);
        }

        let selectedIndex = 0;
        for (let i = 0; i < layers.length; i++) {
            let element = <HTMLOptionElement>document.createElement('option');
            element.innerText = layers[i];
            this.selector.options.add(element);
            if (layers[i] === currentLayer) {
                selectedIndex = i;
            }
        }

        this.selector.selectedIndex = selectedIndex;
        this.listenToEvents = true;
    }
}
