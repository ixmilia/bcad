import { Client } from '../client';
import { ClientPlotSettings, ClientRectangle } from '../contracts.generated';
import { DialogBase } from './dialogBase';
import { DialogHandler } from './dialogHandler';

export class PlotDialog extends DialogBase {
    private displayContainerDiv: HTMLDivElement;
    private displayDiv: HTMLDivElement;
    private outputPane: HTMLDivElement;
    private scaleAbsolute: HTMLInputElement;
    private scaleFit: HTMLInputElement;
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

    constructor(readonly client: Client, dialogHandler: DialogHandler) {
        super(dialogHandler, "plot");
        this.displayContainerDiv = <HTMLDivElement>document.getElementById('dialog-plot-background');
        this.displayDiv = <HTMLDivElement>document.getElementById('dialog-plot-display');
        this.outputPane = <HTMLDivElement>document.getElementById('output-pane');

        this.scaleAbsolute = <HTMLInputElement>document.getElementById('dialog-plot-scaling-type-absolute');
        this.scaleFit = <HTMLInputElement>document.getElementById('dialog-plot-scaling-type-fit');
        this.scaleA = <HTMLInputElement>document.getElementById('dialog-plot-scale-a');
        this.scaleB = <HTMLInputElement>document.getElementById('dialog-plot-scale-b');
        this.viewportExtents = <HTMLInputElement>document.getElementById('dialog-plot-viewport-type-extents');
        this.viewportWindow = <HTMLInputElement>document.getElementById('dialog-plot-viewport-type-window');
        this.width = <HTMLInputElement>document.getElementById('dialog-plot-size-width');
        this.height = <HTMLInputElement>document.getElementById('dialog-plot-size-height');
        this.plotType = <HTMLSelectElement>document.getElementById('dialog-plot-type');
        this.plotSizePdf = <HTMLDivElement>document.getElementById('dialog-plot-size-pdf');
        this.plotSizeSvg = <HTMLDivElement>document.getElementById('dialog-plot-size-svg');
        this.pdfOrientation = <HTMLSelectElement>document.getElementById('dialog-plot-size-pdf-orientation');

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
            'dialog-plot-viewport-type-window',
            'dialog-plot-size-width',
            'dialog-plot-size-height',
            'dialog-plot-type',
            'dialog-plot-size-pdf-orientation',
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

    dialogOk(): object {
        const settings = this.generatePlotSettings();
        return settings;
    }

    dialogCancel() {
        // noop
    }

    private generatePlotSettings(): ClientPlotSettings {
        // TODO: let user select viewport
        const viewport: ClientRectangle = {
            TopLeft: {
                X: 0,
                Y: 0,
                Z: 0,
            },
            BottomRight: {
                X: this.outputPane.clientWidth,
                Y: this.outputPane.clientHeight,
                Z: 0,
            }
        };

        let width = this.width.valueAsNumber;
        let height = this.height.valueAsNumber;

        if (this.plotType.value === 'pdf') {
            switch (this.pdfOrientation.value) {
                case 'letter':
                    width = 8.5;
                    height = 11.0;
                    break;
                case 'landscape':
                    width = 11.0;
                    height = 8.5;
                    break;
            }
        }

        const settings: ClientPlotSettings = {
            PlotType: this.plotType.value,
            Viewport: viewport,
            ScaleA: this.scaleA.value,
            ScaleB: this.scaleB.value,
            ScalingType: this.scaleFit.checked ? 1 : 0,
            ViewPortType: this.viewportWindow ? 1 : 0,
            Width: width,
            Height: height,
            PreviewMaxSize: this.displayContainerDiv.clientHeight,
        };
        return settings;
    }

    private async updatePreview(): Promise<void> {
        const settings = this.generatePlotSettings();
        const previewContent = await this.client.getPlotPreview(settings);
        this.displayDiv.innerHTML = previewContent;
    }
}
