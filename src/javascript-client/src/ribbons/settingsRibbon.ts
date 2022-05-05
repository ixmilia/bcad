import { UnitFormat } from "../contracts.generated";
import { Client } from "../client";

export class SettingsRibbon {
    private unitElement: HTMLSelectElement;
    private precisionElement: HTMLSelectElement;
    private anglePrecisionElement: HTMLSelectElement;

    constructor(client: Client) {
        this.unitElement = <HTMLSelectElement>document.getElementById("drawing-units");
        this.precisionElement = <HTMLSelectElement>document.getElementById('drawing-units-precision');
        this.anglePrecisionElement = <HTMLSelectElement>document.getElementById('angle-units-precision');

        this.unitElement.addEventListener("change", (ev) => {
            client.setSetting('DrawingUnits', (<any>ev.target).value.toString());
        });
        this.precisionElement.addEventListener('change', (ev) => {
            client.setSetting('DrawingPrecision', (<any>ev.target).value.toString());
        });
        this.anglePrecisionElement.addEventListener('change', (ev) => {
            client.setSetting('AnglePrecision', (<any>ev.target).value.toString());
        });

        document.querySelectorAll(".settings-button").forEach(node => {
            let button = <HTMLButtonElement>node;
            let name = button.getAttribute("data-setting-name");
            let value = button.getAttribute("data-setting-value");
            if (name && value) {
                button.addEventListener('click', () => {
                    client.setSetting(name!, value!);
                });
            }
        });

        let reportSelectUpdates = true;
        document.querySelectorAll(".settings-select").forEach(node => {
            const select = <HTMLSelectElement>node;
            const name = select.getAttribute("data-setting-name");
            if (name) {
                select.addEventListener('change', () => {
                    if (reportSelectUpdates) {
                        client.setSetting(name!, select.value);
                    }
                });
            }
        });
        client.subscribeToClientUpdates((clientUpdate) => {
            if (clientUpdate.Settings !== undefined) {
                reportSelectUpdates = false;
                document.querySelectorAll(".settings-select").forEach(node => {
                    const select = <HTMLSelectElement>node;
                    const name = select.getAttribute("data-setting-name");
                    const path = select.getAttribute("data-setting-path");
                    if (name && path) {
                        let newValue = <any>clientUpdate.Settings;
                        const parts = path.split('.');
                        for (const part of parts) {
                            newValue = newValue[part];
                        }
                        select.value = newValue;
                    }
                });
                reportSelectUpdates = true;
            }
        });

        this.getSnapAngleSelectors().forEach(input => {
            input.addEventListener('change', () => {
                client.setSetting("Display.SnapAngles", input.value);
            });
        });

        client.subscribeToClientUpdates((clientUpdate) => {
            if (clientUpdate.Settings !== undefined) {
                // drawing units and precision
                let selectedUnits = 0;
                let availablePrecisions: [string, number][] = [];
                switch (clientUpdate.Settings.DrawingUnits) {
                    case UnitFormat.Architectural:
                        selectedUnits = 0;
                        availablePrecisions = [
                            ['1"', 1],
                            ['1/2"', 2],
                            ['1/4"', 4],
                            ['1/8"', 8],
                            ['1/16"', 16],
                            ['1/32"', 32],
                        ];
                        break;
                    case UnitFormat.Metric:
                        selectedUnits = 1;
                        availablePrecisions = [
                            ['0', 0],
                            ['1', 1],
                            ['2', 2],
                            ['3', 3],
                            ['4', 4],
                            ['5', 5],
                            ['6', 6],
                            ['7', 7],
                            ['8', 8],
                            ['9', 9],
                            ['10', 10],
                            ['11', 11],
                            ['12', 12],
                            ['13', 13],
                            ['14', 14],
                            ['15', 15],
                            ['16', 16],
                        ];
                        break;
                }
                const optionElements = availablePrecisions.map(p => {
                    const option = document.createElement('option');
                    option.innerText = p[0];
                    option.setAttribute('value', p[1].toString());
                    if (clientUpdate.Settings.DrawingPrecision === p[1]) {
                        option.setAttribute('selected', 'selected');
                    }
                    return option;
                });
                this.unitElement.item(selectedUnits)!.selected = true;
                this.precisionElement.replaceChildren(...optionElements);

                // angle precision
                const anglePrecisionElements = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9].map(p => {
                    const option = document.createElement('option');
                    option.innerText = p.toString();
                    option.setAttribute('value', p.toString());
                    if (clientUpdate.Settings.AnglePrecision === p) {
                        option.setAttribute('selected', 'selected');
                    }
                    return option;
                });
                this.anglePrecisionElement.replaceChildren(...anglePrecisionElements);

                // snap angles
                let snapAnglesString = clientUpdate.Settings.SnapAngles.join(";");
                for (let sna of this.getSnapAngleSelectors()) {
                    if (sna.value === snapAnglesString) {
                        sna.checked = true;
                        break;
                    }
                }
            }
        });
    }

    private getSnapAngleSelectors(): HTMLInputElement[] {
        let inputs: HTMLInputElement[] = [];
        document.querySelectorAll(".snap-angle-selector").forEach(node => {
            let input = <HTMLInputElement>node;
            inputs.push(input);
        });

        return inputs;
    }
}
