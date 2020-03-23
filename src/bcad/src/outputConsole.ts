import { Client } from './client';

export class OutputConsole {
    constructor(client: Client) {
        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate.OutputLines !== undefined) {
                let output = <HTMLTextAreaElement>document.getElementById("outputConsole");
                let content = output.value;
                for (let i = 0; i < clientUpdate.OutputLines.length; i++) {
                    if (content.length > 0) {
                        content += "\n";
                    }

                    content = content + clientUpdate.OutputLines[i];
                }

                output.value = content;
                output.scrollTop = output.scrollHeight;
            }
        });
    }
}
