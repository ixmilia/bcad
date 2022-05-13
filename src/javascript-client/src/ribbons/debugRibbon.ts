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

            if (clientUpdate.Drawing) {
                document.getElementById('debug-ribbon-line-count')!.innerText = `${clientUpdate.Drawing.Lines.length}`;
                document.getElementById('debug-ribbon-ellipse-count')!.innerText = `${clientUpdate.Drawing.Ellipses.length}`;
                document.getElementById('debug-ribbon-text-count')!.innerText = `${clientUpdate.Drawing.Text.length}`;
                document.getElementById('debug-ribbon-point-count')!.innerText = `${clientUpdate.Drawing.Points.length}`;
            }
        });
    }

    public setFps(fps: number) {
        document.getElementById('debug-ribbon-fps')!.innerText = `${fps}`;
    }
}
