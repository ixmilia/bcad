import { Client } from "./client";
import { CommandShortcut, Key, ModifierKeys } from "./contracts.generated";

export class ShortcutHandler {
    private _commandShortcuts: CommandShortcut[] = [];

    constructor(private readonly client: Client) {
        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate?.Settings?.CommandShortcuts) {
                this._commandShortcuts = clientUpdate.Settings.CommandShortcuts;
            }
        });
    }

    handleShortcut(isShift: boolean, isCtrl: boolean, isAlt: boolean, key: string) {
        for (const shortcut of this._commandShortcuts) {
            const isShortcutShift = (shortcut.ModifierKeys & ModifierKeys.Shift) === ModifierKeys.Shift;
            const isShortcutCtrl = (shortcut.ModifierKeys & ModifierKeys.Control) === ModifierKeys.Control;
            const isShortcutAlt = (shortcut.ModifierKeys & ModifierKeys.Alt) === ModifierKeys.Alt;
            if (isShortcutShift !== isShift) {
                continue;
            }

            if (isShortcutCtrl !== isCtrl) {
                continue;
            }

            if (isShortcutAlt !== isAlt) {
                continue;
            }

            if (shortcut.Key.toString().toLowerCase() !== key.toLowerCase()) {
                continue;
            }

            this.client.executeCommand(shortcut.Name);
            break;
        }
    }
}
