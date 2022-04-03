import { Client } from './client';
import { ResizeObserver } from 'resize-observer';
import {
    CadColor,
    ClientDrawing,
    ClientEllipse,
    ClientImage,
    ClientLine,
    ClientPoint,
    ClientPointLocation,
    ClientSettings,
    ClientText,
    ClientTransform,
    ClientUpdate,
    CursorState,
    MouseButton,
    Point,
    SelectionMode,
    SelectionState,
    SnapPointKind,
    UnitFormat,
} from './contracts.generated';

interface Drawing extends ClientDrawing {
    LineCount: number;
    LineVertices: WebGLBuffer;
    LineColors: WebGLBuffer;

    LineCountWithDefaultColor: number;
    LineVerticesWithDefaultColor: WebGLBuffer;

    PointCount: number;
    PointLocations: WebGLBuffer;
    PointColors: WebGLBuffer;

    PointCountWithDefaultColor: number;
    PointLocationsWithDefaultColor: WebGLBuffer;

    ImageElements: [ClientImage, HTMLImageElement][];
}

export class ViewControl {
    // DOM
    private cursorCanvas: HTMLCanvasElement;
    private imageCanvas: HTMLCanvasElement;
    private rubberBandImageCanvas: HTMLCanvasElement;
    private drawingCanvas: HTMLCanvasElement;
    private selectionCanvas: HTMLCanvasElement;
    private textCanvas: HTMLCanvasElement;
    private rubberBandTextCanvas: HTMLCanvasElement;
    private outputPane: HTMLDivElement;

    // webgl
    private gl: WebGLRenderingContext;
    private glAngle: ANGLE_instanced_arrays;
    private imageTwoD: CanvasRenderingContext2D;
    private rubberBandImageTwoD: CanvasRenderingContext2D;
    private twod: CanvasRenderingContext2D;
    private selectionTwod: CanvasRenderingContext2D;
    private textCtx: CanvasRenderingContext2D;
    private rubberTextCtx: CanvasRenderingContext2D;
    private viewTransformLocation: WebGLUniformLocation = {};
    private objectWorldTransformLocation: WebGLUniformLocation = {};
    private objectScaleTransformLocation: WebGLUniformLocation = {};
    private pointMarkBuffer: WebGLBuffer = {};
    private ellipseBuffer: WebGLBuffer = {};
    private hotPointMarkBuffer: WebGLBuffer = {};
    private hotPointLocations: WebGLBuffer = {};
    private hotPointCount: number = 0;
    private coordinatesLocation: number = 0;
    private translationLocation: number = 0;
    private colorLocation: number = 0;
    private identity: number[];
    private transform: ClientTransform;

    // CAD
    private client: Client;
    private entityDrawing: Drawing;
    private selectedEntitiesDrawing: ClientDrawing;
    private rubberBandDrawing: Drawing;
    private cursorPosition: { x: number, y: number };
    private cursorState: CursorState;
    private selectionState?: SelectionState;
    private snapPointKind: SnapPointKind;
    private settings: ClientSettings;

    constructor(client: Client) {
        this.client = client;

        // DOM
        this.cursorCanvas = <HTMLCanvasElement>document.getElementById('cursorCanvas');
        this.imageCanvas = <HTMLCanvasElement>document.getElementById('imageCanvas');
        this.rubberBandImageCanvas = <HTMLCanvasElement>document.getElementById('rubberBandImageCanvas');
        this.drawingCanvas = <HTMLCanvasElement>document.getElementById('drawingCanvas');
        this.selectionCanvas = <HTMLCanvasElement>document.getElementById('selectionCanvas');
        this.textCanvas = <HTMLCanvasElement>document.getElementById('textCanvas');
        this.rubberBandTextCanvas = <HTMLCanvasElement>document.getElementById('rubberBandTextCanvas');
        this.outputPane = <HTMLDivElement>document.getElementById('output-pane');

        // CAD
        this.cursorPosition = { x: 0, y: 0 };
        this.cursorState = CursorState.Object | CursorState.Point;
        this.selectionState = undefined;
        this.snapPointKind = SnapPointKind.None;
        this.hotPointCount = 0;
        this.entityDrawing = {
            CurrentLayer: "0",
            Layers: ["0"],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
            LineCount: 0,
            LineVertices: {},
            LineColors: {},
            LineCountWithDefaultColor: 0,
            LineVerticesWithDefaultColor: {},
            PointCount: 0,
            PointLocations: {},
            PointColors: {},
            PointCountWithDefaultColor: 0,
            PointLocationsWithDefaultColor: {},
            ImageElements: [],
        };
        this.selectedEntitiesDrawing = {
            CurrentLayer: "0",
            Layers: ["0"],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
        };
        this.rubberBandDrawing = {
            CurrentLayer: "",
            Layers: [],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
            LineCount: 0,
            LineVertices: {},
            LineColors: {},
            LineCountWithDefaultColor: 0,
            LineVerticesWithDefaultColor: {},
            PointCount: 0,
            PointLocations: {},
            PointColors: {},
            PointCountWithDefaultColor: 0,
            PointLocationsWithDefaultColor: {},
            ImageElements: [],
        };
        this.identity = [
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        ];
        this.transform = {
            Transform: this.identity,
            CanvasTransform: this.identity,
            DisplayXTransform: 1.0,
            DisplayYTransform: 1.0,
        };
        this.settings = {
            AutoColor: { A: 255, R: 255, G: 255, B: 255 },
            BackgroundColor: { A: 255, R: 255, G: 255, B: 255 },
            CursorSize: 60,
            Debug: false,
            DrawingPrecision: 16,
            DrawingUnits: UnitFormat.Architectural,
            EntitySelectionRadius: 3,
            HotPointColor: { A: 255, R: 0, G: 0, B: 255 },
            HotPointSize: 10,
            SnapAngles: [0, 90, 180, 270],
            SnapPointColor: { A: 255, R: 255, G: 255, B: 0 },
            SnapPointSize: 15,
            PointDisplaySize: 48,
            TextCursorSize: 18,
            Theme: 'xp/98.css',
            CommandShortcuts: [],
        };

        // render
        this.gl = this.drawingCanvas.getContext('webgl') || throwError('Unable to get webgl context');
        this.glAngle = this.gl.getExtension('ANGLE_instanced_arrays') || throwError('Unable to get ANGLE_instanced_arrays extension');
        this.imageTwoD = this.imageCanvas.getContext('2d') || throwError('Unable to get image canvas 2d context');
        this.rubberBandImageTwoD = this.rubberBandImageCanvas.getContext('2d') || throwError('Unable to get rubber band image canvas 2d context');
        this.twod = this.cursorCanvas.getContext("2d") || throwError('Unable to get cursor canvas 2d context');
        this.selectionTwod = this.selectionCanvas.getContext("2d") || throwError('Unable to get selection canvas 2d context');
        this.textCtx = this.textCanvas.getContext("2d") || throwError('Unable to get text canvas 2d context');
        this.rubberTextCtx = this.rubberBandTextCanvas.getContext("2d") || throwError('Unable to get rubber text context');
        this.prepareCanvas();
        this.populateStaticVertices();
        this.populateVertices(this.entityDrawing);
        this.populateVertices(this.rubberBandDrawing);
        this.populateHotPoints([]);
        this.prepareEvents();
        this.redraw();

        this.client.subscribeToClientUpdates(clientUpdate => this.update(clientUpdate));
        this.client.ready(this.outputPane.clientWidth, this.outputPane.clientHeight);
    }

    private async update(clientUpdate: ClientUpdate): Promise<void> {
        let redraw = false;
        let redrawCursor = false;
        let redrawSelected = false;
        if (clientUpdate.Drawing !== undefined) {
            this.entityDrawing.FileName = clientUpdate.Drawing.FileName;
            await this.updateDrawing(this.entityDrawing, clientUpdate.Drawing);
            redraw = true;
        }
        if (clientUpdate.SelectedEntitiesDrawing !== undefined) {
            this.selectedEntitiesDrawing = clientUpdate.SelectedEntitiesDrawing;
            redrawSelected = true;
        }
        if (clientUpdate.RubberBandDrawing !== undefined) {
            await this.updateDrawing(this.rubberBandDrawing, clientUpdate.RubberBandDrawing);
            redraw = true;
        }
        if (clientUpdate.TransformedSnapPoint !== undefined) {
            this.cursorPosition = { x: clientUpdate.TransformedSnapPoint.ControlPoint.X, y: clientUpdate.TransformedSnapPoint.ControlPoint.Y };
            this.snapPointKind = clientUpdate.TransformedSnapPoint.Kind;
            redrawCursor = true;
        }
        if (clientUpdate.Transform !== undefined) {
            this.transform = clientUpdate.Transform;
            redraw = true;
            redrawSelected = true;
        }
        if (clientUpdate.CursorState !== undefined) {
            this.cursorState = clientUpdate.CursorState;
            redrawCursor = true;
        }
        if (clientUpdate.HotPoints !== undefined) {
            this.populateHotPoints(clientUpdate.HotPoints);
            redraw = true;
        }
        if (clientUpdate.HasSelectionStateUpdate) {
            this.selectionState = clientUpdate.SelectionState;
            redrawCursor = true;
        }
        if (clientUpdate.Settings !== undefined) {
            this.settings = clientUpdate.Settings;
            this.outputPane.style.background = ViewControl.colorToHex(this.settings.BackgroundColor);
            redraw = true;
            redrawCursor = true;
            redrawSelected = true;
        }
        if (clientUpdate.Prompt !== undefined) {
            document.getElementById("prompt")!.innerText = clientUpdate.Prompt;
        }

        if (redraw) {
            this.redraw();
        }
        if (redrawSelected) {
            this.renderHighlightToCanvas2D(this.selectionTwod, this.selectedEntitiesDrawing);
        }
        if (redrawCursor) {
            this.drawCursor();
        }
    }

    private async updateDrawing(drawing: Drawing, clientDrawing: ClientDrawing): Promise<void> {
        drawing.Points = clientDrawing.Points;
        drawing.Lines = clientDrawing.Lines;
        drawing.Ellipses = clientDrawing.Ellipses;
        drawing.Text = clientDrawing.Text;
        drawing.Images = clientDrawing.Images;
        drawing.ImageElements = await Promise.all(clientDrawing.Images.map(async i => {
            const imageElement = await this.createImageElement(i);
            const result: [ClientImage, HTMLImageElement] = [i, imageElement];
            return result;
        }));
        this.populateVertices(drawing);
    }

    private createImageElement(clientImage: ClientImage): Promise<HTMLImageElement> {
        return new Promise(resolve => {
            const image = new Image();
            image.addEventListener('load', () => {
                resolve(image);
            });
            image.src = `data:${mimeTypeFromPath(clientImage.Path)};base64,${clientImage.Base64ImageData}`;
        });
    }

    private drawCursor() {
        let x = this.cursorPosition.x;
        let y = this.cursorPosition.y;
        this.twod.clearRect(0, 0, this.twod.canvas.width, this.twod.canvas.height);
        this.twod.lineWidth = 1;
        this.twod.fillStyle = `${ViewControl.colorToHex(this.settings.AutoColor)}11`; // default color, partial alpha
        this.twod.strokeStyle = ViewControl.colorToHex(this.settings.AutoColor);

        if (this.selectionState) {
            // dashed lines if partial entity
            this.twod.beginPath();
            this.twod.setLineDash(this.selectionState.Mode === SelectionMode.PartialEntity ? [5, 5] : []);

            // selection box
            this.twod.fillRect(
                this.selectionState.Rectangle.Left,
                this.selectionState.Rectangle.Top,
                this.selectionState.Rectangle.Width,
                this.selectionState.Rectangle.Height);

            // selection border
            this.twod.rect(
                this.selectionState.Rectangle.Left,
                this.selectionState.Rectangle.Top,
                this.selectionState.Rectangle.Width,
                this.selectionState.Rectangle.Height);

            this.twod.stroke();
        }

        // solid lines from here on out
        this.twod.beginPath();
        this.twod.setLineDash([]);

        // point cursor
        if (this.cursorState & CursorState.Point) {
            this.twod.moveTo(x - this.settings.CursorSize / 2, y);
            this.twod.lineTo(x + this.settings.CursorSize / 2, y);
            this.twod.moveTo(x, y - this.settings.CursorSize / 2);
            this.twod.lineTo(x, y + this.settings.CursorSize / 2);
        }

        // object cursor
        if (this.cursorState & CursorState.Object) {
            this.twod.moveTo(x - this.settings.EntitySelectionRadius, y - this.settings.EntitySelectionRadius);
            this.twod.lineTo(x + this.settings.EntitySelectionRadius, y - this.settings.EntitySelectionRadius);
            this.twod.lineTo(x + this.settings.EntitySelectionRadius, y + this.settings.EntitySelectionRadius);
            this.twod.lineTo(x - this.settings.EntitySelectionRadius, y + this.settings.EntitySelectionRadius);
            this.twod.lineTo(x - this.settings.EntitySelectionRadius, y - this.settings.EntitySelectionRadius);
        }

        // pan cursor
        if (this.cursorState & CursorState.Pan) {
            // slightly smaller point cursor
            const armLength = this.settings.CursorSize / 4;
            this.twod.moveTo(x - armLength, y);
            this.twod.lineTo(x + armLength, y);
            this.twod.moveTo(x, y - armLength);
            this.twod.lineTo(x, y + armLength);

            // arrowheads
            const arrowHeadSize = this.settings.CursorSize / 16;
            this.twod.moveTo(x - armLength + arrowHeadSize, y + arrowHeadSize);
            this.twod.lineTo(x - armLength, y);
            this.twod.lineTo(x - armLength + arrowHeadSize, y - arrowHeadSize);

            this.twod.moveTo(x + armLength - arrowHeadSize, y + arrowHeadSize);
            this.twod.lineTo(x + armLength, y);
            this.twod.lineTo(x + armLength - arrowHeadSize, y - arrowHeadSize);

            this.twod.moveTo(x - arrowHeadSize, y + armLength - arrowHeadSize);
            this.twod.lineTo(x, y + armLength);
            this.twod.lineTo(x + arrowHeadSize, y + armLength - arrowHeadSize);

            this.twod.moveTo(x - arrowHeadSize, y - armLength + arrowHeadSize);
            this.twod.lineTo(x, y - armLength);
            this.twod.lineTo(x + arrowHeadSize, y - armLength + arrowHeadSize);
        }

        this.twod.stroke();

        // snap points
        if (this.snapPointKind != SnapPointKind.None) {
            this.twod.beginPath();
            this.twod.lineWidth = 3;
            this.twod.strokeStyle = ViewControl.colorToHex(this.settings.SnapPointColor);

            // snap point always at cursor location?
            switch (this.snapPointKind) {
                case SnapPointKind.Center:
                    this.twod.ellipse(x, y, this.settings.SnapPointSize / 2, this.settings.SnapPointSize / 2, 0.0, 0.0, 360.0);
                    break;
                case SnapPointKind.EndPoint:
                    this.twod.moveTo(x - this.settings.SnapPointSize / 2, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x - this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x - this.settings.SnapPointSize / 2, y - this.settings.SnapPointSize / 2);
                    break;
                case SnapPointKind.MidPoint:
                    this.twod.moveTo(x - this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x - this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    break;
                case SnapPointKind.Quadrant:
                    this.twod.moveTo(x - this.settings.SnapPointSize / 2, y);
                    this.twod.lineTo(x, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y);
                    this.twod.lineTo(x, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x - this.settings.SnapPointSize / 2, y);
                    break;
                case SnapPointKind.Focus:
                    this.twod.moveTo(x, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x, y + this.settings.SnapPointSize / 2);
                    this.twod.moveTo(x - this.settings.SnapPointSize * 0.4, y - this.settings.SnapPointSize * 0.25);
                    this.twod.lineTo(x + this.settings.SnapPointSize * 0.4, y + this.settings.SnapPointSize * 0.25);
                    this.twod.moveTo(x - this.settings.SnapPointSize * 0.4, y + this.settings.SnapPointSize * 0.25);
                    this.twod.lineTo(x + this.settings.SnapPointSize * 0.4, y - this.settings.SnapPointSize * 0.25);
                    break;
                case SnapPointKind.Intersection:
                    this.twod.moveTo(x - this.settings.SnapPointSize / 2, y - this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.moveTo(x - this.settings.SnapPointSize / 2, y + this.settings.SnapPointSize / 2);
                    this.twod.lineTo(x + this.settings.SnapPointSize / 2, y - this.settings.SnapPointSize / 2);
                    break;
            }

            this.twod.stroke();
        }
    }

    private prepareCanvas() {
        let vertCode = `
            attribute vec3 vCoords;
            attribute vec3 vTrans;
            attribute vec3 vColor;

            uniform mat4 objectScaleTransform;
            uniform mat4 objectWorldTransform;
            uniform mat4 viewTransform;
            varying vec3 fColor;

            void main(void) {
                vec4 scaled = objectScaleTransform * vec4(vCoords, 1.0);
                vec4 moved = scaled + vec4(vTrans, 0.0);
                gl_Position = viewTransform * objectWorldTransform * moved;
                fColor = vColor;
            }`;

        let vertShader = this.gl.createShader(this.gl.VERTEX_SHADER)!;
        this.gl.shaderSource(vertShader, vertCode);
        this.gl.compileShader(vertShader);

        let fragCode = `
            precision mediump float;
            varying vec3 fColor;
            void main(void) {
                gl_FragColor = vec4(fColor, 255.0) / 255.0;
            }`;
        let fragShader = this.gl.createShader(this.gl.FRAGMENT_SHADER)!;
        this.gl.shaderSource(fragShader, fragCode);
        this.gl.compileShader(fragShader);

        let program = this.gl.createProgram()!;
        this.gl.attachShader(program, vertShader);
        this.gl.attachShader(program, fragShader);
        this.gl.linkProgram(program);
        this.gl.useProgram(program);

        this.coordinatesLocation = this.gl.getAttribLocation(program, "vCoords");
        this.translationLocation = this.gl.getAttribLocation(program, "vTrans");
        this.colorLocation = this.gl.getAttribLocation(program, "vColor");
        this.objectScaleTransformLocation = this.gl.getUniformLocation(program, "objectScaleTransform") || throwError('Unable to get object scale transform location');
        this.objectWorldTransformLocation = this.gl.getUniformLocation(program, "objectWorldTransform") || throwError('Unable to get object world transform location');
        this.viewTransformLocation = this.gl.getUniformLocation(program, "viewTransform") || throwError('Unable to get view transform location');
    }

    private prepareEvents() {
        this.cursorCanvas.addEventListener('wheel', (ev) => {
            this.client.zoom(ev.offsetX, ev.offsetY, -ev.deltaY);
        });
        this.outputPane.addEventListener('mousedown', async (ev) => {
            console.log(`mousedown ${ev.button} @ (${ev.offsetX}, ${ev.offsetY})`);
            this.client.mouseDown(ViewControl.getMouseButton(ev.button)!, ev.offsetX, ev.offsetY);
        });
        this.outputPane.addEventListener('mouseup', (ev) => {
            this.client.mouseUp(ViewControl.getMouseButton(ev.button)!, ev.offsetX, ev.offsetY);
        });
        this.outputPane.addEventListener('mousemove', async (ev) => {
            this.cursorPosition = { x: ev.offsetX, y: ev.offsetY };
            this.drawCursor();
            this.client.mouseMove(ev.offsetX, ev.offsetY);
        });
        (new ResizeObserver(() => {
            this.imageCanvas.width = this.outputPane.clientWidth;
            this.imageCanvas.height = this.outputPane.clientHeight;
            this.rubberBandImageCanvas.width = this.outputPane.clientWidth;
            this.rubberBandImageCanvas.height = this.outputPane.clientHeight;
            this.drawingCanvas.width = this.outputPane.clientWidth;
            this.drawingCanvas.height = this.outputPane.clientHeight;
            this.selectionCanvas.width = this.outputPane.clientWidth;
            this.selectionCanvas.height = this.outputPane.clientHeight;
            this.cursorCanvas.width = this.outputPane.clientWidth;
            this.cursorCanvas.height = this.outputPane.clientHeight;
            this.textCanvas.width = this.outputPane.clientWidth;
            this.textCanvas.height = this.outputPane.clientHeight;
            this.rubberBandTextCanvas.width = this.outputPane.clientWidth;
            this.rubberBandTextCanvas.height = this.outputPane.clientHeight;
            console.log(`sending resize with size (${this.drawingCanvas.width}, ${this.drawingCanvas.height})`);
            this.client.resize(this.drawingCanvas.width, this.drawingCanvas.height);
        })).observe(this.outputPane);

        // pan/zoom
        let elements = [
            { id: "panLeftButton", dxm: -1, dym: 0 },
            { id: "panRightButton", dxm: 1, dym: 0 },
            { id: "panUpButton", dxm: 0, dym: -1 },
            { id: "panDownButton", dxm: 0, dym: 1 }
        ];
        for (let i = 0; i < elements.length; i++) {
            let element = elements[i];
            (<HTMLButtonElement>document.getElementById(element.id)).addEventListener('click', () => {
                let delta = this.drawingCanvas.width / 8;
                this.client.pan(delta * element.dxm, delta * element.dym);
            });
        }

        (<HTMLButtonElement>document.getElementById("zoomInButton")).addEventListener('click', () => {
            let width = this.drawingCanvas.clientWidth;
            let height = this.drawingCanvas.clientHeight;
            this.client.zoomIn(width / 2, height / 2);
        });

        (<HTMLButtonElement>document.getElementById("zoomOutButton")).addEventListener('click', () => {
            let width = this.drawingCanvas.clientWidth;
            let height = this.drawingCanvas.clientHeight;
            this.client.zoomOut(width / 2, height / 2);
        });
    }

    private populateStaticVertices() {
        // ellipse
        let verts = [];
        for (let n = 0; n <= 720; n++) {
            const point = pointFromAngle(n);
            verts.push(point[0], point[1], 0.0);
        }
        let vertices = new Float32Array(verts);

        this.ellipseBuffer = this.gl.createBuffer() || throwError('Unable to create ellipse buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.ellipseBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);

        // point marker
        verts = [];
        verts.push(-0.5, 0.0, 0.0);
        verts.push(0.5, 0.0, 0.0);
        verts.push(0.0, -0.5, 0.0);
        verts.push(0.0, 0.5, 0.0);
        let pointMarkVertices = new Float32Array(verts);

        this.pointMarkBuffer = this.gl.createBuffer() || throwError('Unable to create point mark buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.pointMarkBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, pointMarkVertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);

        // hot points
        verts = [];
        verts.push(-0.5, -0.5, 0.0);
        verts.push(0.5, -0.5, 0.0);
        verts.push(0.5, -0.5, 0.0);
        verts.push(0.5, 0.5, 0.0);
        verts.push(0.5, 0.5, 0.0);
        verts.push(-0.5, 0.5, 0.0);
        verts.push(-0.5, 0.5, 0.0);
        verts.push(-0.5, -0.5, 0.0);
        let hotPointVertices = new Float32Array(verts);

        this.hotPointMarkBuffer = this.gl.createBuffer() || throwError('Unable to create hot point mark buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.hotPointMarkBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, hotPointVertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private populateVertices(drawing: Drawing) {
        // lines
        let lineVerts: number[] = [];
        let lineCols: number[] = [];
        let lineVertsWithDefaultColor: number[] = [];
        let lineCount = 0;
        let lineCountWithDefaultColor = 0;

        function addLine(p1: number[], p2: number[], color?: CadColor) {
            if (color) {
                lineVerts.push(p1[0], p1[1], p1[2]);
                lineVerts.push(p2[0], p2[1], p2[2]);
                lineCols.push(color.R, color.G, color.B);
                lineCols.push(color.R, color.G, color.B);
                lineCount++;
            } else {
                lineVertsWithDefaultColor.push(p1[0], p1[1], p1[2]);
                lineVertsWithDefaultColor.push(p2[0], p2[1], p2[2]);
                lineCountWithDefaultColor++;
            }
        }

        for (let l of drawing.Lines) {
            addLine([l.P1.X, l.P1.Y, l.P1.Z], [l.P2.X, l.P2.Y, l.P2.Z], l.Color);
        }

        // ellipses with non-integral start- and end-angles
        for (const el of drawing.Ellipses) {
            let startAngleTruncated = Math.trunc(el.StartAngle);
            const endAngleTruncated = Math.trunc(el.EndAngle);
            if (startAngleTruncated !== el.StartAngle) {
                startAngleTruncated++;
                const startLineA = transform(el.Transform, pointFromAngle(startAngleTruncated));
                const startLineB = transform(el.Transform, pointFromAngle(el.StartAngle));
                addLine(startLineA, startLineB, el.Color);
            }

            if (endAngleTruncated !== el.EndAngle) {
                const endLineA = transform(el.Transform, pointFromAngle(endAngleTruncated));
                const endLineB = transform(el.Transform, pointFromAngle(el.EndAngle));
                addLine(endLineA, endLineB, el.Color);
            }
        }

        let lineVertices = new Float32Array(lineVerts);
        let lineColors = new Uint8Array(lineCols);
        let lineVerticesWithDefaultColor = new Float32Array(lineVertsWithDefaultColor);

        drawing.LineVertices = this.gl.createBuffer() || throwError('Unable to create line vertices buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineVertices);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, lineVertices, this.gl.STATIC_DRAW);

        drawing.LineColors = this.gl.createBuffer() || throwError('Unable to create line colors buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineColors);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, lineColors, this.gl.STATIC_DRAW);

        drawing.LineVerticesWithDefaultColor = this.gl.createBuffer() || throwError('Unable to create line vertices with default color buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineVerticesWithDefaultColor);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, lineVerticesWithDefaultColor, this.gl.STATIC_DRAW);

        drawing.LineCount = lineCount;
        drawing.LineCountWithDefaultColor = lineCountWithDefaultColor;

        // points
        let pointVerts = [];
        let pointCols = [];
        let pointVertsWithDefaultColor = [];
        let pointCount = 0;
        let pointCountWithDefaultColor = 0;
        for (let p of drawing.Points) {
            if (p.Color) {
                pointVerts.push(p.Location.X, p.Location.Y, p.Location.Z);
                pointCols.push(p.Color.R, p.Color.G, p.Color.B);
                pointCount++;
            } else {
                pointVertsWithDefaultColor.push(p.Location.X, p.Location.Y, p.Location.Z);
                pointCountWithDefaultColor++;
            }
        }

        let pointVertices = new Float32Array(pointVerts);
        let pointColors = new Uint8Array(pointCols);
        let pointVerticesWithDefaultColor = new Float32Array(pointVertsWithDefaultColor);

        drawing.PointLocations = this.gl.createBuffer() || throwError('Unable to create point locations buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointLocations);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, pointVertices, this.gl.STATIC_DRAW);

        drawing.PointColors = this.gl.createBuffer() || throwError('Unable to create point colors buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointColors);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, pointColors, this.gl.STATIC_DRAW);

        drawing.PointLocationsWithDefaultColor = this.gl.createBuffer() || throwError('Unable to create point locations with default color buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointLocationsWithDefaultColor);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, pointVerticesWithDefaultColor, this.gl.STATIC_DRAW);

        drawing.PointCount = pointCount;
        drawing.PointCountWithDefaultColor = pointCountWithDefaultColor;
    }

    private populateHotPoints(points: Point[]) {
        this.hotPointCount = points.length;
        let verts = [];
        for (let p of points) {
            verts.push(p.X, p.Y, p.Z);
        }
        let vertices = new Float32Array(verts);
        this.hotPointLocations = this.gl.createBuffer() || throwError('Unable to create hot point locations');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.hotPointLocations);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private redraw() {
        this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
        this.gl.clearColor(0.0, 0.0, 0.0, 0.0);
        this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);
        this.redrawSpecific(this.entityDrawing);
        this.redrawSpecific(this.rubberBandDrawing);
        this.redrawText(this.textCtx, this.entityDrawing);
        this.redrawText(this.rubberTextCtx, this.rubberBandDrawing);
        this.redrawImages(this.imageTwoD, this.entityDrawing);
        this.redrawImages(this.rubberBandImageTwoD, this.rubberBandDrawing);
        this.redrawHotPoints();
    }

    private resetRenderer() {
        this.gl.disableVertexAttribArray(this.colorLocation!);
        this.gl.disableVertexAttribArray(this.translationLocation!);
        this.gl.vertexAttrib3f(this.translationLocation!, 0.0, 0.0, 0.0);
        this.gl.uniformMatrix4fv(this.viewTransformLocation, false, this.transform.Transform!);
        this.gl.uniformMatrix4fv(this.objectWorldTransformLocation, false, this.identity);
        this.gl.uniformMatrix4fv(this.objectScaleTransformLocation, false, this.identity);
        this.glAngle.vertexAttribDivisorANGLE(this.colorLocation!, 0);
        this.glAngle.vertexAttribDivisorANGLE(this.translationLocation!, 0);
    }

    private redrawSpecific(drawing: Drawing) {
        this.resetRenderer();

        //
        // lines
        //
        if (drawing.LineCount > 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineVertices);
            this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
            this.gl.enableVertexAttribArray(this.coordinatesLocation!);

            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineColors);
            this.gl.vertexAttribPointer(this.colorLocation!, 3, this.gl.UNSIGNED_BYTE, false, 0, 0);
            this.gl.enableVertexAttribArray(this.colorLocation!);

            this.gl.drawArrays(this.gl.LINES, 0, drawing.LineCount * 2); // 2 points per line
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
        }

        //
        // default color lines
        //
        if (drawing.LineCountWithDefaultColor > 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.LineVerticesWithDefaultColor);
            this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
            this.gl.enableVertexAttribArray(this.coordinatesLocation!);

            this.gl.disableVertexAttribArray(this.colorLocation!);
            this.gl.vertexAttrib3f(this.colorLocation!, this.settings.AutoColor.R, this.settings.AutoColor.G, this.settings.AutoColor.B);

            this.gl.drawArrays(this.gl.LINES, 0, drawing.LineCountWithDefaultColor * 2); // 2 points per line
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
        }

        //
        // ellipses (with specified and default colors)
        //
        this.gl.disableVertexAttribArray(this.colorLocation!);
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.ellipseBuffer);
        this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.gl.enableVertexAttribArray(this.coordinatesLocation!);
        for (let el of drawing.Ellipses) {
            this.gl.uniformMatrix4fv(this.objectWorldTransformLocation, false, el.Transform);
            let startAngle = Math.trunc(el.StartAngle);
            let endAngle = Math.trunc(el.EndAngle);
            if (startAngle !== el.StartAngle) {
                // start on next whole degree
                startAngle++;
            }

            let ellipseColor = el.Color || this.settings.AutoColor;
            this.gl.vertexAttrib3f(this.colorLocation!, ellipseColor.R, ellipseColor.G, ellipseColor.B);
            this.gl.drawArrays(this.gl.LINE_STRIP, startAngle, endAngle - startAngle + 1); // + 1 to account for end angle
        }

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);

        //
        // points
        //
        let pointMarkerScale = this.createConstantScaleTransform(this.settings.PointDisplaySize, this.settings.PointDisplaySize);
        this.gl.uniformMatrix4fv(this.objectScaleTransformLocation, false, pointMarkerScale);
        this.gl.uniformMatrix4fv(this.objectWorldTransformLocation, false, this.identity);

        // draw point mark repeatedly...
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.pointMarkBuffer);
        this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.gl.enableVertexAttribArray(this.coordinatesLocation!);

        // ...for each point location
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointLocations);
        this.gl.vertexAttribPointer(this.translationLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.glAngle.vertexAttribDivisorANGLE(this.translationLocation!, 1);
        this.gl.enableVertexAttribArray(this.translationLocation!);

        // ...and each color
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointColors);
        this.gl.vertexAttribPointer(this.colorLocation!, 3, this.gl.UNSIGNED_BYTE, false, 0, 0);
        this.glAngle.vertexAttribDivisorANGLE(this.colorLocation!, 1);
        this.gl.enableVertexAttribArray(this.colorLocation!);

        let vertsPerPoint = 2 * 2; // 2 segments, 2 verts per segment
        this.glAngle.drawArraysInstancedANGLE(this.gl.LINES, 0, vertsPerPoint, drawing.PointCount);
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);

        //
        // default color points
        //
        // ...for each point location
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.PointLocationsWithDefaultColor);
        this.gl.vertexAttribPointer(this.translationLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.glAngle.vertexAttribDivisorANGLE(this.translationLocation!, 1);
        this.gl.enableVertexAttribArray(this.translationLocation!);

        // ...with a static color
        this.gl.disableVertexAttribArray(this.colorLocation!);
        this.gl.vertexAttrib3f(this.colorLocation!, this.settings.AutoColor.R, this.settings.AutoColor.G, this.settings.AutoColor.B);

        this.glAngle.drawArraysInstancedANGLE(this.gl.LINES, 0, vertsPerPoint, drawing.PointCountWithDefaultColor);
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private renderHighlightToCanvas2D(context: CanvasRenderingContext2D, drawing: ClientDrawing) {
        context.clearRect(0, 0, context.canvas.width, context.canvas.height);
        context.lineWidth = 8;
        context.font = '10px monospace'; // no text drawn; used for text metrics

        for (const el of drawing.Ellipses) {
            this.renderEllipseToCanvas2D(context, el);
        }
        for (const line of drawing.Lines) {
            this.renderLineToCanvas2D(context, line);
        }
        for (const point of drawing.Points) {
            this.renderPointToCanvas2D(context, point);
        }
        for (const text of drawing.Text) {
            this.renderTextToCanvas2D(context, text);
        }
    }

    private renderEllipseToCanvas2D(context: CanvasRenderingContext2D, el: ClientEllipse) {
        context.beginPath();
        this.setContextHighlightStroke(context, this.settings.AutoColor);
        const start = transform(this.transform.CanvasTransform, transform(el.Transform, pointFromAngle(el.StartAngle)));
        context.moveTo(start[0], start[1]);
        for (let angle = el.StartAngle + 1; angle <= el.EndAngle; angle++) {
            const point = transform(this.transform.CanvasTransform, transform(el.Transform, pointFromAngle(angle)));
            context.lineTo(point[0], point[1]);
        }

        context.stroke();
    }

    private renderLineToCanvas2D(context: CanvasRenderingContext2D, line: ClientLine) {
        this.renderHighlightLineToCanvas(context, line.P1, line.P2, this.settings.AutoColor);
    }

    private renderPointToCanvas2D(context: CanvasRenderingContext2D, point: ClientPointLocation) {
        const center = transform(this.transform.CanvasTransform, [point.Location.X, point.Location.Y, point.Location.Z, 1]);
        context.beginPath();
        this.setContextHighlightStroke(context, this.settings.AutoColor);
        context.arc(center[0], center[1], 4, 0, 2 * Math.PI);
        context.stroke();
    }

    private renderTextToCanvas2D(context: CanvasRenderingContext2D, text: ClientText) {
        const metrics = context.measureText(text.Text);
        const bottomLeft = [text.Location.X, text.Location.Y, text.Location.Z, 1];
        const bottomRight = add(bottomLeft, [metrics.width, 0, 0, 0]);
        const topLeft = add(bottomLeft, [0, metrics.fontBoundingBoxAscent, 0, 0]);
        const topRight = add(bottomRight, [0, metrics.fontBoundingBoxAscent, 0, 0]);

        const screenBottomLeft = transform(this.transform.CanvasTransform, bottomLeft);
        const screenBottomRight = transform(this.transform.CanvasTransform, bottomRight);
        const screenTopLeft = transform(this.transform.CanvasTransform, topLeft);
        const screenTopRight = transform(this.transform.CanvasTransform, topRight);

        context.beginPath();
        this.setContextHighlightStroke(context, this.settings.AutoColor);
        context.moveTo(screenBottomLeft[0], screenBottomLeft[1]);
        context.lineTo(screenBottomRight[0], screenBottomRight[1]);
        context.lineTo(screenTopRight[0], screenTopRight[1]);
        context.lineTo(screenTopLeft[0], screenTopLeft[1]);
        context.closePath();
        context.stroke();
    }

    private renderHighlightLineToCanvas(context: CanvasRenderingContext2D, p1: ClientPoint, p2: ClientPoint, color: CadColor) {
        const p1Transformed = transform(this.transform.CanvasTransform, [p1.X, p1.Y, p1.Z, 1]);
        const p2Transformed = transform(this.transform.CanvasTransform, [p2.X, p2.Y, p2.Z, 1]);
        context.beginPath();
        this.setContextHighlightStroke(context, color);
        context.moveTo(p1Transformed[0], p1Transformed[1]);
        context.lineTo(p2Transformed[0], p2Transformed[1]);
        context.stroke();
    }

    private setContextHighlightStroke(context: CanvasRenderingContext2D, color: CadColor) {
        context.strokeStyle = `${ViewControl.colorToHex(color)}40`; // default color, partial alpha
    }

    private redrawText(textContext: CanvasRenderingContext2D, drawing: ClientDrawing) {
        textContext.setTransform(1, 0, 0, 1, 0, 0); // reset to identity
        textContext.clearRect(0, 0, textContext.canvas.width, textContext.canvas.height);
        const yscale = this.transform.CanvasTransform[5];

        for (let text of drawing.Text) {
            // this is only correct in very simple cases, but should be >90% of the time
            const location = transform(this.transform.CanvasTransform, [text.Location.X, text.Location.Y, text.Location.Z, 1]);
            const x = location[0];
            const y = location[1];
            const textHeight = text.Height * Math.abs(yscale);

            textContext.setTransform(1, 0, 0, 1, 0, 0); // reset to identity
            textContext.translate(x, y);
            textContext.rotate(-text.RotationAngle * Math.PI / 180.0);
            textContext.fillStyle = ViewControl.colorToHex(text.Color || this.settings.AutoColor);
            textContext.font = `${textHeight}px monospace`;
            textContext.fillText(text.Text, 0, 0);
        }
    }

    private redrawImages(context: CanvasRenderingContext2D, drawing: Drawing) {
        const scale = this.transform.CanvasTransform[0];
        context.clearRect(0, 0, context.canvas.width, context.canvas.height);
        for (const [image, img] of drawing.ImageElements) {
            const radians = image.Rotation * Math.PI / 180.0;
            const upVectorWorld = [-Math.sin(radians) * image.Height, Math.cos(radians) * image.Height];
            const topLeftWorld = [image.Location.X + upVectorWorld[0], image.Location.Y + upVectorWorld[1], image.Location.Z, 1];
            const topLeftScreen = transform(this.transform.CanvasTransform, topLeftWorld);
            const x = topLeftScreen[0];
            const y = topLeftScreen[1];
            const width = image.Width * scale;
            const height = image.Height * scale;

            context.save();
            context.translate(x, y);
            context.rotate(-radians);
            context.drawImage(img, 0, 0, width, height);
            context.restore();
        }
    }

    private redrawHotPoints() {
        if (this.hotPointCount == 0) {
            return;
        }

        this.resetRenderer();

        let hotPointScale = this.createConstantScaleTransform(this.settings.HotPointSize, this.settings.HotPointSize);
        this.gl.uniformMatrix4fv(this.objectScaleTransformLocation, false, hotPointScale);
        this.gl.uniformMatrix4fv(this.objectWorldTransformLocation, false, this.identity);

        // draw hot point repeatedly...
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.hotPointMarkBuffer);
        this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.gl.enableVertexAttribArray(this.coordinatesLocation!);

        // ...with a constant color
        this.gl.vertexAttrib3f(this.colorLocation!, this.settings.HotPointColor.R, this.settings.HotPointColor.G, this.settings.HotPointColor.B);

        // ...for each hot point location
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.hotPointLocations);
        this.gl.vertexAttribPointer(this.translationLocation!, 3, this.gl.FLOAT, false, 0, 0);
        this.glAngle.vertexAttribDivisorANGLE(this.translationLocation!, 1);
        this.gl.enableVertexAttribArray(this.translationLocation!);

        let vertsPerPoint = 4 * 2; // 4 segments, 2 verts per segment
        this.glAngle.drawArraysInstancedANGLE(this.gl.LINES, 0, vertsPerPoint, this.hotPointCount);
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private createConstantScaleTransform(x: number, y: number): number[] {
        let sfx = this.transform.DisplayXTransform * x;
        let sfy = this.transform.DisplayYTransform * y;
        let scale = [
            sfx, 0.0, 0.0, 0.0,
            0.0, sfy, 0.0, 0.0,
            0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0,
        ];
        return scale;
    }

    private static getMouseButton(button: number) {
        switch (button) {
            case 0:
                return MouseButton.Left;
            case 1:
                return MouseButton.Middle;
            case 2:
                return MouseButton.Right;
        }
    }

    private static numberToHex(n: number): string {
        let result = n.toString(16);
        while (result.length < 2) {
            result = '0' + result;
        }

        return result;
    }

    private static colorToHex(c: CadColor): string {
        return `#${ViewControl.numberToHex(c.R)}${ViewControl.numberToHex(c.G)}${ViewControl.numberToHex(c.B)}`;
    }
}

function add(vector1: number[], vector2: number[]): number[] {
    return [vector1[0] + vector2[0], vector1[1] + vector2[1], vector1[2] + vector2[2], 1];
}

function transform(matrix: number[], point: number[]): number[] {
    return [
        matrix[0] * point[0] + matrix[4] * point[1] + matrix[8] * point[2] + matrix[12] * point[3],
        matrix[1] * point[0] + matrix[5] * point[1] + matrix[9] * point[2] + matrix[13] * point[3],
        matrix[2] * point[0] + matrix[6] * point[1] + matrix[10] * point[2] + matrix[14] * point[3],
        matrix[3] * point[0] + matrix[7] * point[1] + matrix[11] * point[2] + matrix[15] * point[3]
    ];
}

function pointFromAngle(angle: number): number[] {
    return [Math.cos(angle * Math.PI / 180.0), Math.sin(angle * Math.PI / 180.0), 0.0, 1.0];
}

function toPoint(vector: number[]): Point {
    return { X: vector[0], Y: vector[1], Z: vector[2] };
}

function throwError<T>(message: string): T {
    throw new Error(message);
}

function mimeTypeFromPath(path: string): string {
    const extension = path.split('.').pop();
    switch (extension) {
        case 'bmp':
            return 'image/bmp';
        case 'jpg':
        case 'jpeg':
            return 'image/jpeg';
        case 'png':
            return 'image/png';
        default:
            return 'image/unknown';
    }
}
