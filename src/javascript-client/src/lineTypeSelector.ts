import { Client } from './client';
import { ClientUpdate } from './contracts.generated';

export class LineTypeSelector {
    private listenToEvents: boolean = true;
    private selector: HTMLSelectElement;

    constructor(client: Client) {
        this.selector = <HTMLSelectElement>document.getElementById('current-line-type-selector');
        this.selector.addEventListener('change', (_ev) => {
            if (this.listenToEvents) {
                let index = this.selector.selectedIndex;
                if (index >= 0) {
                    let item = this.selector.options[index];
                    let lineTypeName = item.innerText;
                    client.changeCurrentLineType(lineTypeName);
                }
            }
        });
        client.subscribeToClientUpdates(clientUpdate => this.update(clientUpdate));
    }

    private update(clientUpdate: ClientUpdate) {
        if (clientUpdate.Drawing !== undefined) {
            this.buildSelector(clientUpdate.Drawing.CurrentLineType, clientUpdate.Drawing.LineTypes);
        }
    }

    private buildSelector(currentLineType: string | undefined, lineTypes: string[]) {
        this.listenToEvents = false;
        for (let i = this.selector.options.length; i >= 0; i--) {
            this.selector.options.remove(i);
        }

        let selectedIndex = 0;
        let element = <HTMLOptionElement>document.createElement('option');
        element.innerText = '(Auto)';
        this.selector.options.add(element);
        for (let i = 0; i < lineTypes.length; i++) {
            let element = <HTMLOptionElement>document.createElement('option');
            element.innerText = lineTypes[i];
            this.selector.options.add(element);
            if (lineTypes[i] === currentLineType) {
                selectedIndex = this.selector.options.length - 1;
            }
        }

        this.selector.selectedIndex = selectedIndex;
        this.listenToEvents = true;
    }
}
