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
            "draw",
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
                const rootRibbon = document.getElementById('ribbon');
                rootRibbon?.classList.remove('ribbon-unpinned-hide');
            });
        });

        document.getElementById('ribbon')?.addEventListener('mouseleave', (ev) => {
            const ribbon = <HTMLDivElement>ev.target;
            if (ribbon.classList.contains('ribbon-unpinned-show')) {
                ribbon.classList.add('ribbon-unpinned-hide');
            }
        });

        document.getElementById('ribbon-is-pinned')?.addEventListener('click', (ev) => {
            const ribbon = document.getElementById('ribbon')!;
            const checkbox = <HTMLInputElement>ev.target;
            if (checkbox.checked) {
                ribbon.classList.remove('ribbon-unpinned-show', 'ribbon-unpinned-hide');
            } else {
                ribbon.classList.add('ribbon-unpinned-show');
            }
        });

        new SettingsRibbon(client);
        new DebugRibbon(client);
    }
}
