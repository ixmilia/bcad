import { Client } from './client';
import { ResizeObserver } from 'resize-observer';
import {
    CadColor,
    ClientBezier,
    ClientDrawing,
    ClientEllipse,
    ClientImage,
    ClientLine,
    ClientPoint,
    ClientPointLocation,
    ClientSettings,
    ClientText,
    ClientTransform,
    ClientTriangle,
    ClientUpdate,
    CursorState,
    DrawingUnits,
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

    TriangleCount: number;
    TriangleVertices: WebGLBuffer;
    TriangleColors: WebGLBuffer;

    TriangleCountWithDefaultColor: number;
    TriangleVerticesWithDefaultColor: WebGLBuffer;

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

    private flatCanvas: HTMLCanvasElement;
    private rubberBandFlatCanvas: HTMLCanvasElement;
    private flatContext: CanvasRenderingContext2D;
    private rubberBandFlatContext: CanvasRenderingContext2D;

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

    private frameTimes: number[] = [];
    private maxFrameTimes: number = 10;

    constructor(client: Client, private reportFps: (fps: number) => void) {
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

        this.flatCanvas = <HTMLCanvasElement>document.getElementById('flatCanvas');
        this.rubberBandFlatCanvas = <HTMLCanvasElement>document.getElementById('rubberBandFlatCanvas');

        // CAD
        this.cursorPosition = { x: 0, y: 0 };
        this.cursorState = CursorState.Object | CursorState.Point;
        this.selectionState = undefined;
        this.snapPointKind = SnapPointKind.None;
        this.hotPointCount = 0;
        this.entityDrawing = {
            CurrentLayer: "0",
            Layers: ["0"],
            LineTypes: [],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
            Triangles: [],
            Beziers: [],
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
            TriangleCount: 0,
            TriangleVertices: {},
            TriangleColors: {},
            TriangleCountWithDefaultColor: 0,
            TriangleVerticesWithDefaultColor: {},
            ImageElements: [],
            CurrentDimensionStyle: '',
            DimensionStyles: [''],
        };
        this.selectedEntitiesDrawing = {
            CurrentLayer: "0",
            Layers: ["0"],
            LineTypes: [],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
            Triangles: [],
            Beziers: [],
            CurrentDimensionStyle: '',
            DimensionStyles: [''],
        };
        this.rubberBandDrawing = {
            CurrentLayer: "",
            Layers: [],
            LineTypes: [],
            FileName: "",
            Points: [],
            Lines: [],
            Ellipses: [],
            Text: [],
            Images: [],
            Triangles: [],
            Beziers: [],
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
            TriangleCount: 0,
            TriangleVertices: {},
            TriangleColors: {},
            TriangleCountWithDefaultColor: 0,
            TriangleVerticesWithDefaultColor: {},
            ImageElements: [],
            CurrentDimensionStyle: '',
            DimensionStyles: [''],
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
            AnglePrecision: 0,
            DrawingPrecision: 16,
            DrawingUnits: DrawingUnits.English,
            UnitFormat: UnitFormat.Architectural,
            EntitySelectionRadius: 3,
            HotPointColor: { A: 255, R: 0, G: 0, B: 255 },
            HotPointSize: 10,
            RenderId: 'webgl',
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
        this.flatContext = this.flatCanvas.getContext('2d') || throwError('Unable to get flat canvas 2d context');
        this.rubberBandFlatContext = this.rubberBandFlatCanvas.getContext('2d') || throwError('Unable to get rubber band flat canvas 2d context');
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
        drawing.Triangles = clientDrawing.Triangles;
        drawing.Beziers = clientDrawing.Beziers;
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
            this.flatCanvas.width = this.outputPane.clientWidth;
            this.flatCanvas.height = this.outputPane.clientHeight;
            this.rubberBandFlatCanvas.width = this.outputPane.clientWidth;
            this.rubberBandFlatCanvas.height = this.outputPane.clientHeight;
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
        const bezierLineSegments = 20;
        for (const b of drawing.Beziers) {
            let last = [b.P1.X, b.P1.Y, b.P1.Z];
            for (let i = 0; i <= bezierLineSegments; i++) {
                const t = i / bezierLineSegments;
                const tprime = 1 - t;
                const next = [];
                next[0] = tprime * tprime * tprime * b.P1.X + 3 * tprime * tprime * t * b.P2.X + 3 * tprime * t * t * b.P3.X + t * t * t * b.P4.X;
                next[1] = tprime * tprime * tprime * b.P1.Y + 3 * tprime * tprime * t * b.P2.Y + 3 * tprime * t * t * b.P3.Y + t * t * t * b.P4.Y;
                next[2] = tprime * tprime * tprime * b.P1.Z + 3 * tprime * tprime * t * b.P2.Z + 3 * tprime * t * t * b.P3.Z + t * t * t * b.P4.Z;
                addLine([last[0], last[1], last[2]], [next[0], next[1], next[2]], b.Color);
                last = next;
            }
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

        // triangles
        const triangleVerts = [];
        const triangleCols = [];
        const triangleVertsWithDefaultColor = [];
        let triangleCount = 0;
        let triangleCountWithDefaultColor = 0;
        for (const t of drawing.Triangles) {
            if (t.Color) {
                triangleVerts.push(t.P1.X, t.P1.Y, t.P1.Z);
                triangleVerts.push(t.P2.X, t.P2.Y, t.P2.Z);
                triangleVerts.push(t.P3.X, t.P3.Y, t.P3.Z);
                triangleCols.push(t.Color.R, t.Color.G, t.Color.B);
                triangleCount++;
            } else {
                triangleVertsWithDefaultColor.push(t.P1.X, t.P1.Y, t.P1.Z);
                triangleVertsWithDefaultColor.push(t.P2.X, t.P2.Y, t.P2.Z);
                triangleVertsWithDefaultColor.push(t.P3.X, t.P3.Y, t.P3.Z);
                triangleCountWithDefaultColor++;
            }
        }

        const triangleVertices = new Float32Array(triangleVerts);
        const triangleColors = new Uint8Array(triangleCols);
        const triangleVerticesWithDefaultColor = new Float32Array(triangleVertsWithDefaultColor);

        drawing.TriangleVertices = this.gl.createBuffer() || throwError('Unable to create triangle locations buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleVertices);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, triangleVertices, this.gl.STATIC_DRAW);

        drawing.TriangleColors = this.gl.createBuffer() || throwError('Unable to create triangle colors buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleColors);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, triangleColors, this.gl.STATIC_DRAW);

        drawing.TriangleVerticesWithDefaultColor = this.gl.createBuffer() || throwError('Unable to create triangle locations with default color buffer');
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleVerticesWithDefaultColor);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, triangleVerticesWithDefaultColor, this.gl.STATIC_DRAW);

        drawing.TriangleCount = triangleCount;
        drawing.TriangleCountWithDefaultColor = triangleCountWithDefaultColor;
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
        this.measureFrameTime(() => {
            // clear flat renderer
            this.flatContext.clearRect(0, 0, this.flatContext.canvas.width, this.flatContext.canvas.height);
            this.rubberBandFlatContext.clearRect(0, 0, this.rubberBandFlatContext.canvas.width, this.rubberBandFlatContext.canvas.height);
            // clear gl renderer
            this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
            this.gl.clearColor(0.0, 0.0, 0.0, 0.0);
            this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);

            switch (this.settings.RenderId) {
                case 'canvas':
                    this.redraw2d(this.flatContext, this.entityDrawing);
                    this.redraw2d(this.rubberBandFlatContext, this.rubberBandDrawing);
                    break;
                case 'webgl':
                    this.redrawSpecific(this.entityDrawing);
                    this.redrawSpecific(this.rubberBandDrawing);
                    break;
            }

            this.redrawText(this.textCtx, this.entityDrawing);
            this.redrawText(this.rubberTextCtx, this.rubberBandDrawing);
            this.redrawImages(this.imageTwoD, this.entityDrawing);
            this.redrawImages(this.rubberBandImageTwoD, this.rubberBandDrawing);
            this.redrawHotPoints();
        });

        if (this.frameTimes.length > 0) {
            const avgFrameTime = this.frameTimes.reduce((a, b) => a + b, 0) / this.frameTimes.length;
            const fps = Math.round(1000 / avgFrameTime);
            this.reportFps(fps);
        }
    }

    private measureFrameTime(body: () => void) {
        const start = performance.now();
        body();
        const now = performance.now();
        const diff = now - start;
        this.frameTimes.push(diff);
        if (this.frameTimes.length > this.maxFrameTimes) {
            this.frameTimes.shift();
        }
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
        // triangles
        //
        if (drawing.TriangleCount > 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleVertices);
            this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
            this.gl.enableVertexAttribArray(this.coordinatesLocation!);

            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleColors);
            this.gl.vertexAttribPointer(this.colorLocation!, 3, this.gl.UNSIGNED_BYTE, false, 0, 0);
            this.gl.enableVertexAttribArray(this.colorLocation!);

            this.gl.drawArrays(this.gl.TRIANGLES, 0, drawing.TriangleCount * 3); // 3 points per triangle
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
        }

        //
        // default color triangles
        //
        if (drawing.TriangleCountWithDefaultColor > 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.TriangleVerticesWithDefaultColor);
            this.gl.vertexAttribPointer(this.coordinatesLocation!, 3, this.gl.FLOAT, false, 0, 0);
            this.gl.enableVertexAttribArray(this.coordinatesLocation!);

            this.gl.disableVertexAttribArray(this.colorLocation!);
            this.gl.vertexAttrib3f(this.colorLocation!, this.settings.AutoColor.R, this.settings.AutoColor.G, this.settings.AutoColor.B);

            this.gl.drawArrays(this.gl.TRIANGLES, 0, drawing.TriangleCountWithDefaultColor * 3); // 3 points per triangle
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
        }

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
        for (const triangle of drawing.Triangles) {
            this.renderTriangleToCanvas2D(context, triangle);
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
        const textScaleFactor = text.Height / metrics.fontBoundingBoxAscent;
        const bottomLeft = [text.Location.X, text.Location.Y, text.Location.Z, 1];
        const rotationInRadians = text.RotationAngle * Math.PI / 180.0;
        const rotation = createRotationMatrix(rotationInRadians);
        const rightVector = [metrics.actualBoundingBoxRight * textScaleFactor, 0.0, 0.0, 1.0];
        const upVector = [0.0, text.Height, 0.0, 1.0];
        const rotatedRightVector = transform(rotation, rightVector);
        const rotatedUpVector = transform(rotation, upVector);
        const bottomRight = add(bottomLeft, rotatedRightVector);
        const topLeft = add(bottomLeft, rotatedUpVector);
        const topRight = add(bottomRight, rotatedUpVector);

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

    private renderTriangleToCanvas2D(context: CanvasRenderingContext2D, triangle: ClientTriangle) {
        const p1Transformed = transform(this.transform.CanvasTransform, [triangle.P1.X, triangle.P1.Y, triangle.P1.Z, 1]);
        const p2Transformed = transform(this.transform.CanvasTransform, [triangle.P2.X, triangle.P2.Y, triangle.P2.Z, 1]);
        const p3Transformed = transform(this.transform.CanvasTransform, [triangle.P3.X, triangle.P3.Y, triangle.P3.Z, 1]);
        context.fillStyle = ViewControl.colorToHex(triangle.Color || this.settings.AutoColor);
        context.beginPath();
        context.moveTo(p1Transformed[0], p1Transformed[1]);
        context.lineTo(p2Transformed[0], p2Transformed[1]);
        context.lineTo(p3Transformed[0], p3Transformed[1]);
        context.closePath();
        context.stroke();
        context.fill();
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

    private redraw2d(context: CanvasRenderingContext2D, drawing: ClientDrawing) {
        for (const line of drawing.Lines) {
            this.redrawLine(context, line);
        }

        for (const el of drawing.Ellipses) {
            this.redrawEllipse(context, el);
        }

        for (const b of drawing.Beziers) {
            this.redrawBezier(context, b);
        }

        for (const t of drawing.Triangles) {
            this.redrawTriangle(context, t);
        }

        context.setLineDash([]);
        for (const point of drawing.Points) {
            this.redrawPoint(context, point);
        }
    }

    private redrawLine(context: CanvasRenderingContext2D, line: ClientLine) {
        const p1 = transform(this.transform.CanvasTransform, [line.P1.X, line.P1.Y, line.P1.Z, 1.0]);
        const p2 = transform(this.transform.CanvasTransform, [line.P2.X, line.P2.Y, line.P2.Z, 1.0]);
        context.beginPath();
        context.strokeStyle = ViewControl.colorToHex(line.Color || this.settings.AutoColor);
        context.setLineDash(line.LinePattern.map(p => p * this.transform.CanvasTransform[0]));
        context.moveTo(p1[0], p1[1]);
        context.lineTo(p2[0], p2[1]);
        context.stroke();
    }

    private redrawEllipse(context: CanvasRenderingContext2D, el: ClientEllipse) {
        const matrix = multiply(el.Transform, this.transform.CanvasTransform);
        const center = transform(matrix, [0, 0, 0, 1]);
        const xaxis = sub(transform(matrix, [1, 0, 0, 1]), center);
        const yaxis = sub(transform(matrix, [0, 1, 0, 1]), center);
        const radiusX = len(xaxis);
        const radiusY = len(yaxis);
        const rotation = Math.atan2(xaxis[1], xaxis[0]);
        context.beginPath();
        context.strokeStyle = ViewControl.colorToHex(el.Color || this.settings.AutoColor);
        context.setLineDash(el.LinePattern.map(p => p * this.transform.CanvasTransform[0]));
        context.ellipse(center[0], center[1], radiusX, radiusY, rotation, el.StartAngle * Math.PI / -180.0, el.EndAngle * Math.PI / -180.0, true);
        context.stroke();
    }

    private redrawBezier(context: CanvasRenderingContext2D, b: ClientBezier) {
        const p1 = transform(this.transform.CanvasTransform, [b.P1.X, b.P1.Y, b.P1.Z, 1.0]);
        const p2 = transform(this.transform.CanvasTransform, [b.P2.X, b.P2.Y, b.P2.Z, 1.0]);
        const p3 = transform(this.transform.CanvasTransform, [b.P3.X, b.P3.Y, b.P3.Z, 1.0]);
        const p4 = transform(this.transform.CanvasTransform, [b.P4.X, b.P4.Y, b.P4.Z, 1.0]);
        context.beginPath();
        context.strokeStyle = ViewControl.colorToHex(b.Color || this.settings.AutoColor);
        context.setLineDash(b.LinePattern.map(p => p * this.transform.CanvasTransform[0]));
        context.moveTo(p1[0], p1[1]);
        context.bezierCurveTo(p2[0], p2[1], p3[0], p3[1], p4[0], p4[1]);
        context.stroke();
    }

    private redrawTriangle(context: CanvasRenderingContext2D, triangle: ClientTriangle) {
        const p1 = transform(this.transform.CanvasTransform, [triangle.P1.X, triangle.P1.Y, triangle.P1.Z, 1]);
        const p2 = transform(this.transform.CanvasTransform, [triangle.P2.X, triangle.P2.Y, triangle.P2.Z, 1]);
        const p3 = transform(this.transform.CanvasTransform, [triangle.P3.X, triangle.P3.Y, triangle.P3.Z, 1]);
        context.beginPath();
        context.fillStyle = ViewControl.colorToHex(triangle.Color || this.settings.AutoColor);
        context.strokeStyle = ViewControl.colorToHex(triangle.Color || this.settings.AutoColor);
        context.moveTo(p1[0], p1[1]);
        context.lineTo(p2[0], p2[1]);
        context.lineTo(p3[0], p3[1]);
        context.closePath();
        context.stroke();
        context.fill();
    }

    private redrawPoint(context: CanvasRenderingContext2D, point: ClientPointLocation) {
        const location = transform(this.transform.CanvasTransform, [point.Location.X, point.Location.Y, point.Location.Z, 1]);
        context.beginPath();
        context.strokeStyle = ViewControl.colorToHex(point.Color || this.settings.AutoColor);
        const halfSize = this.settings.PointDisplaySize * 0.5;
        context.moveTo(location[0] - halfSize, location[1]);
        context.lineTo(location[0] + halfSize, location[1]);
        context.moveTo(location[0], location[1] - halfSize);
        context.lineTo(location[0], location[1] + halfSize);
        context.stroke();
    }

    private redrawText(context: CanvasRenderingContext2D, drawing: ClientDrawing) {
        context.setTransform(1, 0, 0, 1, 0, 0); // reset to identity
        context.clearRect(0, 0, context.canvas.width, context.canvas.height);
        const yscale = this.transform.CanvasTransform[5];

        for (let text of drawing.Text) {
            // this is only correct in very simple cases, but should be >90% of the time
            const location = transform(this.transform.CanvasTransform, [text.Location.X, text.Location.Y, text.Location.Z, 1]);
            const x = location[0];
            const y = location[1];
            const textHeight = text.Height * Math.abs(yscale);

            context.setTransform(1, 0, 0, 1, 0, 0); // reset to identity
            context.translate(x, y);
            context.rotate(-text.RotationAngle * Math.PI / 180.0);
            context.fillStyle = ViewControl.colorToHex(text.Color || this.settings.AutoColor);
            context.font = `${textHeight}px monospace`;
            context.fillText(text.Text, 0, 0);
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

function sub(vector1: number[], vector2: number[]): number[] {
    return [vector1[0] - vector2[0], vector1[1] - vector2[1], vector1[2] - vector2[2], 1];
}

function len(vector: number[]) {
    return Math.sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
}

function createRotationMatrix(angle: number): number[] {
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    return [
        cos, sin, 0.0, 0.0,
        -sin, cos, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 0.0, 1.0
    ];
}

function multiply(matrix1: number[], matrix2: number[]): number[] {
    return [
        matrix1[0] * matrix2[0] + matrix1[1] * matrix2[4] +
        matrix1[2] * matrix2[8] + matrix1[3] * matrix2[12],
        matrix1[0] * matrix2[1] + matrix1[1] * matrix2[5] +
        matrix1[2] * matrix2[9] + matrix1[3] * matrix2[13],
        matrix1[0] * matrix2[2] + matrix1[1] * matrix2[6] +
        matrix1[2] * matrix2[10] + matrix1[3] * matrix2[14],
        matrix1[0] * matrix2[3] + matrix1[1] * matrix2[7] +
        matrix1[2] * matrix2[11] + matrix1[3] * matrix2[15],
        matrix1[4] * matrix2[0] + matrix1[5] * matrix2[4] +
        matrix1[6] * matrix2[8] + matrix1[7] * matrix2[12],
        matrix1[4] * matrix2[1] + matrix1[5] * matrix2[5] +
        matrix1[6] * matrix2[9] + matrix1[7] * matrix2[13],
        matrix1[4] * matrix2[2] + matrix1[5] * matrix2[6] +
        matrix1[6] * matrix2[10] + matrix1[7] * matrix2[14],
        matrix1[4] * matrix2[3] + matrix1[5] * matrix2[7] +
        matrix1[6] * matrix2[11] + matrix1[7] * matrix2[15],
        matrix1[8] * matrix2[0] + matrix1[9] * matrix2[4] +
        matrix1[10] * matrix2[8] + matrix1[11] * matrix2[12],
        matrix1[8] * matrix2[1] + matrix1[9] * matrix2[5] +
        matrix1[10] * matrix2[9] + matrix1[11] * matrix2[13],
        matrix1[8] * matrix2[2] + matrix1[9] * matrix2[6] +
        matrix1[10] * matrix2[10] + matrix1[11] * matrix2[14],
        matrix1[8] * matrix2[3] + matrix1[9] * matrix2[7] +
        matrix1[10] * matrix2[11] + matrix1[11] * matrix2[15],
        matrix1[12] * matrix2[0] + matrix1[13] * matrix2[4] +
        matrix1[14] * matrix2[8] + matrix1[15] * matrix2[12],
        matrix1[12] * matrix2[1] + matrix1[13] * matrix2[5] +
        matrix1[14] * matrix2[9] + matrix1[15] * matrix2[13],
        matrix1[12] * matrix2[2] + matrix1[13] * matrix2[6] +
        matrix1[14] * matrix2[10] + matrix1[15] * matrix2[14],
        matrix1[12] * matrix2[3] + matrix1[13] * matrix2[7] +
        matrix1[14] * matrix2[11] + matrix1[15] * matrix2[15]
    ];
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
