import { Client } from './client';

export class InputConsole {
    private client: Client;
    private input: HTMLInputElement;
    constructor(client: Client) {
        this.client = client;
        this.input = <HTMLInputElement>document.getElementById("input");
        this.input.addEventListener('keydown', (ev) => {
            this.handleLocalKeystroke(ev);
        });

        document.addEventListener('keydown', (ev) => {
            this.handleKeystroke(ev, true);
        });
    }

    private handleKeystroke(ev: KeyboardEvent, manualAppend: boolean) {
        switch (ev.key) {
            case "Enter":
            case " ":
                this.submit();
                break;
            case "Escape":
                this.clearInput();
                this.client.cancel();
                break;
            default:
                if (manualAppend && ev.key.length == 1) {
                    this.input.value += ev.key;
                }
                break;
        }
    }

    private handleLocalKeystroke(ev: KeyboardEvent) {
        ev.stopImmediatePropagation();
        this.handleKeystroke(ev, false);
    }

    private clearInput() {
        this.input.value = "";
    }

    private submit() {
        let value = this.input.value;
        this.clearInput();
        this.client.submitInput(value);
    }
}
