import { Client } from './client';

export class ScriptPane {
    constructor(client: Client) {
        const scriptPane = <HTMLDivElement>document.getElementById("script-pane");
        const showScriptToggle = <HTMLInputElement>document.getElementById('showScript');
        showScriptToggle.addEventListener('change', ev => {
            if (showScriptToggle.checked) {
                scriptPane.style.display = 'block';
            } else {
                scriptPane.style.display = 'none';
            }
        });

        const textArea = <HTMLTextAreaElement>document.getElementById("script-content");
        ScriptPane.captureUserGestures(scriptPane);
        const runScriptButton = <HTMLButtonElement>document.getElementById("run-script");
        runScriptButton.addEventListener('click', ev => {
            const scriptContent = textArea.value;
            client.executeScript(scriptContent);
        });
    }

    static captureUserGestures(element: HTMLElement) {
        const eventList = ['keydown', 'keyup', 'mousedown', 'mouseup', 'mousemove'];
        eventList.forEach(eventName => {
            element.addEventListener(eventName, ev => {
                ev.stopPropagation();
            });
        });
    }
}
