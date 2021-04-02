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
        const elements = [
            'dialog-plot-scaling-type-absolute',
            'dialog-plot-scaling-type-fit',
            'dialog-plot-scale-a',
            'dialog-plot-scale-b',
            'dialog-plot-viewport-type-extents',
            'dialog-plot-viewport-type-window',
            'dialog-plot-size-width',
            'dialog-plot-size-height',
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
        const settings: ClientPlotSettings = {
            Viewport: viewport,
            ScaleA: this.scaleA.value,
            ScaleB: this.scaleB.value,
            ScalingType: this.scaleFit.checked ? 1 : 0,
            ViewPortType: this.viewportWindow ? 1 : 0,
            Width: this.width.valueAsNumber,
            Height: this.height.valueAsNumber,
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
