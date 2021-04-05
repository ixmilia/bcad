import * as fs from 'fs';
import { BrowserWindow, dialog, globalShortcut, Menu, MenuItemConstructorOptions } from "electron";
import { StdioCadServerTransport } from "./StdioCadServerTransport";

export class CadMenu {
    private filePath: string | undefined = undefined;

    constructor(private mainWindow: BrowserWindow, private transport: StdioCadServerTransport) {
        this.buildMenu();
        this.transport.subscribeToClientUpdates(clientUpdate => this.updateTitle(clientUpdate.IsDirty));
        this.updateTitle(false);
    }

    private buildMenu() {
        const template: MenuItemConstructorOptions[] = [
            {
                role: 'fileMenu',
                submenu: [
                    {
                        label: 'Open',
                        accelerator: 'CommandOrControl+O',
                        click: () => this.openClick()
                    },
                    {
                        label: 'Save',
                        accelerator: 'CommandOrControl+S',
                        click: () => this.saveClick()
                    },
                    {
                        label: 'Save As',
                        accelerator: 'CommandOrControl+Shift+S',
                        click: () => this.saveAsClick()
                    },
                    {
                        role: 'quit',
                        accelerator: 'Alt+F4'
                    }
                ]
            },
            {
                role: 'editMenu',
                submenu: [
                    {
                        label: 'Undo',
                        accelerator: 'CommandOrControl+Z',
                        click: () => this.transport.undo()
                    },
                    {
                        label: 'Redo',
                        accelerator: 'CommandOrControl+Y',
                        click: () => this.transport.redo()
                    }
                ]
            }
        ];
        const menu = Menu.buildFromTemplate(template);
        this.mainWindow.setMenu(menu);
    }

    private async openClick() {
        const result = await dialog.showOpenDialog(this.mainWindow, {
            filters: [
                {
                    name: 'CAD Drawings',
                    extensions: ['dxf', 'iges']
                }
            ]
        });
        if (result.filePaths.length > 0) {
            this.filePath = result.filePaths[0];
            fs.readFile(this.filePath, (error, data) => {
                if (!error) {
                    const base64 = data.toString('base64');
                    this.transport.parseFile(this.filePath!, base64);
                }
            });
        }
    }

    private async saveClick() {
        if (!this.filePath) {
            this.filePath = await this.getFileSavePath();
        }

        if (this.filePath) {
            this.save(this.filePath, true);
        }
    }

    private async saveAsClick() {
        const filePath = await this.getFileSavePath();
        if (filePath) {
            this.filePath = filePath
            this.save(this.filePath, false);
        }
    }

    private async getFileSavePath(): Promise<string | undefined> {
        const result = await dialog.showSaveDialog(this.mainWindow, {
            filters: [
                {
                    name: 'CAD Drawings',
                    extensions: ['dxf', 'iges']
                }
            ]
        });
        return result.filePath;
    }

    private async save(filePath: string, preserveSettings: boolean) {
        const contents = await this.transport.getDrawingContents(filePath, preserveSettings);
        if (contents !== null) {
            const buffer = Buffer.from(contents, 'base64');
            fs.writeFile(filePath, buffer, () => { });
        }
    }

    private updateTitle(isDirty: boolean) {
        const filePath = this.filePath || '[untitled]';
        const dirtyMarker = isDirty ? ' *' : '';
        this.mainWindow.setTitle(`BCad: ${filePath}${dirtyMarker}`);
    }
}
