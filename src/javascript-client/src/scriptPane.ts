import { Client } from './client';
import * as contracts from './contracts.generated';
import * as monaco from 'monaco-editor';

export class ScriptPane {
    constructor(client: Client) {
        const scriptPane = <HTMLDivElement>document.getElementById("script-pane");
        const scriptEditor = <HTMLDivElement>document.getElementById("script-content");
        ScriptPane.captureUserGestures(scriptPane);

        // prepare monaco editor
        const languageId = 'cad';
        monaco.languages.register({
            id: languageId
        });
        monaco.languages.setMonarchTokensProvider(languageId, {
            ignoreCase: true,
            keywords: contracts.CommandNames,
            tokenizer: {
                root: [
                    [/[+-]?\d+(?:(?:\.\d*)?(?:[eE][+-]?\d+)?)?/, 'number'],
                    [/"[^"]*"/, 'string'],
                    [/"[^"]*$/, 'string.invalid'],
                    [/^;.*/, 'comment'],
                    [/[a-zA-Z_#][a-zA-Z0-9_\-\?\!\*.]*/, {
                        cases: {
                            '@keywords': 'keyword',
                            '@default': 'identifier'
                        }
                    }],
                ],
            }
        });
        const editor = monaco.editor.create(scriptEditor, {
            value: '; add your script here',
            language: languageId,
        });

        const showScriptToggle = <HTMLInputElement>document.getElementById('showScript');
        showScriptToggle.addEventListener('change', ev => {
            if (showScriptToggle.checked) {
                scriptPane.style.display = 'block';
                editor.layout();
            } else {
                scriptPane.style.display = 'none';
            }
        });

        // prepare script execution
        function executeScript() {
            const scriptContent = editor.getValue();
            client.executeScript(scriptContent);
        }
        const runScriptButton = <HTMLButtonElement>document.getElementById("run-script");
        runScriptButton.addEventListener('click', ev => {
            executeScript();
        });

        // add ctrl+enter shortcut to run script
        editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, () => {
            executeScript();
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
