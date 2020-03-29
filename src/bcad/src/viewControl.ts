import { Client, ClientDrawing, ClientSettings, ClientUpdate, Color, CursorState, MouseButton, SnapPointKind, SelectionState, SelectionMode } from './client';
import { remote } from 'electron';
import { ResizeObserver } from 'resize-observer';

interface Drawing extends ClientDrawing {
    Vertices: WebGLBuffer;
    Colors: WebGLBuffer;
}

export class ViewControl {
    // DOM
    private cursorCanvas: HTMLCanvasElement;
    private drawingCanvas: HTMLCanvasElement;
    private outputPane: HTMLDivElement;

    // webgl
    private gl: WebGLRenderingContext;
    private twod: CanvasRenderingContext2D;
    private worldTransformLocation: WebGLUniformLocation;
    private objectTransformLocation: WebGLUniformLocation;
    private ellipseBuffer: WebGLBuffer;
    private coordinatesLocation: number;
    private colorLocation: number;
    private identity: number[];
    private transform: number[];

    // CAD
    private client: Client;
    private entityDrawing: Drawing;
    private rubberBandDrawing: Drawing;
    private cursorPosition: {x: number, y: number};
    private cursorState: CursorState;
    private selectionState?: SelectionState;
    private snapPointKind: SnapPointKind;
    private settings: ClientSettings;

    constructor(client: Client) {
        this.client = client;

        // DOM
        this.cursorCanvas = <HTMLCanvasElement>document.getElementById('cursorCanvas');
        this.drawingCanvas = <HTMLCanvasElement>document.getElementById('drawingCanvas');
        this.outputPane = <HTMLDivElement>document.getElementById('output-pane');

        // CAD
        this.cursorPosition = {x: 0, y: 0};
        this.cursorState = CursorState.Object | CursorState.Point;
        this.selectionState = null;
        this.snapPointKind = SnapPointKind.None;
        this.entityDrawing = {
            CurrentLayer: "0",
            Layers: ["0"],
            FileName: null,
            Lines: [],
            Ellipses: [],
            Vertices: null,
            Colors: null,
        };
        this.rubberBandDrawing = {
            CurrentLayer: null,
            Layers: [],
            FileName: null,
            Lines: [],
            Ellipses: [],
            Vertices: null,
            Colors: null,
        };
        this.identity = [
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        ];
        this.transform = this.identity;
        this.settings = {
            AutoColor: {A: 255, R: 255, G: 255, B: 255},
            BackgroundColor: {A: 255, R: 255, G: 255, B: 255},
            CursorSize: 60,
            Debug: false,
            EntitySelectionRadius: 3,
            HotPointColor: {A: 255, R: 0, G: 0, B: 255},
            SnapPointColor: {A: 255, R: 255, G: 255, B: 0},
            SnapPointSize: 15,
            TextCursorSize: 18,
        };

        // render
        this.prepareCanvas();
        this.populateStaticVertices();
        this.populateVertices(this.entityDrawing);
        this.populateVertices(this.rubberBandDrawing);
        this.prepareEvents();
        this.redraw();

        this.client.subscribeToClientUpdates(clientUpdate => this.update(clientUpdate));
        this.client.ready(this.outputPane.clientWidth, this.outputPane.clientHeight);
    }

    private update(clientUpdate: ClientUpdate) {
        let redraw = false;
        let redrawCursor = false;
        if (clientUpdate.Drawing !== undefined) {
            this.entityDrawing.FileName = clientUpdate.Drawing.FileName;
            this.entityDrawing.Lines = clientUpdate.Drawing.Lines;
            this.entityDrawing.Ellipses = clientUpdate.Drawing.Ellipses;
            let fileName = this.entityDrawing.FileName || "(Untitled)";
            let dirtyText = clientUpdate.IsDirty ? " *" : "";
            let title = `BCad [${fileName}]${dirtyText}`;
            remote.getCurrentWindow().setTitle(title);
            this.populateVertices(this.entityDrawing);
            redraw = true;
        }
        if (clientUpdate.RubberBandDrawing !== undefined) {
            this.rubberBandDrawing.Lines = clientUpdate.RubberBandDrawing.Lines;
            this.rubberBandDrawing.Ellipses = clientUpdate.RubberBandDrawing.Ellipses;
            this.populateVertices(this.rubberBandDrawing);
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
        }
        if (clientUpdate.CursorState !== undefined) {
            this.cursorState = clientUpdate.CursorState;
            redrawCursor = true;
        }
        if (clientUpdate.HasSelectionStateUpdate) {
            this.selectionState = clientUpdate.SelectionState;
            redrawCursor = true;
        }
        if (clientUpdate.Settings !== undefined) {
            this.settings = clientUpdate.Settings;
            redraw = true;
            redrawCursor = true;
        }
        if (clientUpdate.Prompt !== undefined) {
            document.getElementById("prompt").innerText = clientUpdate.Prompt;
        }

        if (redraw) {
            this.redraw();
        }
        if (redrawCursor) {
            this.drawCursor();
        }
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
            }

            this.twod.stroke();
        }
    }

    private prepareCanvas() {
        this.twod = this.cursorCanvas.getContext("2d");
        this.gl = this.drawingCanvas.getContext("webgl");
        let gl = this.gl;

        let vertCode = `
            attribute vec3 vCoords;
            attribute vec3 vColor;

            uniform mat4 objectTransform;
            uniform mat4 worldTransform;
            varying vec3 fColor;

            void main(void) {
                gl_Position = worldTransform * objectTransform * vec4(vCoords, 1.0);
                fColor = vColor;
            }`;

        let vertShader = gl.createShader(gl.VERTEX_SHADER);
        gl.shaderSource(vertShader, vertCode);
        gl.compileShader(vertShader);

        let fragCode = `
            precision mediump float;
            varying vec3 fColor;
            void main(void) {
                gl_FragColor = vec4(fColor, 255.0) / 255.0;
            }`;
        let fragShader = gl.createShader(gl.FRAGMENT_SHADER);
        gl.shaderSource(fragShader, fragCode);
        gl.compileShader(fragShader);

        let program = gl.createProgram();
        gl.attachShader(program, vertShader);
        gl.attachShader(program, fragShader);
        gl.linkProgram(program);
        gl.useProgram(program);

        this.coordinatesLocation = gl.getAttribLocation(program, "vCoords");
        this.colorLocation = gl.getAttribLocation(program, "vColor");
        this.objectTransformLocation = gl.getUniformLocation(program, "objectTransform");
        this.worldTransformLocation = gl.getUniformLocation(program, "worldTransform");
    }

    private prepareEvents() {
        this.cursorCanvas.addEventListener('wheel', (ev) => {
            this.client.zoom(ev.offsetX, ev.offsetY, -ev.deltaY);
        });
        this.outputPane.addEventListener('mousedown', async (ev) => {
            console.log(`mousedown ${ev.button} @ (${ev.offsetX}, ${ev.offsetY})`);
            this.client.mouseDown(ViewControl.getMouseButton(ev.button), ev.offsetX, ev.offsetY);
        });
        this.outputPane.addEventListener('mouseup', (ev) => {
            this.client.mouseUp(ViewControl.getMouseButton(ev.button), ev.offsetX, ev.offsetY);
        });
        this.outputPane.addEventListener('mousemove', async (ev) => {
            this.cursorPosition = {x: ev.offsetX, y: ev.offsetY};
            this.drawCursor();
            this.client.mouseMove(ev.offsetX, ev.offsetY);
        });
        (new ResizeObserver(_entries => {
            this.drawingCanvas.width = this.outputPane.clientWidth;
            this.drawingCanvas.height = this.outputPane.clientHeight;
            this.cursorCanvas.width = this.outputPane.clientWidth;
            this.cursorCanvas.height = this.outputPane.clientHeight;
            console.log(`sending resize with size (${this.drawingCanvas.width}, ${this.drawingCanvas.height})`);
            this.client.resize(this.drawingCanvas.width, this.drawingCanvas.height);
        })).observe(this.outputPane);

        // pan/zoom
        let elements = [
            {id: "panLeftButton", dxm: -1, dym: 0},
            {id: "panRightButton", dxm: 1, dym: 0},
            {id: "panUpButton", dxm: 0, dym: -1},
            {id: "panDownButton", dxm: 0, dym: 1}
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
        let verts = [];
        for (let n = 0; n <= 720; n++) {
            let x = Math.cos(n * Math.PI / 180.0);
            let y = Math.sin(n * Math.PI / 180.0);
            verts.push(x, y, 0.0);
        }
        let vertices = new Float32Array(verts);

        this.ellipseBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.ellipseBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private populateVertices(drawing: Drawing) {
        let verts = [];
        let cols = [];
        for (let i = 0; i < drawing.Lines.length; i++) {
            verts.push(drawing.Lines[i].P1.X);
            verts.push(drawing.Lines[i].P1.Y);
            verts.push(drawing.Lines[i].P1.Z);
            cols.push(drawing.Lines[i].Color.R);
            cols.push(drawing.Lines[i].Color.G);
            cols.push(drawing.Lines[i].Color.B);
            verts.push(drawing.Lines[i].P2.X);
            verts.push(drawing.Lines[i].P2.Y);
            verts.push(drawing.Lines[i].P2.Z);
            cols.push(drawing.Lines[i].Color.R);
            cols.push(drawing.Lines[i].Color.G);
            cols.push(drawing.Lines[i].Color.B);
        }
        let vertices = new Float32Array(verts);
        let colors = new Uint8Array(cols);

        drawing.Vertices = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Vertices);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        drawing.Colors = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Colors);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, colors, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private redraw() {
        this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
        this.gl.clearColor(this.settings.BackgroundColor.R / 255.0, this.settings.BackgroundColor.G / 255.0, this.settings.BackgroundColor.B / 255.0, 1.0);
        this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);
        this.redrawSpecific(this.entityDrawing);
        this.redrawSpecific(this.rubberBandDrawing);
    }

    private redrawSpecific(drawing: Drawing) {
        this.gl.uniformMatrix4fv(this.worldTransformLocation, false, this.transform);
        this.gl.uniformMatrix4fv(this.objectTransformLocation, false, this.identity);

        // lines
        if (drawing.Lines.length > 0) {
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Vertices);
            this.gl.vertexAttribPointer(this.coordinatesLocation, 3, this.gl.FLOAT, false, 0, 0);
            this.gl.enableVertexAttribArray(this.coordinatesLocation);

            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Colors);
            this.gl.vertexAttribPointer(this.colorLocation, 3, this.gl.UNSIGNED_BYTE, false, 0, 0);
            this.gl.enableVertexAttribArray(this.colorLocation);

            this.gl.drawArrays(this.gl.LINES, 0, drawing.Lines.length * 2); // 2 points per line
            this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
        }

        // ellipses
        this.gl.disableVertexAttribArray(this.colorLocation);
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.ellipseBuffer);
        this.gl.vertexAttribPointer(this.coordinatesLocation, 3, this.gl.FLOAT, false, 0, 0);
        this.gl.enableVertexAttribArray(this.coordinatesLocation);
        for (let i = 0; i < drawing.Ellipses.length; i++) {
            let el = drawing.Ellipses[i];
            this.gl.uniformMatrix4fv(this.objectTransformLocation, false, el.Transform);
            let startAngle = Math.trunc(el.StartAngle);
            let endAngle = Math.trunc(el.EndAngle);

            this.gl.vertexAttrib3f(this.colorLocation, el.Color.R, el.Color.G, el.Color.B);
            this.gl.drawArrays(this.gl.LINE_STRIP, startAngle, endAngle - startAngle + 1); // + 1 to account for end angle
        }
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

    private static colorToHex(c: Color): string {
        return `#${ViewControl.numberToHex(c.R)}${ViewControl.numberToHex(c.G)}${ViewControl.numberToHex(c.B)}`;
    }
}
