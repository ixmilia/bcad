// All of the Node.js APIs are available in the preload process.
// It has the same sandbox as a Chrome extension.
window.addEventListener("DOMContentLoaded", () => {
    const replaceText = (selector: string, text: string) => {
        const element = document.getElementById(selector);
        if (element) {
            element.innerText = text;
        }
    };

    for (const type of ["chrome", "node", "electron"]) {
        replaceText(`${type}-version`, (process.versions as any)[type]);
    }

    const client = require('./client/main.js');
    client.start(process.argv).catch((err: any) => {
        const errorMessage = `error: ${err}`;
        console.error(errorMessage);
        let output = <HTMLTextAreaElement>document.getElementById("outputConsole");
        if (output.value.length > 0) {
            output.value += '\n';
        }
        output.value += errorMessage;
    });;
});

// @ts-ignore
window.ipcRenderer = require('electron').ipcRenderer;
