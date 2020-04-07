import { Client } from "../client";

export class SettingsRibbon {
    constructor(client: Client) {
        document.querySelectorAll(".settings-button").forEach(node => {
            let button = <HTMLButtonElement>node;
            let name = button.getAttribute("data-setting-name");
            let value = button.getAttribute("data-setting-value");
            if (name && value) {
                button.addEventListener('click', () => {
                    client.setSetting(name, value);
                });
            }
        });
    }
}
