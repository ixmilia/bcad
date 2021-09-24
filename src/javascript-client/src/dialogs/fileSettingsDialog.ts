import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';
import { DxfFileSettings, DxfFileVersion } from '../contracts.generated';

interface FileSettingsOptions {
    Extension: string;
    Settings: any;
}

export class FileSettingsDialog extends DialogBase {
    private selector: HTMLSelectElement;

    constructor(dialogHandler: DialogHandler) {
        super(dialogHandler, "FileSettings");
        this.selector = <HTMLSelectElement>document.getElementById('dialog-FileSettings-version');
    }

    dialogShowing(dialogOptions: object) {
        for (let i = this.selector.options.length; i >= 0; i--) {
            this.selector.options.remove(i);
        }

        const settings = <FileSettingsOptions>dialogOptions;
        switch (settings.Extension) {
            case ".dxf":
                const dxfSettings = <DxfFileSettings>settings.Settings;
                this.buildVersionSelectorForDxf(dxfSettings.FileVersion);
                break;
        }
    }

    dialogOk(): object {
        const result: DxfFileSettings = {
            FileVersion: DxfFileVersion[this.selector.value as keyof typeof DxfFileVersion]
        };

        return result;
    }

    dialogCancel() {
        // noop
    }

    private buildVersionSelectorForDxf(selected: DxfFileVersion) {
        for (const versionString in DxfFileVersion) {
            if (typeof DxfFileVersion[versionString] === 'string') {
                continue;
            }
            const version = DxfFileVersion[versionString as keyof typeof DxfFileVersion];
            let option = document.createElement('option');
            option.setAttribute('value', versionString);
            option.innerText = versionString;
            if (version === selected) {
                option.setAttribute('selected', '1');
            }

            this.selector.appendChild(option);
        }
    }
}
