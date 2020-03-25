import { Client } from './client';

export class Ribbon {
    constructor(client: Client) {
        document.querySelectorAll(".command-button").forEach(node => {
            let button = <HTMLButtonElement>node;
            button.addEventListener('click', () => {
                client.executeCommand(button.getAttribute("data-command-name"));
            });
        });

        var tabs = [
            "home",
            "view",
            "settings",
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
    }
}
