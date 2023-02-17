import { Client } from './client';
import * as contracts from './contracts.generated';
import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
import * as lispGrammar from './grammar/grammar';
import { InputConsole } from './inputConsole';

export class ScriptPane {
    constructor(client: Client) {
        const scriptPane = <HTMLDivElement>document.getElementById("script-pane");
        const scriptEditor = <HTMLDivElement>document.getElementById("script-content");
        const scriptTypeSelector = <HTMLSelectElement>document.getElementById("script-type");
        InputConsole.ensureCapturedEvents(scriptPane);

        // prepare monaco editor
        const scriptLanguageId = 'scr';
        monaco.languages.register({
            id: scriptLanguageId
        });
        const lispLanguageId = 'lisp';
        monaco.languages.register({
            id: lispLanguageId
        });
        monaco.languages.setMonarchTokensProvider(scriptLanguageId, {
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
        monaco.languages.setMonarchTokensProvider(lispLanguageId, lispGrammar.languageGrammar);
        const editor = monaco.editor.create(scriptEditor, {
            value: '; add your script here\n',
            language: scriptLanguageId,
        });

        scriptTypeSelector.addEventListener('change', ev => {
            const model = editor.getModel();
            if (model) {
                switch (scriptTypeSelector.value) {
                    case '.scr':
                        monaco.editor.setModelLanguage(model, scriptLanguageId);
                        break;
                    case '.lisp':
                        monaco.editor.setModelLanguage(model, lispLanguageId);
                        break;
                }
            }

            editor.layout();
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
            client.executeScript(scriptTypeSelector.value, scriptContent);
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
}
