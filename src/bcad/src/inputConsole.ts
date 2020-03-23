import { Client } from './client';

export class InputConsole {
    constructor(client: Client) {
        let input = <HTMLInputElement>document.getElementById("input");
        input.addEventListener('keydown', (ev) => {
            if (ev.key == "Enter") {
                let value = input.value;
                input.value = "";
                client.submitInput(value);
            }
        });
    }
}
