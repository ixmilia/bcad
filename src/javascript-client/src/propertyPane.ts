import { Client } from './client';

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
                        if (value.AllowedValues !== undefined && value.AllowedValues.length > 0) {
                            // dropdown
                            const select = <HTMLSelectElement>document.createElement('select');
                            select.style.width = '100%';
                            for (const allowedValue of value.AllowedValues) {
                                const option = <HTMLOptionElement>document.createElement('option');
                                option.innerText = allowedValue;
                                if (value.Value === allowedValue) {
                                    option.setAttribute('selected', 'selected');
                                }

                                select.appendChild(option);
                            }

                            select.addEventListener('change', () => {
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
                            text.value = value.Value;
                            text.addEventListener('change', () => {
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
