import { Client } from './client';
import { LogWriter } from './logWriter';

export class PropertyPane {
    constructor(client: Client) {
        const propertyPane = <HTMLDivElement>document.getElementById("property-pane");
        propertyPane.addEventListener('mousemove', ev => ev.stopPropagation());
        propertyPane.addEventListener('mousedown', ev => ev.stopPropagation());
        propertyPane.addEventListener('mouseup', ev => ev.stopPropagation());
        propertyPane.addEventListener('wheel', ev => ev.stopPropagation());

        const propertyPaneContents = <HTMLDivElement>document.getElementById("property-pane-contents");
        client.subscribeToClientUpdates(clientUpdate => {
            if (clientUpdate.PropertyPane !== undefined) {
                propertyPaneContents.innerHTML = '';
                propertyPane.style.display = 'none';
                if (clientUpdate.PropertyPane.Values.length > 0) {
                    const table = <HTMLTableElement>document.createElement('table');
                    for (const value of clientUpdate.PropertyPane.Values) {
                        const name = <HTMLTableCellElement>document.createElement('td');
                        name.classList.add('property-pane-setting-name');
                        name.setAttribute('align', 'right');
                        name.innerText = value.DisplayName;

                        const valueCell = <HTMLTableCellElement>document.createElement('td');
                        valueCell.classList.add('property-pane-setting-value');
                        if (value.Name === 'color') {
                            // special handling for colors
                            const color = <HTMLInputElement>document.createElement('input');
                            color.disabled = !value.Value;
                            color.type = 'color';
                            color.value = value.Value || '#000000';

                            const isAuto = <HTMLInputElement>document.createElement('input');
                            isAuto.id = 'property-pane-is-color-auto';
                            isAuto.type = 'checkbox';
                            isAuto.checked = !value.Value;
                            isAuto.addEventListener('change', () => {
                                color.disabled = isAuto.checked;
                            });

                            const label = <HTMLLabelElement>document.createElement('label');
                            label.setAttribute('for', isAuto.id);
                            label.innerText = 'Auto?';

                            function reportColorChange() {
                                const newValue = isAuto.checked ? undefined : color.value;
                                LogWriter.write(`PROPERTY-PANE: setting ${value.Name} to ${newValue}`);
                                client.setPropertyPaneValue({
                                    Name: value.Name,
                                    DisplayName: value.DisplayName,
                                    Value: newValue,
                                });
                            }

                            isAuto.addEventListener('change', () => reportColorChange());
                            color.addEventListener('change', () => reportColorChange());

                            // special case for variable value
                            if (value.IsUnrepresentable) {
                                const warningSpan = <HTMLSpanElement>document.createElement('span');
                                warningSpan.innerText = '*VARIES*';
                                valueCell.appendChild(warningSpan);
                            }

                            valueCell.appendChild(isAuto);
                            valueCell.appendChild(label);
                            valueCell.appendChild(color);
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
                            text.addEventListener('change', () => {
                                LogWriter.write(`PROPERTY-PANE: setting ${value.Name} to ${text.value}`);
                                client.setPropertyPaneValue({
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