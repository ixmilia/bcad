import { Client } from './client';
import { ShortcutHandler } from './shortcutHandler';

const mouseEvents: string[] = [
    'mouseup',
    'mousedown',
    'wheel',
];

const userEvents: string[] = [
    'keyup',
    'keydown',
    ...mouseEvents,
];

export class InputConsole {
    private client: Client;
    private input: HTMLInputElement;

    constructor(client: Client, shortcutHandler: ShortcutHandler) {
        this.client = client;
        this.input = <HTMLInputElement>document.getElementById("input");

        this.client.addFunctionHandler('clearInput', () => {
            this.clearInput();
        });

        InputConsole.ensureCapturedEvents(this.input);

        // all select elements need to handle their own mouse events
        document.querySelectorAll('select').forEach(select => {
            mouseEvents.forEach(mouseEvent => {
                select.addEventListener(mouseEvent, ev => {
                    ev.stopPropagation();
                });
            });
        });

        // otherwise always focus the input
        userEvents.forEach(eventName => {
            document.addEventListener(eventName, () => {
                this.focus();
            });
        });

        this.input.addEventListener('input', _ => {
            this.client.inputChanged(this.input.value);
        });
        this.input.addEventListener('keydown', ev => {
            this.handleKeystroke(ev);
            shortcutHandler.handleShortcut(ev.shiftKey, ev.ctrlKey, ev.altKey, ev.key);
        });

        this.focus();
    }

    focus() {
        this.input.focus();
    }

    static ensureCapturedEvents(element: HTMLElement, captureMouseMove?: boolean) {
        const mouseMoveEvents: string[] = captureMouseMove ? ['mousemove'] : [];
        const allEvents = [...userEvents, ...mouseMoveEvents];
        for (const eventName of allEvents) {
            element.addEventListener(eventName, ev => {
                ev.stopPropagation();
            });
        }
    }

    private handleKeystroke(ev: KeyboardEvent) {
        switch (ev.key) {
            case "Enter":
                this.submit();
                break;
            case "Escape":
                this.clearInput();
                this.client.cancel();
                break;
        }
    }

    private clearInput() {
        this.input.value = "";
    }

    private submit() {
        let value = this.input.value;
        this.clearInput();
        this.client.submitInput(value);
    }
}
