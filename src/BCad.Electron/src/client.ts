import * as cp from 'child_process';
import * as rpc from 'vscode-jsonrpc';
import { remote } from 'electron';

interface Point3 {
    X: number;
    Y: number;
    Z: number;
}

interface Line {
    P1: Point3;
    P2: Point3;
}

interface Drawing {
    Lines: Line[];
}

interface ClientUpdate {
    Transform?: number[];
    Drawing?: Drawing;
}

enum DisplayUpdate {
    ZoomIn,
    ZoomOut,
    PanLeft,
    PanRight,
    PanUp,
    PanDown,
}

interface ServerUpdate {
    DisplayUpdate?: DisplayUpdate;
    CursorLocation?: {X: number, Y: number};
}

export class Client {
    private connection: rpc.MessageConnection;
    private outputPane: HTMLDivElement;
    private canvas: HTMLCanvasElement;
    private gl: WebGLRenderingContext;
    private vertices: Float32Array;
    private drawing: Drawing;
    private transform: number[];
    private transformLocation: WebGLUniformLocation;

    // client notifications
    private ServerUpdateRequest: rpc.NotificationType1<ServerUpdate, void>;

    constructor() {
        this.canvas = <HTMLCanvasElement> document.getElementById('canvas');
        this.outputPane = <HTMLDivElement> document.getElementById('output-pane');
        this.ServerUpdateRequest = new rpc.NotificationType1<ServerUpdate, void>('ServerUpdate');
        this.prepareConnection();
        this.prepareEvents();
        this.prepareListeners();
    }

    start() {
        this.drawing = {Lines: []};
        this.transform = [
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        ];
        this.populateVertices();
        this.predraw();
        this.redraw();
        this.connection.listen();
        this.connection.sendNotification(new rpc.NotificationType0('Ready'));
    }

    private prepareConnection() {
        let childProcess = cp.spawn('dotnet.exe', [__dirname + '/../bin/IxMilia.BCad.Server.dll']);
        childProcess.on('exit', (code: number, _signal: string) => {
            alert('process exited with ' + code);
        });
        console.log('server process ' + childProcess.pid);
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

    private prepareEvents() {
        this.outputPane.addEventListener('mousedown', async (ev) => {
            //
        });
        this.outputPane.addEventListener('mouseup', (ev) => {
            //
        });
        this.outputPane.addEventListener('mousemove', async (ev) => {
            //
        });
        this.outputPane.addEventListener('resize', () => {
            // TODO:
        });
        var elements = [
            {id: "panLeftButton", action: DisplayUpdate.PanLeft},
            {id: "panRightButton", action: DisplayUpdate.PanRight},
            {id: "panUpButton", action: DisplayUpdate.PanUp},
            {id: "panDownButton", action: DisplayUpdate.PanDown}
        ];
        for (var i = 0; i < elements.length; i++) {
            var element = elements[i];
            (<HTMLButtonElement>document.getElementById(element.id)).addEventListener('click', () => {
                this.connection.sendNotification(this.ServerUpdateRequest, {
                    DisplayUpdate: element.action,
                });
            });
        }

        this.canvas.addEventListener('wheel', (ev) => {
            var zoomType = ev.deltaY < 0 ? DisplayUpdate.ZoomIn : DisplayUpdate.ZoomOut;
            this.connection.sendNotification(this.ServerUpdateRequest, {
                DisplayUpdate: zoomType,
                CursorLocation: {X: ev.offsetX, Y: ev.offsetY}
            });
        });
    }

    private prepareListeners() {
        // notifications
        this.connection.onNotification(new rpc.NotificationType1<ClientUpdate, void>('ClientUpdate'), (clientUpdate) => {
            let redraw = false;
            alert('got client update');
            if (clientUpdate.Drawing !== undefined) {
                this.drawing = clientUpdate.Drawing;
                this.populateVertices();
                redraw = true;
            }
            if (clientUpdate.Transform !== undefined) {
                this.transform = clientUpdate.Transform;
                redraw = true;
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
    }

    private populateVertices() {
        var verts = [];
        for (var i = 0; i < this.drawing.Lines.length; i++) {
            verts.push(this.drawing.Lines[i].P1.X);
            verts.push(this.drawing.Lines[i].P1.Y);
            verts.push(this.drawing.Lines[i].P1.Z);
            verts.push(this.drawing.Lines[i].P2.X);
            verts.push(this.drawing.Lines[i].P2.Y);
            verts.push(this.drawing.Lines[i].P2.Z);
        }
        this.vertices = new Float32Array(verts);
    }

    private predraw() {
        // copypasta begin
        this.gl = this.canvas.getContext("webgl");
        var gl = this.gl;

        var vertex_buffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, vertex_buffer);
        gl.bufferData(gl.ARRAY_BUFFER, this.vertices, gl.STATIC_DRAW);
        gl.bindBuffer(gl.ARRAY_BUFFER, null);

        var vertCode =
            "attribute vec3 coordinates;\n" +
            "uniform mat4 transform;\n" +
            "void main(void) {\n" +
            "    gl_Position = transform * vec4(coordinates, 1.0);\n" +
            "}\n";

        var vertShader = gl.createShader(gl.VERTEX_SHADER);
        gl.shaderSource(vertShader, vertCode);
        gl.compileShader(vertShader);

        var fragCode =
            "void main(void) {\n" +
            "    gl_FragColor = vec4(0.0, 0.0, 0.0, 0.1);\n" +
            "}\n";
        var fragShader = gl.createShader(gl.FRAGMENT_SHADER);
        gl.shaderSource(fragShader, fragCode);
        gl.compileShader(fragShader);

        var program = gl.createProgram();
        gl.attachShader(program, vertShader);
        gl.attachShader(program, fragShader);
        gl.linkProgram(program);
        gl.useProgram(program);

        gl.bindBuffer(gl.ARRAY_BUFFER, vertex_buffer);

        var coordLocation = gl.getAttribLocation(program, "coordinates");
        this.transformLocation = gl.getUniformLocation(program, "transform");
        gl.vertexAttribPointer(coordLocation, 3, gl.FLOAT, false, 0, 0);
        gl.enableVertexAttribArray(coordLocation);

        // copypasta end
    }

    private redraw() {
        this.gl.viewport(0, 0, this.gl.canvas.width, this.gl.canvas.height);
        //gl.clearColor(0.3921, 0.5843, 0.9294, 1.0); // cornflower blue
        this.gl.clearColor(0.0, 0.0, 0.0, 1.0); // black
        this.gl.clear(this.gl.COLOR_BUFFER_BIT | this.gl.DEPTH_BUFFER_BIT);
        this.gl.uniformMatrix4fv(this.transformLocation, false, this.transform);
        this.gl.drawArrays(this.gl.LINES, 0, this.vertices.length / 3);
    }
}
