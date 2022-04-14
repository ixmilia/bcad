import { CadColor } from '../contracts.generated';

export interface ColorPickerOptions {
    initialColor: CadColor | string | undefined;
    allowNullColor?: boolean | undefined;
    colorsPerRow?: number | undefined;
    onColorChanged?: (color: CadColor | undefined, colorAsHex: string | undefined) => void;
}

// from https://github.com/ixmilia/dxf/blob/1e2f82e50d08eccf46b1fed2da545eb27fb4f2da/src/IxMilia.Dxf/DxfColor.cs#L162-L196
const colors: number[] = [
    0xFF000000, 0xFFFF0000, 0xFFFFFF00, 0xFF00FF00, 0xFF00FFFF, 0xFF0000FF, 0xFFFF00FF, 0xFFFFFFFF,
    0xFF414141, 0xFF808080, 0xFFFF0000, 0xFFFFAAAA, 0xFFBD0000, 0xFFBD7E7E, 0xFF810000, 0xFF815656,
    0xFF680000, 0xFF684545, 0xFF4F0000, 0xFF4F3535, 0xFFFF3F00, 0xFFFFBFAA, 0xFFBD2E00, 0xFFBD8D7E,
    0xFF811F00, 0xFF816056, 0xFF681900, 0xFF684E45, 0xFF4F1300, 0xFF4F3B35, 0xFFFF7F00, 0xFFFFD4AA,
    0xFFBD5E00, 0xFFBD9D7E, 0xFF814000, 0xFF816B56, 0xFF683400, 0xFF685645, 0xFF4F2700, 0xFF4F4235,
    0xFFFFBF00, 0xFFFFEAAA, 0xFFBD8D00, 0xFFBDAD7E, 0xFF816000, 0xFF817656, 0xFF684E00, 0xFF685F45,
    0xFF4F3B00, 0xFF4F4935, 0xFFFFFF00, 0xFFFFFFAA, 0xFFBDBD00, 0xFFBDBD7E, 0xFF818100, 0xFF818156,
    0xFF686800, 0xFF686845, 0xFF4F4F00, 0xFF4F4F35, 0xFFBFFF00, 0xFFEAFFAA, 0xFF8DBD00, 0xFFADBD7E,
    0xFF608100, 0xFF768156, 0xFF4E6800, 0xFF5F6845, 0xFF3B4F00, 0xFF494F35, 0xFF7FFF00, 0xFFD4FFAA,
    0xFF5EBD00, 0xFF9DBD7E, 0xFF408100, 0xFF6B8156, 0xFF346800, 0xFF566845, 0xFF274F00, 0xFF424F35,
    0xFF3FFF00, 0xFFBFFFAA, 0xFF2EBD00, 0xFF8DBD7E, 0xFF1F8100, 0xFF608156, 0xFF196800, 0xFF4E6845,
    0xFF134F00, 0xFF3B4F35, 0xFF00FF00, 0xFFAAFFAA, 0xFF00BD00, 0xFF7EBD7E, 0xFF008100, 0xFF568156,
    0xFF006800, 0xFF456845, 0xFF004F00, 0xFF354F35, 0xFF00FF3F, 0xFFAAFFBF, 0xFF00BD2E, 0xFF7EBD8D,
    0xFF00811F, 0xFF568160, 0xFF006819, 0xFF45684E, 0xFF004F13, 0xFF354F3B, 0xFF00FF7F, 0xFFAAFFD4,
    0xFF00BD5E, 0xFF7EBD9D, 0xFF008140, 0xFF56816B, 0xFF006834, 0xFF456856, 0xFF004F27, 0xFF354F42,
    0xFF00FFBF, 0xFFAAFFEA, 0xFF00BD8D, 0xFF7EBDAD, 0xFF008160, 0xFF568176, 0xFF00684E, 0xFF45685F,
    0xFF004F3B, 0xFF354F49, 0xFF00FFFF, 0xFFAAFFFF, 0xFF00BDBD, 0xFF7EBDBD, 0xFF008181, 0xFF568181,
    0xFF006868, 0xFF456868, 0xFF004F4F, 0xFF354F4F, 0xFF00BFFF, 0xFFAAEAFF, 0xFF008DBD, 0xFF7EADBD,
    0xFF006081, 0xFF567681, 0xFF004E68, 0xFF455F68, 0xFF003B4F, 0xFF35494F, 0xFF007FFF, 0xFFAAD4FF,
    0xFF005EBD, 0xFF7E9DBD, 0xFF004081, 0xFF566B81, 0xFF003468, 0xFF455668, 0xFF00274F, 0xFF35424F,
    0xFF003FFF, 0xFFAABFFF, 0xFF002EBD, 0xFF7E8DBD, 0xFF001F81, 0xFF566081, 0xFF001968, 0xFF454E68,
    0xFF00134F, 0xFF353B4F, 0xFF0000FF, 0xFFAAAAFF, 0xFF0000BD, 0xFF7E7EBD, 0xFF000081, 0xFF565681,
    0xFF000068, 0xFF454568, 0xFF00004F, 0xFF35354F, 0xFF3F00FF, 0xFFBFAAFF, 0xFF2E00BD, 0xFF8D7EBD,
    0xFF1F0081, 0xFF605681, 0xFF190068, 0xFF4E4568, 0xFF13004F, 0xFF3B354F, 0xFF7F00FF, 0xFFD4AAFF,
    0xFF5E00BD, 0xFF9D7EBD, 0xFF400081, 0xFF6B5681, 0xFF340068, 0xFF564568, 0xFF27004F, 0xFF42354F,
    0xFFBF00FF, 0xFFEAAAFF, 0xFF8D00BD, 0xFFAD7EBD, 0xFF600081, 0xFF765681, 0xFF4E0068, 0xFF5F4568,
    0xFF3B004F, 0xFF49354F, 0xFFFF00FF, 0xFFFFAAFF, 0xFFBD00BD, 0xFFBD7EBD, 0xFF810081, 0xFF815681,
    0xFF680068, 0xFF684568, 0xFF4F004F, 0xFF4F354F, 0xFFFF00BF, 0xFFFFAAEA, 0xFFBD008D, 0xFFBD7EAD,
    0xFF810060, 0xFF815676, 0xFF68004E, 0xFF68455F, 0xFF4F003B, 0xFF4F3549, 0xFFFF007F, 0xFFFFAAD4,
    0xFFBD005E, 0xFFBD7E9D, 0xFF810040, 0xFF81566B, 0xFF680034, 0xFF684556, 0xFF4F0027, 0xFF4F3542,
    0xFFFF003F, 0xFFFFAABF, 0xFFBD002E, 0xFFBD7E8D, 0xFF81001F, 0xFF815660, 0xFF680019, 0xFF68454E,
    0xFF4F0013, 0xFF4F353B, 0xFF333333, 0xFF505050, 0xFF696969, 0xFF828282, 0xFFBEBEBE, 0xFFFFFFFF,
];

export class ColorPicker {
    private colorPreview: HTMLDivElement;
    private colorName: HTMLDivElement;
    private pickerWindow: HTMLDivElement;
    private pickerBody: HTMLDivElement;

    private _color: CadColor | undefined;

    private defaultRowSize = 4;

    constructor(private readonly element: HTMLElement, private readonly options: ColorPickerOptions) {
        this._color = undefined;
        if (this.options.initialColor) {
            if (typeof this.options.initialColor === 'string') {
                this._color = parseColor(this.options.initialColor);
            } else {
                this._color = this.options.initialColor;
            }
        }

        // populate preview
        this.colorPreview = document.createElement('div');
        this.colorPreview.classList.add('color-preview');
        this.colorPreview.style.backgroundColor = colorToHex(this.color);
        this.colorName = document.createElement('div');
        this.colorName.classList.add('color-name');
        this.colorName.textContent = colorToDisplayText(this.color);
        this.element.innerHTML = '';
        this.element.appendChild(this.colorPreview);
        this.element.appendChild(this.colorName);

        // create and populate picker window
        this.pickerWindow = document.createElement('div');
        this.pickerBody = document.createElement('div');

        // place the picker directly above the element
        this.pickerWindow.style.position = 'absolute';
        this.pickerWindow.style.left = '0px';
        this.pickerWindow.style.top = '0px';
        this.pickerWindow.classList.add('window');
        this.pickerWindow.style.display = 'none'; // initially hidden

        const titleBarText = document.createElement('div');
        titleBarText.classList.add('title-bar-text');
        titleBarText.innerText = 'Color';
        const titleBarControls = document.createElement('div');
        titleBarControls.classList.add('title-bar-controls');
        const closeButton = document.createElement('button');
        closeButton.ariaLabel = 'Close';
        closeButton.addEventListener('click', (e) => {
            e.stopPropagation();
            this.closePopup();
        });
        titleBarControls.appendChild(closeButton);
        const titleBar = document.createElement('div');
        titleBar.classList.add('title-bar');
        titleBar.appendChild(titleBarText);
        titleBar.appendChild(titleBarControls);
        this.pickerWindow.appendChild(titleBar);

        this.pickerBody.style.overflowY = 'auto';
        this.pickerBody.style.maxHeight = '400px';
        this.pickerBody.classList.add('window-body');

        this.pickerWindow.appendChild(this.pickerBody);
        this.element.appendChild(this.pickerWindow);
        this.render();

        this.element.addEventListener('click', () => {
            this.showPopup();
        });
    }

    get color(): CadColor | undefined {
        return this._color;
    }

    set color(value: CadColor | undefined) {
        if ((this.options.allowNullColor !== undefined && this.options.allowNullColor === false) &&
            value === undefined) {
            return;
        }

        this._color = value;
        this.colorPreview.style.backgroundColor = colorToHex(value);
        this.colorName.textContent = colorToDisplayText(value);
        this.reportColorChange();
    }

    get colorAsHex(): string | undefined {
        if (this.color) {
            return colorToHex(this.color);
        }

        return undefined;
    }

    private reportColorChange() {
        if (this.options.onColorChanged) {
            this.options.onColorChanged(this.color, this.colorAsHex);
        }
    }

    private showPopup() {
        this.pickerWindow.style.display = 'block';

        // now make sure it fits
        const rect = this.element.getBoundingClientRect();
        const fullControlWidth = this.pickerWindow.offsetWidth + 20;
        const rightEdge = rect.left + fullControlWidth;
        if (rightEdge > window.innerWidth) {
            this.pickerWindow.style.left = `-${rightEdge - window.innerWidth}px`;
        }
    }

    private closePopup() {
        this.pickerWindow.style.display = 'none';
    }

    private render() {
        const rowSize = this.options.colorsPerRow || this.defaultRowSize;
        this.pickerBody.innerHTML = '';
        const table = document.createElement('table');
        let row = document.createElement('tr');

        function addTableCell(content: HTMLElement) {
            const td = document.createElement('td');
            td.appendChild(content);
            row.appendChild(td);
        }

        function newRow() {
            table.appendChild(row);
            row = document.createElement('tr');
        }

        // create first row with auto button
        if (this.options.allowNullColor === undefined || this.options.allowNullColor) {
            addTableCell(this.createColorBlock(undefined));
            newRow();
        }

        // put remaining colors in rows
        for (let i = 0; i < colors.length; i++) {
            if (i % rowSize === 0) {
                newRow();
            }

            const color = colors[i];
            const cadColor = {
                A: 255,
                R: color >> 16 & 0xFF,
                G: color >> 8 & 0xFF,
                B: color & 0xFF,
            };

            const colorButton = this.createColorBlock(cadColor)
            addTableCell(colorButton);
        }

        table.appendChild(row);
        this.pickerBody.appendChild(table);
    }

    private closeAndReportColor(color: CadColor | undefined) {
        this.color = color;
        this.closePopup();
    }

    private createColorBlock(specifiedColor: CadColor | undefined): HTMLElement {
        const colorButton = document.createElement('div');
        const colorButtonPreview = document.createElement('div');
        colorButtonPreview.classList.add('color-preview');
        colorButtonPreview.style.backgroundColor = colorToHex(specifiedColor);
        const colorButtonText = document.createElement('div');
        colorButtonText.classList.add('color-name');
        colorButtonText.textContent = colorToDisplayText(specifiedColor);
        colorButton.appendChild(colorButtonPreview);
        colorButton.appendChild(colorButtonText);
        colorButton.addEventListener('click', (e) => {
            e.stopPropagation();
            this.closeAndReportColor(specifiedColor);
        });
        return colorButton;
    }
}

function numberToHex(n: number): string {
    let result = n.toString(16);
    while (result.length < 2) {
        result = '0' + result;
    }

    return result.toUpperCase();
}

function colorToDisplayText(c: CadColor | undefined): string {
    if (!c) {
        return '(Auto)';
    } else {
        return colorToHex(c);
    }
}

function colorToHex(color: CadColor | undefined): string {
    const c = color || { A: 255, R: 0, G: 0, B: 0 };
    return `#${numberToHex(c.R)}${numberToHex(c.G)}${numberToHex(c.B)}`;
}

function parseColor(colorString: string | undefined): CadColor | undefined {
    if (colorString) {
        if (colorString.startsWith('#')) {
            colorString = colorString.substring(1);
        }
        if (colorString.length === 8) {
            colorString = colorString.substring(2);
        }
        if (colorString.length === 6) {
            const r = parseInt(colorString.substring(0, 2), 16);
            const g = parseInt(colorString.substring(2, 4), 16);
            const b = parseInt(colorString.substring(4, 6), 16);
            return {
                A: 255,
                R: r,
                G: g,
                B: b,
            };
        }
    }

    return undefined;
}
