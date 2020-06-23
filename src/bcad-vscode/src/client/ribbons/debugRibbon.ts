import { Client } from "../client";

export class DebugRibbon {
    constructor(client: Client) {
        client.subscribeToClientUpdates((clientUpdate) => {
            if (clientUpdate?.Transform?.Transform?.length === 16) {
                let digits = 6;
                for (let i = 0; i < 16; i++) {
                    const r = Math.trunc(i / 4) + 1;
                    const c = (i % 4) + 1;
                    const v = clientUpdate.Transform.Transform![i];
                    const d = v.toFixed(digits);
                    document.getElementById(`debug-transform-m${r}${c}`)!.innerText = d;
                }
            }
        });
    }
}
