import { LogWriter } from '../logWriter';
import { Client } from '../client';
import { ClientPlotSettings, ClientRectangle, PlotColorType, PlotScalingType, PlotViewPortType } from '../contracts.generated';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

export class PlotDialog extends DialogBase {
    private displayContainerDiv: HTMLDivElement;
    private displayDiv: HTMLDivElement;
    private outputPane: HTMLDivElement;
    private scaleAbsolute: HTMLInputElement;
    private scaleFit: HTMLInputElement;
    private drawingExtents: HTMLInputElement;
    private viewportExtents: HTMLInputElement;
    private viewportWindow: HTMLInputElement;
    private scaleA: HTMLInputElement;
    private scaleB: HTMLInputElement;
    private width: HTMLInputElement;
    private height: HTMLInputElement;
    private plotType: HTMLSelectElement;
    private plotSizeSvg: HTMLDivElement;
    private plotSizePdf: HTMLDivElement;
    private pdfOrientation: HTMLSelectElement;
    private selectViewportButton: HTMLButtonElement;
    private colorTypeExact: HTMLInputElement;
    private colorTypeContrast: HTMLInputElement;
    private colorTypeBlack: HTMLInputElement;

    private selectedViewport: ClientRectangle;

    constructor(readonly client: Client, dialogHandler: DialogHandler) {
        super(dialogHandler, "plot");
        this.displayContainerDiv = <HTMLDivElement>document.getElementById('dialog-plot-background');
        this.displayDiv = <HTMLDivElement>document.getElementById('dialog-plot-display');
        this.outputPane = <HTMLDivElement>document.getElementById('output-pane');

        this.scaleAbsolute = <HTMLInputElement>document.getElementById('dialog-plot-scaling-type-absolute');
        this.scaleFit = <HTMLInputElement>document.getElementById('dialog-plot-scaling-type-fit');
        this.scaleA = <HTMLInputElement>document.getElementById('dialog-plot-scale-a');
        this.scaleB = <HTMLInputElement>document.getElementById('dialog-plot-scale-b');
        this.drawingExtents = <HTMLInputElement>document.getElementById('dialog-plot-viewport-type-extents');
        this.viewportExtents = <HTMLInputElement>document.getElementById('dialog-plot-viewport-type-viewport');
        this.viewportWindow = <HTMLInputElement>document.getElementById('dialog-plot-viewport-type-window');
        this.width = <HTMLInputElement>document.getElementById('dialog-plot-size-width');
        this.height = <HTMLInputElement>document.getElementById('dialog-plot-size-height');
        this.plotType = <HTMLSelectElement>document.getElementById('dialog-plot-type');
        this.plotSizePdf = <HTMLDivElement>document.getElementById('dialog-plot-size-pdf');
        this.plotSizeSvg = <HTMLDivElement>document.getElementById('dialog-plot-size-svg');
        this.pdfOrientation = <HTMLSelectElement>document.getElementById('dialog-plot-size-pdf-orientation');
        this.selectViewportButton = <HTMLButtonElement>document.getElementById('dialog-plot-select-viewport');
        this.colorTypeExact = <HTMLInputElement>document.getElementById('dialog-plot-color-type-exact');
        this.colorTypeContrast = <HTMLInputElement>document.getElementById('dialog-plot-color-type-contrast');
        this.colorTypeBlack = <HTMLInputElement>document.getElementById('dialog-plot-color-type-black');

        // ensure it's set to _something_
        this.selectedViewport = {
            TopLeft: { X: 0, Y: 0, Z: 0 },
            BottomRight: { X: 0, Y: 0, Z: 0 },
        };

        [this.scaleA, this.scaleB].forEach(s => s.addEventListener('change', () => { this.scaleAbsolute.checked = true; }));

        this.selectViewportButton.addEventListener('click', async () => {
            dialogHandler.hideDialogs();
            const selection = await this.client.getSelectionRectangle();
            dialogHandler.showDialogs();
            if (selection) {
                this.selectedViewport = selection;
                this.viewportWindow.checked = true;
            }
        });

        this.plotType.addEventListener('change', () => {
            this.plotSizePdf.style.display = 'none';
            this.plotSizeSvg.style.display = 'none';
            switch (this.plotType.value) {
                case 'pdf':
                    this.plotSizePdf.style.display = 'block';
                    break;
                case 'svg':
                    this.plotSizeSvg.style.display = 'block';
                    break;
            }
        });

        const elements = [
            'dialog-plot-scaling-type-absolute',
            'dialog-plot-scaling-type-fit',
            'dialog-plot-scale-a',
            'dialog-plot-scale-b',
            'dialog-plot-viewport-type-extents',
            'dialog-plot-viewport-type-viewport',
            'dialog-plot-viewport-type-window',
            'dialog-plot-size-width',
            'dialog-plot-size-height',
            'dialog-plot-type',
            'dialog-plot-size-pdf-orientation',
            'dialog-plot-color-type-exact',
            'dialog-plot-color-type-contrast',
            'dialog-plot-color-type-black',
        ];
        for (const element of elements) {
            document.getElementById(element)!.addEventListener('change', () => {
                this.updatePreview();
            });
        }
    }

    dialogShowing(dialogOptions: object) {
        this.updatePreview().then(() => { });
    }

    dialogTitle(dialogOptions: object): string {
        return 'Plot';
    }

    dialogOk(): object {
        const settings = this.generatePlotSettings();
        return settings;
    }

    dialogCancel() {
        // noop
    }

    private generatePlotSettings(): ClientPlotSettings {
        const viewport = ((): ClientRectangle => {
            if (this.viewportWindow.checked) {
                return this.selectedViewport;
            } else if (this.viewportExtents.checked) {
                return {
                    TopLeft: { X: 0, Y: 0, Z: 0, },
                    BottomRight: { X: this.outputPane.clientWidth, Y: this.outputPane.clientHeight, Z: 0, }
                };
            } else {
                // if (this.drawingExtents.checked) // doesn't matter, value not used
                return {
                    TopLeft: { X: 0, Y: 0, Z: 0, },
                    BottomRight: { X: this.outputPane.clientWidth, Y: this.outputPane.clientHeight, Z: 0, }
                };
            }
        })();

        const [width, height] = ((): [number, number] => {
            switch (this.plotType.value) {
                case 'pdf':
                    const pdfPpi = 72.0;
                    const mmPerInch = 25.4
                    const pointsFromInches = (inches: number) => inches * pdfPpi;
                    const pointsFromMm = (mm: number) => pointsFromInches(mm / mmPerInch);

                    const a4Small = 210;
                    const a4Large = 297;
                    const letterSmall = 8.5;
                    const letterLarge = 11.0;

                    switch (this.pdfOrientation.value) {
                        case 'a4-portrait':
                            return [pointsFromMm(a4Small), pointsFromMm(a4Large)];
                        case 'a4-landscape':
                            return [pointsFromMm(a4Large), pointsFromMm(a4Small)];
                        case 'letter-portrait':
                            return [pointsFromInches(letterSmall), pointsFromInches(letterLarge)];
                        case 'letter-landscape':
                            return [pointsFromInches(letterLarge), pointsFromInches(letterSmall)];
                    }
                    break;
                case 'svg':
                    return [this.width.valueAsNumber, this.height.valueAsNumber];
            }

            // make the compiler happy and return something
            return [this.width.valueAsNumber, this.height.valueAsNumber];
        })();

        const settings: ClientPlotSettings = {
            PlotType: this.plotType.value,
            Viewport: viewport,
            ScaleA: this.scaleA.value,
            ScaleB: this.scaleB.value,
            ScalingType: this.scaleFit.checked ? PlotScalingType.ToFit : PlotScalingType.Absolute,
            ViewPortType: !this.drawingExtents.checked ? PlotViewPortType.Window : PlotViewPortType.Extents,
            ColorType: this.colorTypeExact.checked ? PlotColorType.Exact : this.colorTypeContrast.checked ? PlotColorType.Contrast : PlotColorType.Black,
            Width: width,
            Height: height,
            PreviewMaxSize: this.displayContainerDiv.clientHeight,
        };
        return settings;
    }

    private async updatePreview(): Promise<void> {
        const settings = this.generatePlotSettings();
        LogWriter.write(`Plot settings: ${JSON.stringify(settings)}`);
        const previewContent = await this.client.getPlotPreview(settings);
        this.displayDiv.innerHTML = previewContent;
    }
}
