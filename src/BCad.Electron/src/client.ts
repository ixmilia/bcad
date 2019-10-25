import * as cp from 'child_process';
import * as rpc from 'vscode-jsonrpc';
import { remote } from 'electron';

interface Point3 {
    X: number;
    Y: number;
    Z: number;
}

interface Color {
    A: number;
    R: number;
    G: number;
    B: number;
}

interface Line {
    P1: Point3;
    P2: Point3;
    Color: Color;
}

interface Ellipse {
    StartAngle: number;
    EndAngle: number;
    Transform: number[];
    Color: Color;
}

interface ClientDrawing {
    FileName: string;
    Lines: Line[];
    Ellipses: Ellipse[];
}

interface Drawing extends ClientDrawing {
    Vertices: WebGLBuffer;
    Colors: WebGLBuffer;
}

enum CursorState {
    None = 0,
    Point = 1,
    Object = 2,
    Text = 4,
}

interface ClientUpdate {
    IsDirty: boolean;
    Transform?: number[];
    Drawing?: ClientDrawing;
    RubberBandDrawing?: ClientDrawing;
    CursorState?: CursorState;
    Prompt?: string;
    OutputLines?: string[];
}

enum MouseButton {
    Left,
    Middle,
    Right,
}

export class Client {
    private connection: rpc.MessageConnection;
    private outputPane: HTMLDivElement;
    private drawingCanvas: HTMLCanvasElement;
    private cursorCanvas: HTMLCanvasElement;
    private gl: WebGLRenderingContext;
    private twod: CanvasRenderingContext2D;
    private entityDrawing: Drawing;
    private rubberBandDrawing: Drawing;
    private identity: number[];
    private transform: number[];
    private coordinatesLocation: number;
    private colorLocation: number;
    private worldTransformLocation: WebGLUniformLocation;
    private objectTransformLocation: WebGLUniformLocation;
    private ellipseBuffer: WebGLBuffer;
    private cursorPosition: {x: number, y: number};
    private cursorState: CursorState;

    // client notifications
    private ClientUpdateNotification: rpc.NotificationType<ClientUpdate[], void>;
    private MouseDownNotification: rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>;
    private MouseUpNotification: rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>;
    private MouseMoveNotification: rpc.NotificationType<{cursorX: Number, cursorY: Number}, void>;
    private PanNotification: rpc.NotificationType<{dx: Number, dy: Number}, void>;
    private ReadyNotification: rpc.NotificationType<{width: Number, height: Number}, void>;
    private SubmitIntputNotification: rpc.NotificationType<{value: string}, void>;
    private ZoomNotification: rpc.NotificationType<{cursorX: Number, cursorY: Number, delta: Number}, void>;
    private ExecuteCommandRequest: rpc.RequestType1<{command: String}, boolean, void, void>;

    constructor() {
        this.drawingCanvas = <HTMLCanvasElement> document.getElementById('drawingCanvas');
        this.cursorCanvas = <HTMLCanvasElement> document.getElementById('cursorCanvas');
        this.outputPane = <HTMLDivElement> document.getElementById('output-pane');
        this.ClientUpdateNotification = new rpc.NotificationType<ClientUpdate[], void>('ClientUpdate');
        this.MouseDownNotification = new rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>('MouseDown');
        this.MouseUpNotification = new rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>('MouseUp');
        this.MouseMoveNotification = new rpc.NotificationType<{cursorX: Number, cursorY: Number}, void>('MouseMove');
        this.PanNotification = new rpc.NotificationType<{dx: Number, dy: Number}, void>('Pan');
        this.ReadyNotification = new rpc.NotificationType<{width: Number, height: Number}, void>('Ready');
        this.SubmitIntputNotification = new rpc.NotificationType<{value: string}, void>('SubmitInput');
        this.ZoomNotification = new rpc.NotificationType<{cursorX: Number, cursorY: Number, delta: Number}, void>('Zoom');
        this.ExecuteCommandRequest = new rpc.RequestType1<{command: String}, boolean, void, void>('ExecuteCommand');
        this.cursorPosition = {x: 0, y: 0};
        this.cursorState = CursorState.Object | CursorState.Point;
    }

    start() {
        this.entityDrawing = {
            FileName: null,
            Lines: [],
            Ellipses: [],
            Vertices: null,
            Colors: null,
        };
        this.rubberBandDrawing = {
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
        this.prepareCanvas();
        this.populateStaticVertices();
        this.populateVertices(this.entityDrawing);
        this.populateVertices(this.rubberBandDrawing);
        this.redraw();
        this.prepareConnection();
        this.prepareEvents();
        this.prepareListeners();
        this.connection.listen();
        this.connection.sendNotification(this.ReadyNotification, {width: this.drawingCanvas.width, height: this.drawingCanvas.height});
    }

    private prepareConnection() {
        let serverAssembly = "IxMilia.BCad.Server.dll";
        var serverPath = __dirname + '/../bin/' + serverAssembly;
        serverPath = __dirname + '/../../../artifacts/bin/IxMilia.BCad.Server/Debug/netcoreapp3.0/' + serverAssembly;
        let childProcess = cp.spawn('dotnet.exe', [serverPath]);
        childProcess.on('exit', (code: number, _signal: string) => {
            alert('process exited with ' + code);
        });
        console.log('server pid ' + childProcess.pid);
        let logger: rpc.Logger = {
            error: console.log,
            warn: console.log,
            info: console.log,
            log: console.log,
        };
        this.connection = rpc.createMessageConnection(
            new rpc.StreamMessageReader(childProcess.stdout),
            new rpc.StreamMessageWriter(childProcess.stdin),
            logger);
    }

    private zoom(cursorX: Number, cursorY: Number, delta: Number) {
        this.connection.sendNotification(this.ZoomNotification, {cursorX: cursorX, cursorY: cursorY, delta: delta});
    }

    private zoomIn(cursorX: Number, cursorY: Number) {
        this.zoom(cursorX, cursorY, 1);
    }

    private zoomOut(cursorX: Number, cursorY: Number) {
        this.zoom(cursorX, cursorY, -1);
    }

    private getMouseButton(button: number) {
        switch (button) {
            case 0:
                return MouseButton.Left;
            case 1:
                return MouseButton.Middle;
            case 2:
                return MouseButton.Right;
        }
    }

    private prepareEvents() {
        this.outputPane.addEventListener('mousedown', async (ev) => {
            this.connection.sendNotification(this.MouseDownNotification, {button: this.getMouseButton(ev.button), cursorX: ev.offsetX, cursorY: ev.offsetY});
        });
        this.outputPane.addEventListener('mouseup', (ev) => {
            this.connection.sendNotification(this.MouseUpNotification, {button: this.getMouseButton(ev.button), cursorX: ev.offsetX, cursorY: ev.offsetY});
        });
        this.outputPane.addEventListener('mousemove', async (ev) => {
            this.cursorPosition = {x: ev.offsetX, y: ev.offsetY};
            this.drawCursor();
            this.connection.sendNotification(this.MouseMoveNotification, {cursorX: ev.offsetX, cursorY: ev.offsetY});
        });
        this.outputPane.addEventListener('resize', () => {
            // TODO:
        });
        var elements = [
            {id: "panLeftButton", dxm: -1, dym: 0},
            {id: "panRightButton", dxm: 1, dym: 0},
            {id: "panUpButton", dxm: 0, dym: -1},
            {id: "panDownButton", dxm: 0, dym: 1}
        ];
        for (var i = 0; i < elements.length; i++) {
            let element = elements[i];
            (<HTMLButtonElement>document.getElementById(element.id)).addEventListener('click', () => {
                let delta = this.drawingCanvas.width / 8;
                this.connection.sendNotification(this.PanNotification, {
                    dx: delta * element.dxm,
                    dy: delta * element.dym
                });
            });
        }

        (<HTMLButtonElement>document.getElementById("zoomInButton")).addEventListener('click', () => {
            let width = this.drawingCanvas.clientWidth;
            let height = this.drawingCanvas.clientHeight;
            this.zoomIn(width / 2, height / 2);
        });

        (<HTMLButtonElement>document.getElementById("zoomOutButton")).addEventListener('click', () => {
            let width = this.drawingCanvas.clientWidth;
            let height = this.drawingCanvas.clientHeight;
            this.zoomOut(width / 2, height / 2);
        });

        this.cursorCanvas.addEventListener('wheel', (ev) => {
            this.zoom(ev.offsetX, ev.offsetY, -ev.deltaY);
        });

        (<HTMLInputElement>document.getElementById("input")).addEventListener('keydown', (ev) => {
            if (ev.key == "Enter") {
                this.submitInput();
            }
        });
    }

    private prepareListeners() {
        // notifications
        this.connection.onNotification(this.ClientUpdateNotification, (params) => {
            let clientUpdate = params[0];
            let redraw = false;
            if (clientUpdate.Drawing !== undefined) {
                this.entityDrawing.FileName = clientUpdate.Drawing.FileName;
                this.entityDrawing.Lines = clientUpdate.Drawing.Lines;
                this.entityDrawing.Ellipses = clientUpdate.Drawing.Ellipses;
                var fileName = this.entityDrawing.FileName || "(Untitled)";
                var dirtyText = clientUpdate.IsDirty ? " *" : "";
                var title = `BCad [${fileName}]${dirtyText}`;
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
            if (clientUpdate.Transform !== undefined) {
                this.transform = clientUpdate.Transform;
                redraw = true;
            }
            if (clientUpdate.CursorState !== undefined) {
                this.cursorState = clientUpdate.CursorState;
                this.drawCursor();
            }
            if (clientUpdate.Prompt !== undefined) {
                document.getElementById("prompt").innerText = clientUpdate.Prompt;
            }
            if (clientUpdate.OutputLines !== undefined) {
                var output = <HTMLTextAreaElement>document.getElementById("outputConsole");
                var content = output.value;
                for (var i = 0; i < clientUpdate.OutputLines.length; i++) {
                    if (content.length > 0) {
                        content += "\n";
                    }
                    content = content + clientUpdate.OutputLines[i];
                }
                output.value = content;
                output.scrollTop = output.scrollHeight;
            }

            if (redraw) {
                this.redraw();
            }
        });

        // file system service
        this.connection.onRequest(new rpc.RequestType<void, string, void, void>('GetFileNameFromUserForOpen'), async () => {
            let result = await remote.dialog.showOpenDialog({
                filters: [
                    { name: 'DXF Files', extensions: ['dxf', 'dxb'] },
                    { name: 'IGES Files', extensions: ['igs', 'iges'] }
                ]
            });

            if (result.filePaths.length > 0) {
                return result.filePaths[0];
            }
            else {
                return null;
            }
        });

        this.connection.onUnhandledNotification((msg) => {
            alert('got unhandled notification ' + msg);
        });

        this.connection.onError((e) => console.log('rpc error: ' + e));
        this.connection.onClose(() => alert('rpc closing'));
    }

    private prepareCanvas() {
        this.twod = this.cursorCanvas.getContext("2d");
        this.gl = this.drawingCanvas.getContext("webgl");
        var gl = this.gl;

        var vertCode = `
            attribute vec3 vCoords;
            attribute vec3 vColor;

            uniform mat4 objectTransform;
            uniform mat4 worldTransform;
            varying vec3 fColor;

            void main(void) {
                gl_Position = worldTransform * objectTransform * vec4(vCoords, 1.0);
                fColor = vColor;
            }`;

        var vertShader = gl.createShader(gl.VERTEX_SHADER);
        gl.shaderSource(vertShader, vertCode);
        gl.compileShader(vertShader);

        var fragCode = `
            precision mediump float;
            varying vec3 fColor;
            void main(void) {
                gl_FragColor = vec4(fColor, 255.0) / 255.0;
            }`;
        var fragShader = gl.createShader(gl.FRAGMENT_SHADER);
        gl.shaderSource(fragShader, fragCode);
        gl.compileShader(fragShader);

        var program = gl.createProgram();
        gl.attachShader(program, vertShader);
        gl.attachShader(program, fragShader);
        gl.linkProgram(program);
        gl.useProgram(program);

        this.coordinatesLocation = gl.getAttribLocation(program, "vCoords");
        this.colorLocation = gl.getAttribLocation(program, "vColor");
        this.objectTransformLocation = gl.getUniformLocation(program, "objectTransform");
        this.worldTransformLocation = gl.getUniformLocation(program, "worldTransform");
    }

    private populateStaticVertices() {
        var verts = [];
        for (var n = 0; n <= 720; n++) {
            var x = Math.cos(n * Math.PI / 180.0);
            var y = Math.sin(n * Math.PI / 180.0);
            verts.push(x, y, 0.0);
        }
        var vertices = new Float32Array(verts);

        this.ellipseBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.ellipseBuffer);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private populateVertices(drawing: Drawing) {
        var verts = [];
        var cols = [];
        for (var i = 0; i < drawing.Lines.length; i++) {
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
        var vertices = new Float32Array(verts);
        var colors = new Uint8Array(cols);

        drawing.Vertices = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Vertices);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.STATIC_DRAW);

        drawing.Colors = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, drawing.Colors);
        this.gl.bufferData(this.gl.ARRAY_BUFFER, colors, this.gl.STATIC_DRAW);

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, null);
    }

    private submitInput() {
        var input = <HTMLInputElement>document.getElementById("input");
        var text = input.value;
        input.value = "";
        this.connection.sendNotification(this.SubmitIntputNotification, {value: text});
    }

    private redraw() {
        this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
        //gl.clearColor(0.3921, 0.5843, 0.9294, 1.0); // cornflower blue
        this.gl.clearColor(0.0, 0.0, 0.0, 1.0); // black
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
        for (var i = 0; i < drawing.Ellipses.length; i++) {
            var el = drawing.Ellipses[i];
            this.gl.uniformMatrix4fv(this.objectTransformLocation, false, el.Transform);
            var startAngle = Math.trunc(el.StartAngle);
            var endAngle = Math.trunc(el.EndAngle);

            this.gl.vertexAttrib3f(this.colorLocation, el.Color.R, el.Color.G, el.Color.B);
            this.gl.drawArrays(this.gl.LINE_STRIP, startAngle, endAngle - startAngle + 1); // + 1 to account for end angle
        }
    }

    private drawCursor() {
        var x = this.cursorPosition.x;
        var y = this.cursorPosition.y;
        var boxSize = 8; // TODO: from options
        var cursorSize = 60; // TODO: from options
        this.twod.clearRect(0, 0, this.twod.canvas.width, this.twod.canvas.height);
        this.twod.beginPath();
        this.twod.strokeStyle = "white"; // TODO: auto color or from options?

        // point cursor
        if (this.cursorState & CursorState.Point) {
            this.twod.moveTo(x - cursorSize / 2, y);
            this.twod.lineTo(x + cursorSize / 2, y);
            this.twod.moveTo(x, y - cursorSize / 2);
            this.twod.lineTo(x, y + cursorSize / 2);
        }

        // object cursor
        if (this.cursorState & CursorState.Object) {
            this.twod.moveTo(x - boxSize / 2, y - boxSize / 2);
            this.twod.lineTo(x + boxSize / 2, y - boxSize / 2);
            this.twod.lineTo(x + boxSize / 2, y + boxSize / 2);
            this.twod.lineTo(x - boxSize / 2, y + boxSize / 2);
            this.twod.lineTo(x - boxSize / 2, y - boxSize / 2);
        }

        this.twod.stroke();
    }

    async executeCommand(commandName: string) {
        var result = await this.connection.sendRequest(this.ExecuteCommandRequest, {command: commandName});
    }
}
