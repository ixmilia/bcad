import { Client } from "./client";

export class DebugRibbon {
    constructor(client: Client) {
        client.subscribeToClientUpdates((clientUpdate) => {
            if (clientUpdate.Transform !== undefined) {
                let digits = 4;
                for (let i = 0; i < 16; i++) {
                    let r = Math.trunc(i / 4) + 1;
                    let c = (i % 4) + 1;
                    document.getElementById(`debug-transform-m${r}${c}`).innerText = clientUpdate.Transform.Transform[i].toFixed(digits);
                }
            }
        });
    }
}
