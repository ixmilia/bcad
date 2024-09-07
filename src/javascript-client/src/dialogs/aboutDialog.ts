import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';
import { Client } from '../client';

export class AboutDialog extends DialogBase {
    private content: HTMLDivElement;

    constructor(dialogHandler: DialogHandler, readonly client: Client) {
        super(dialogHandler, "about");
        this.content = <HTMLDivElement>document.getElementById('about-content');
        this.client.getVersionInformation().then(versionInformation => {
            // preload this once
            this.content.innerHTML = versionInformation.AboutString;
        })
    }

    async dialogShowing(_dialogOptions: object) {
        // noop
    }

    dialogTitle(_dialogOptions: object): string {
        return 'About';
    }

    dialogOk(): object {
        return {};
    }

    dialogCancel() {
        // noop
    }
}
