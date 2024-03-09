import { Client } from './client';
import { ColorPicker } from './controls/colorPicker';
import { InputConsole } from './inputConsole';
import { LogWriter } from './logWriter';

export class PropertyPane {
    constructor(client: Client) {
        const propertyPane = <HTMLDivElement>document.getElementById("property-pane");
        InputConsole.ensureCapturedEvents(propertyPane, true);

        const closeButton = document.getElementById('property-close');
        closeButton?.addEventListener('click', () => {
            client.cancel();
        });

        const propertyPaneContents = <HTMLDivElement>document.getElementById("property-pane-contents");
        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate.PropertyPane !== undefined) {
                propertyPaneContents.innerHTML = '';
                propertyPane.style.display = 'none';
                if (clientUpdate.PropertyPane.Values.length > 0) {
                    LogWriter.write(`PROPERTY-PANE: got ${JSON.stringify(clientUpdate.PropertyPane.Values)}`);
                    const table = <HTMLTableElement>document.createElement('table');
                    for (const value of clientUpdate.PropertyPane.Values) {
                        const name = <HTMLTableCellElement>document.createElement('td');
                        name.classList.add('property-pane-setting-name');
                        name.setAttribute('align', 'right');
                        name.innerText = value.DisplayName;

                        const valueCell = <HTMLTableCellElement>document.createElement('td');
                        valueCell.classList.add('property-pane-setting-value');
                        if (value.IsReadOnly) {
                            if (value.Value) {
                                // simple text span for read-only values
                                const span = document.createElement('span');
                                span.innerText = value.Value;
                                valueCell.appendChild(span);
                            }
                        } else if (value.IsAction) {
                            name.innerText = ''; // display nothing; let the button do it
                            const button = <HTMLButtonElement>document.createElement('button');
                            button.textContent = value.DisplayName;
                            button.addEventListener('click', () => {
                                LogWriter.write(`PROPERTY-PANE: executing action ${value.Name}`);
                                client.setPropertyPaneValue({
                                    IsReadOnly: false,
                                    IsAction: true,
                                    Name: value.Name,
                                    DisplayName: value.DisplayName,
                                    Value: undefined,
                                });
                            });
                            valueCell.appendChild(button);
                        } else if (value.Name === 'color' || value.Name.endsWith('-color')) {
                            // special handling for colors
                            valueCell.setAttribute('align', 'left');
                            const colorPickerDiv = document.createElement('div');
                            const _colorPicker = new ColorPicker(colorPickerDiv, {
                                initialColor: value.Value,
                                onColorChanged: (color, colorAsHex) => {
                                    LogWriter.write(`PROPERTY-PANE: setting ${value.Name} to ${JSON.stringify(color)}`);
                                    client.setPropertyPaneValue({
                                        IsReadOnly: false,
                                        IsAction: false,
                                        Name: value.Name,
                                        DisplayName: value.DisplayName,
                                        Value: colorAsHex,
                                    });
                                }
                            });
                            valueCell.appendChild(colorPickerDiv);
                        } else if (value.AllowedValues !== undefined && value.AllowedValues.length > 0) {
                            // dropdown
                            const select = <HTMLSelectElement>document.createElement('select');
                            select.style.width = '100%';

                            if (value.IsUnrepresentable) {
                                const option = <HTMLOptionElement>document.createElement('option');
                                option.innerText = '*VARIES*';
                                option.disabled = true;
                                option.selected = true;
                                select.appendChild(option);
                            }

                            for (const allowedValue of value.AllowedValues) {
                                const option = <HTMLOptionElement>document.createElement('option');
                                option.innerText = allowedValue;
                                if (value.Value === allowedValue && !value.IsUnrepresentable) {
                                    option.selected = true;
                                }

                                select.appendChild(option);
                            }

                            select.addEventListener('change', () => {
                                LogWriter.write(`PROPERTY-PANE: setting ${value.Name} to ${select.value}`);
                                client.setPropertyPaneValue({
                                    IsReadOnly: false,
                                    IsAction: false,
                                    Name: value.Name,
                                    DisplayName: value.DisplayName,
                                    Value: select.value,
                                });
                            });
                            valueCell.appendChild(select);
                        } else {
                            // simple string value
                            const text = <HTMLInputElement>document.createElement('input');
                            text.value = value.Value || '';
                            text.addEventListener('focus', () => {
                                text.select();
                            });
                            text.addEventListener('keydown', (e) => {
                                if (e.key === 'Escape') {
                                    client.cancel();
                                }
                            });
                            text.addEventListener('change', () => {
                                LogWriter.write(`PROPERTY-PANE: setting ${value.Name} to ${text.value}`);
                                client.setPropertyPaneValue({
                                    IsReadOnly: false,
                                    IsAction: false,
                                    Name: value.Name,
                                    DisplayName: value.DisplayName,
                                    Value: text.value,
                                });
                            });
                            valueCell.appendChild(text);
                        }

                        const row = <HTMLTableRowElement>document.createElement('tr');
                        row.appendChild(name);
                        row.appendChild(valueCell);

                        table.appendChild(row);
                    }

                    propertyPaneContents.appendChild(table);
                    propertyPane.style.display = 'block';
                }
            }
        });
    }
}
