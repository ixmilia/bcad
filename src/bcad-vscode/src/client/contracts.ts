export interface Point3 {
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
    CurrentLayer?: string;
    Layers: string[];
    FileName?: string;
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
    HotPointSize: number;
    SnapAngles: number[];
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
    HotPoints?: Point3[];
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

export interface DialogOptions {
    dialogId: string;
    args: Array<any>;
}
