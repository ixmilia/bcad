import { Client } from './client';

export class InputConsole {
    private client: Client;
    private input: HTMLInputElement;
    constructor(client: Client) {
        this.client = client;
        this.input = <HTMLInputElement>document.getElementById("input");
        this.input.addEventListener('keyup', (ev) => {
            this.handleLocalKeystroke(ev);
        });

        document.addEventListener('keyup', (ev) => {
            this.handleKeystroke(ev, true);
        });
    }

    private handleKeystroke(ev: KeyboardEvent, manualEdit: boolean) {
        switch (ev.key) {
            case "Enter":
            case " ":
                this.submit(ev.key === " ");
                break;
            case "Escape":
                this.clearInput();
                this.client.cancel();
                break;
            case "Backspace":
                if (manualEdit && this.input.value.length > 0) {
                    this.input.value = this.input.value.substr(0, this.input.value.length - 1);
                }
                break;
            default:
                if (manualEdit &&
                    ev.key.length == 1 &&
                    !ev.altKey &&
                    !ev.ctrlKey) {
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

    private submit(trimLastSpace: boolean) {
        let value = this.input.value;
        this.clearInput();
        if (trimLastSpace && value.length > 0 && value.charAt(value.length - 1) == " ") {
            // if submitted with SPACE, last character needs to be removed
            value = value.substr(0, value.length - 1);
        }

        this.input.focus();
        this.client.submitInput(value);
    }
}
