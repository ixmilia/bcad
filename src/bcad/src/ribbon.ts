import { Client } from './client';

export class Ribbon {
    constructor(client: Client) {
        document.querySelectorAll(".commandButton").forEach(node => {
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
            var button = <HTMLButtonElement> document.getElementById(t + "Button");
            var ribbon = <HTMLButtonElement> document.getElementById(t + "Ribbon");
            button.addEventListener('click', () => {
                document.querySelectorAll(".ribbon").forEach(node => (<HTMLElement>node).style.display = "none");
                document.querySelectorAll(".tabButton").forEach(node => (<HTMLButtonElement>node).style.background = "lightGray");
                button.style.background = "white";
                ribbon.style.height = document.getElementById("ribbon").style.height;
                ribbon.style.display = "block";
            });
        });

        (<HTMLButtonElement>document.getElementById("homeButton")).click();
    }
}
