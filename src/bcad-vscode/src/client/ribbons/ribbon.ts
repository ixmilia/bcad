import { Client } from '../client';
import { SettingsRibbon } from './settingsRibbon';
import { DebugRibbon } from './debugRibbon';

export class Ribbon {
    constructor(client: Client) {
        document.querySelectorAll(".command-button").forEach(node => {
            let button = <HTMLButtonElement>node;
            let commandName = button.getAttribute("data-command-name");
            if (commandName) {
                button.addEventListener('click', () => {
                    client.executeCommand(commandName!);
                });
            }
        });

        var tabs = [
            "home",
            "view",
            "settings",
            "debug",
        ];
        tabs.forEach(t => {
            var button = <HTMLButtonElement>document.getElementById(t + "Button");
            var ribbon = <HTMLButtonElement>document.getElementById(t + "Ribbon");
            button.addEventListener('click', () => {
                document.querySelectorAll(".ribbon-selector").forEach(node => node.classList.remove('ribbon-selector-active'));
                document.querySelectorAll(".ribbon").forEach(node => node.classList.remove('ribbon-active'));
                button.classList.add('ribbon-selector-active');
                ribbon.classList.add('ribbon-active');
            });
        });

        new SettingsRibbon(client);
        new DebugRibbon(client);
    }
}
