import { Client } from "../client";

export class AnnotationRibbon {
    private currentDimensionStyleSelector: HTMLSelectElement;

    constructor(client: Client) {
        this.currentDimensionStyleSelector = <HTMLSelectElement>document.getElementById("current-dimension-style-selector");
        this.currentDimensionStyleSelector.addEventListener("change", (ev) => {
            client.changeCurrentDimensionStyle((<any>ev.target).value.toString());
        });

        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate.Drawing !== undefined) {
                const options: HTMLOptionElement[] = [];
                for (const dimStyle of clientUpdate.Drawing.DimensionStyles) {
                    const option = document.createElement('option');
                    option.innerText = dimStyle;
                    option.value = dimStyle;
                    option.selected = dimStyle === clientUpdate.Drawing.CurrentDimensionStyle;
                    options.push(option);
                }

                this.currentDimensionStyleSelector.replaceChildren(...options);
            }
        });
    }
}
