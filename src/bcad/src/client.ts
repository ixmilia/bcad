import * as cp from 'child_process';
import * as os from 'os';
import * as path from 'path';
import * as rpc from 'vscode-jsonrpc';
import { remote } from 'electron';
import { Arguments } from './args';

interface Point3 {
    X: number;
    Y: number;
    Z: number;
}

export interface Color {
    A: number;
    R: number;
    G: number;
    B: number;
}

interface PointLocation {
    Location: Point3;
    Color: Color;
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

export interface ClientDrawing {
    CurrentLayer: string;
    Layers: string[];
    FileName: string;
    Points: PointLocation[];
    Lines: Line[];
    Ellipses: Ellipse[];
}

export enum CursorState {
    None = 0,
    Point = 1,
    Object = 2,
    Text = 4,
}

export interface Rect {
    Left: number,
    Top: number,
    Width: number,
    Height: number,
}

export enum SelectionMode {
    WholeEntity = 0,
    PartialEntity = 1,
}

export interface SelectionState {
    Rectangle: Rect,
    Mode: SelectionMode,
}

export interface ClientSettings {
    AutoColor: Color;
    BackgroundColor: Color;
    CursorSize: number;
    Debug: boolean;
    EntitySelectionRadius: number;
    HotPointColor: Color;
    SnapPointColor: Color;
    SnapPointSize: number;
    PointDisplaySize: number;
    TextCursorSize: number;
}

export enum SnapPointKind {
    None = 0x00,
    Center = 0x01,
    EndPoint = 0x02,
    MidPoint = 0x04,
    Quadrant = 0x08,
    Focus = 0x10,
}

export interface ClientTransform {
    Transform?: number[],
    DisplayXTransform: number,
    DisplayYTransform: number,
}

export interface ClientUpdate {
    IsDirty: boolean;
    Transform?: ClientTransform;
    Drawing?: ClientDrawing;
    RubberBandDrawing?: ClientDrawing;
    TransformedSnapPoint?: {WorldPoint: Point3, ControlPoint: Point3, Kind: SnapPointKind};
    CursorState?: CursorState;
    HasSelectionStateUpdate: boolean;
    SelectionState?: SelectionState;
    Settings?: ClientSettings;
    Prompt?: string;
    OutputLines?: string[];
}

export enum MouseButton {
    Left,
    Middle,
    Right,
}

export class Client {
    private arguments: Arguments;
    private connection: rpc.MessageConnection;
    private clientUpdateNotifications: {(clientUpdate: ClientUpdate): void}[] = [];
    private currentDialogHandler: {(dialogId: string, dialogOptions: object): Promise<any>} = async (_dialogId, _dialogOptions) => {};

    // client notifications
    private ClientUpdateNotification: rpc.NotificationType<ClientUpdate[], void>;
    private ShowDialogRequest: rpc.RequestType1<{id: string, parameter: object}[], object, void, void>;

    private ChangeCurrentLayerNotification: rpc.NotificationType<{currentLayer: string}, void>;
    private CancelNotification: rpc.NotificationType<void, void>;
    private MouseDownNotification: rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>;
    private MouseUpNotification: rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>;
    private MouseMoveNotification: rpc.NotificationType<{cursorX: Number, cursorY: Number}, void>;
    private PanNotification: rpc.NotificationType<{dx: Number, dy: Number}, void>;
    private ReadyNotification: rpc.NotificationType<{width: Number, height: Number}, void>;
    private ResizeNotification: rpc.NotificationType<{width: Number, height: Number}, void>;
    private SubmitIntputNotification: rpc.NotificationType<{value: string}, void>;
    private ZoomNotification: rpc.NotificationType<{cursorX: Number, cursorY: Number, delta: Number}, void>;
    private ExecuteCommandRequest: rpc.RequestType1<{command: String}, boolean, void, void>;

    constructor(args: Arguments) {
        this.arguments = args;
        this.ClientUpdateNotification = new rpc.NotificationType<ClientUpdate[], void>('ClientUpdate');
        this.ShowDialogRequest = new rpc.RequestType1<{id: string, parameter: object}[], object, void, void>('ShowDialog');

        this.ChangeCurrentLayerNotification = new rpc.NotificationType<{currentLayer: string}, void>('ChangeCurrentLayer');
        this.CancelNotification = new rpc.NotificationType<void, void>('Cancel');
        this.MouseDownNotification = new rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>('MouseDown');
        this.MouseUpNotification = new rpc.NotificationType<{button: MouseButton, cursorX: Number, cursorY: Number}, void>('MouseUp');
        this.MouseMoveNotification = new rpc.NotificationType<{cursorX: Number, cursorY: Number}, void>('MouseMove');
        this.PanNotification = new rpc.NotificationType<{dx: Number, dy: Number}, void>('Pan');
        this.ReadyNotification = new rpc.NotificationType<{width: Number, height: Number}, void>('Ready');
        this.ResizeNotification = new rpc.NotificationType<{width: Number, height: Number}, void>('Resize');
        this.SubmitIntputNotification = new rpc.NotificationType<{value: string}, void>('SubmitInput');
        this.ZoomNotification = new rpc.NotificationType<{cursorX: Number, cursorY: Number, delta: Number}, void>('Zoom');
        this.ExecuteCommandRequest = new rpc.RequestType1<{command: String}, boolean, void, void>('ExecuteCommand');
    }

    start() {
        this.prepareConnection();
        this.prepareListeners();
        this.connection.listen();
    }

    registerDialogHandler(dialogHandler: {(dialogId: string, dialogOptions: object): Promise<object>}) {
        this.currentDialogHandler = dialogHandler;
    }

    subscribeToClientUpdates(clientUpdateNotification: {(clientUpdate: ClientUpdate): void}) {
        this.clientUpdateNotifications.push(clientUpdateNotification);
    }

    changeCurrentLayer(currentLayer: string) {
        this.connection.sendNotification(this.ChangeCurrentLayerNotification, {currentLayer: currentLayer});
    }

    cancel() {
        this.connection.sendNotification(this.CancelNotification);
    }

    mouseDown(button: MouseButton, cursorX: number, cursorY: number) {
        this.connection.sendNotification(this.MouseDownNotification, {button: button, cursorX: cursorX, cursorY: cursorY});
    }

    mouseUp(button: MouseButton, cursorX: number, cursorY: number) {
        this.connection.sendNotification(this.MouseUpNotification, {button: button, cursorX: cursorX, cursorY: cursorY});
    }

    mouseMove(cursorX: number, cursorY: number) {
        this.connection.sendNotification(this.MouseMoveNotification, {cursorX: cursorX, cursorY: cursorY});
    }

    pan(dx: number, dy: number) {
        this.connection.sendNotification(this.PanNotification, {dx: dx, dy: dy});
    }

    ready(width: number, height: number) {
        this.connection.sendNotification(this.ReadyNotification, {width: width, height: height});
    }

    resize(width: number, height: number) {
        this.connection.sendNotification(this.ResizeNotification, {width: width, height: height});
    }

    private prepareConnection() {
        let serverAssembly = os.platform() == "win32"
            ? "IxMilia.BCad.Server.exe"
            : "IxMilia.BCad.Server";
        let serverSubPath = this.arguments.isLocal
            ? '../../../artifacts/bin/IxMilia.BCad.Server/Debug/netcoreapp3.1'
            : '../../publish';
        let serverPath = path.join(__dirname, serverSubPath, serverAssembly);
        let childProcess = cp.spawn(serverPath);
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

    zoom(cursorX: Number, cursorY: Number, delta: Number) {
        this.connection.sendNotification(this.ZoomNotification, {cursorX: cursorX, cursorY: cursorY, delta: delta});
    }

    zoomIn(cursorX: Number, cursorY: Number) {
        this.zoom(cursorX, cursorY, 1);
    }

    zoomOut(cursorX: Number, cursorY: Number) {
        this.zoom(cursorX, cursorY, -1);
    }

    submitInput(value: string) {
        this.connection.sendNotification(this.SubmitIntputNotification, {value: value});
    }

    async executeCommand(commandName: string): Promise<boolean> {
        let result = await this.connection.sendRequest(this.ExecuteCommandRequest, {command: commandName});
        return result;
    }

    private prepareListeners() {
        // notifications
        this.connection.onNotification(this.ClientUpdateNotification, (params) => {
            let clientUpdate = params[0];
            for (let clientUpdateNotification of this.clientUpdateNotifications) {
                clientUpdateNotification(clientUpdate);
            }
        });

        // dialog service
        this.connection.onRequest(this.ShowDialogRequest, (params) => {
            let parameter = params[0];
            return this.currentDialogHandler(parameter.id, parameter.parameter).then(
                result => {
                    // dialog was completed
                    return result;
                },
                _reason => {
                    // dialog was cancelled
                }
            );
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
        this.connection.onRequest(new rpc.RequestType<void, string, void, void>('GetFileNameFromUserForSave'), async () => {
            let result = await remote.dialog.showSaveDialog({
                filters: [
                    { name: 'DXF Files', extensions: ['dxf', 'dxb'] },
                    { name: 'IGES Files', extensions: ['igs', 'iges'] }
                ]
            });

            return result.filePath;
        })

        this.connection.onUnhandledNotification((msg) => {
            alert('got unhandled notification ' + msg);
        });

        this.connection.onError((e) => console.log('rpc error: ' + e));
        this.connection.onClose(() => alert('rpc closing'));
    }
}
