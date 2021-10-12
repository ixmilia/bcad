import { Client } from "../client";

export class SettingsRibbon {
    constructor(client: Client) {
        document.querySelectorAll(".settings-button").forEach(node => {
            let button = <HTMLButtonElement>node;
            let name = button.getAttribute("data-setting-name");
            let value = button.getAttribute("data-setting-value");
            if (name && value) {
                button.addEventListener('click', () => {
                    client.setSetting(name!, value!);
                });
            }
        });

        this.getSnapAngleSelectors().forEach(input => {
            input.addEventListener('change', () => {
                client.setSetting("Display.SnapAngles", input.value);
            });
        });

        client.subscribeToClientUpdates((clientUpdate) => {
            if (clientUpdate.Settings !== undefined) {
                let snapAnglesString = clientUpdate.Settings.SnapAngles.join(";");
                for (let sna of this.getSnapAngleSelectors()) {
                    if (sna.value === snapAnglesString) {
                        sna.checked = true;
                        break;
                    }
                }
            }
        });
    }

    private getSnapAngleSelectors(): HTMLInputElement[] {
        let inputs: HTMLInputElement[] = [];
        document.querySelectorAll(".snap-angle-selector").forEach(node => {
            let input = <HTMLInputElement>node;
            inputs.push(input);
        });

        return inputs;
    }
}
