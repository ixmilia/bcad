import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';
import { DwgFileSettings, DwgFileVersion, DxfFileSettings, DxfFileVersion } from '../contracts.generated';
import { LogWriter } from '../logWriter';

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
        LogWriter.write(`showing file settings with: ${JSON.stringify(dialogOptions)}`);
        for (let i = this.selector.options.length; i >= 0; i--) {
            this.selector.options.remove(i);
        }

        const settings = <FileSettingsOptions>dialogOptions;
        switch (settings.Extension) {
            case ".dwg":
                const dwgSettings = <DwgFileSettings>settings.Settings;
                this.buildVersionSelector(Object.values(DwgFileVersion), dwgSettings.FileVersion);
                break;
            case ".dxf":
                const dxfSettings = <DxfFileSettings>settings.Settings;
                this.buildVersionSelector(Object.values(DxfFileVersion), dxfSettings.FileVersion);
                break;
        }
    }

    dialogTitle(dialogOptions: object): string {
        return 'Drawing Settings';
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

    private buildVersionSelector(avaialbleValues: string[], selectedValue: string) {
        for (const versionString of avaialbleValues) {
            let option = document.createElement('option');
            option.setAttribute('value', versionString);
            option.innerText = versionString;
            if (versionString === selectedValue) {
                option.setAttribute('selected', '1');
            }

            this.selector.appendChild(option);
        }
    }
}
