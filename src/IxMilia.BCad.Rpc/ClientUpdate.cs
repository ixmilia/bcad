using System;
using System.Collections.Generic;
using System.Linq;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Display;
using IxMilia.BCad.Extensions;
using IxMilia.BCad.Plotting;
using IxMilia.BCad.Settings;
using IxMilia.Pdf;

namespace IxMilia.BCad.Rpc
{
    public struct ClientPoint
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public ClientPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        internal ClientPoint(Point p)
            : this(p.X, p.Y, p.Z)
        {
        }

        public Point ToPoint()
        {
            return new Point(X, Y, Z);
        }

        public static implicit operator ClientPoint(Point p)
        {
            return new ClientPoint(p.X, p.Y, p.Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    public struct ClientRectangle
    {
        public ClientPoint TopLeft { get; }
        public ClientPoint BottomRight { get; }

        public ClientRectangle(ClientPoint topLeft, ClientPoint bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public override string ToString()
        {
            return $"{{{TopLeft} - {BottomRight}}}";
        }
    }

    public enum PlotColorType
    {
        Exact,
        Contrast,
        Black,
    }

    public struct ClientPlotSettings
    {
        public string PlotType { get; }
        public ClientRectangle Viewport { get; }
        public string ScaleA { get; }
        public string ScaleB { get; }
        public PlotScalingType ScalingType { get; }
        public PlotViewPortType ViewPortType { get; }
        public PlotColorType ColorType { get; }
        public double Width { get; }
        public double Height { get; }
        public double PreviewMaxSize { get; }

        public ClientPlotSettings(string plotType, ClientRectangle viewport, string scaleA, string scaleB, PlotScalingType scalingType, PlotViewPortType viewPortType, PlotColorType colorType, double width, double height, double previewMaxSize)
        {
            PlotType = plotType;
            Viewport = viewport;
            ScaleA = scaleA;
            ScaleB = scaleB;
            ScalingType = scalingType;
            ViewPortType = viewPortType;
            ColorType = colorType;
            Width = width;
            Height = height;
            PreviewMaxSize = previewMaxSize;
        }

        public (double scaleA, double scaleB) GetUnitAdjustedScale(DrawingSettings drawingSettings)
        {
            var drawingUnits = drawingSettings.UnitFormat == UnitFormat.Architectural ? PdfMeasurementType.Inch : PdfMeasurementType.Mm;
            var scaleAValue = DrawingSettings.TryParseUnits(ScaleA, out var parsedScaleA) ? parsedScaleA : 1.0;
            var scaleBValue = DrawingSettings.TryParseUnits(ScaleB, out var parsedScaleB) ? parsedScaleB : 1.0;
            var scaleAMeasurement = new PdfMeasurement(scaleAValue, drawingUnits);
            return (scaleAMeasurement.ConvertTo(PdfMeasurementType.Point).RawValue, scaleBValue);
        }
    }

    public struct ClientDownload
    {
        public string Filename { get; }
        public string MimeType { get; }
        public string Data { get; }

        public ClientDownload(string filename, string mimeType, string data)
        {
            Filename = filename;
            MimeType = mimeType;
            Data = data;
        }
    }

    public struct ClientPointLocation
    {
        public ClientPoint Location { get; }
        public CadColor? Color { get; }

        public ClientPointLocation(ClientPoint location, CadColor? color)
        {
            Location = location;
            Color = color;
        }
    }

    public struct ClientLine
    {
        public ClientPoint P1 { get; }
        public ClientPoint P2 { get; }
        public CadColor? Color { get; }

        public ClientLine(ClientPoint p1, ClientPoint p2, CadColor? color)
        {
            P1 = p1;
            P2 = p2;
            Color = color;
        }
    }

    public struct ClientEllipse
    {
        public double StartAngle { get; }
        public double EndAngle { get; }
        public double[] Transform { get; }
        public CadColor? Color { get; }

        public ClientEllipse(double startAngle, double endAngle, double[] transform, CadColor? color)
        {
            StartAngle = startAngle;
            EndAngle = endAngle;
            Transform = transform;
            Color = color;
        }
    }

    public struct ClientText
    {
        public string Text { get; }
        public ClientPoint Location { get; }
        public double Height { get; }
        public double RotationAngle { get; }
        public CadColor? Color { get; }

        public ClientText(string text, ClientPoint location, double height, double rotationAngle, CadColor? color)
        {
            Text = text;
            Location = location;
            Height = height;
            RotationAngle = rotationAngle;
            Color = color;
        }
    }

    public class ClientDrawing
    {
        public string CurrentLayer { get; set; }
        public List<string> Layers { get; } = new List<string>();
        public string FileName { get; }
        public List<ClientPointLocation> Points { get; } = new List<ClientPointLocation>();
        public List<ClientLine> Lines { get; } = new List<ClientLine>();
        public List<ClientEllipse> Ellipses { get; } = new List<ClientEllipse>();
        public List<ClientText> Text { get; } = new List<ClientText>();

        public ClientDrawing(string fileName)
        {
            FileName = fileName;
        }
    }

    public class ClientPropertyPaneValue : IEquatable<ClientPropertyPaneValue>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public List<string> AllowedValues { get; set; }
        public bool IsUnrepresentable { get; set; }

        public ClientPropertyPaneValue(string name, string displayName, string value, IEnumerable<string> allowedValues = null, bool isUnrepresentable = false)
        {
            Name = name;
            DisplayName = displayName;
            Value = value;
            AllowedValues = allowedValues?.ToList();
            IsUnrepresentable = isUnrepresentable;
        }

        public static bool operator==(ClientPropertyPaneValue v1, ClientPropertyPaneValue v2)
        {
            if (v1 is null && v2 is null)
            {
                return true;
            }

            if (v1 is null || v2 is null)
            {
                return false;
            }

            if (ReferenceEquals(v1, v2))
            {
                return true;
            }

            return v1.Name == v2.Name
                && v1.DisplayName == v2.DisplayName
                && v1.Value == v2.Value
                && ((v1.AllowedValues is null && v2.AllowedValues is null) ||
                    (v1.AllowedValues is not null && v2.AllowedValues is not null && v1.AllowedValues.SequenceEqual(v2.AllowedValues)))
                && v1.IsUnrepresentable == v2.IsUnrepresentable;
        }

        public static bool operator !=(ClientPropertyPaneValue v1, ClientPropertyPaneValue v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            if (obj is ClientPropertyPaneValue other)
            {
                return this == other;
            }

            return false;
        }

        public bool Equals(ClientPropertyPaneValue other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, DisplayName, Value, AllowedValues, IsUnrepresentable);
        }
    }

    public class ClientPropertyPane
    {
        public List<ClientPropertyPaneValue> Values { get; set; }

        public ClientPropertyPane(IEnumerable<ClientPropertyPaneValue> values = null)
        {
            Values = (values ?? Enumerable.Empty<ClientPropertyPaneValue>()).ToList();
        }
    }

    public class ClientSettings
    {
        public CadColor AutoColor => BackgroundColor.GetAutoContrastingColor();
        public CadColor BackgroundColor { get; }
        public int CursorSize { get; }
        public bool Debug { get; }
        public int DrawingPrecision { get; }
        public UnitFormat DrawingUnits { get; }
        public double EntitySelectionRadius { get; }
        public CadColor HotPointColor { get; }
        public double HotPointSize { get; }
        public double[] SnapAngles { get; }
        public CadColor SnapPointColor { get; }
        public double SnapPointSize { get; }
        public double PointDisplaySize { get; }
        public int TextCursorSize { get; }
        public string Theme { get; }
        public CommandShortcut[] CommandShortcuts { get; }

        public ClientSettings(IWorkspace workspace)
        {
            BackgroundColor = workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.BackgroundColor);
            CursorSize = workspace.SettingsService.GetValue<int>(DisplaySettingsNames.CursorSize);
            Debug = workspace.SettingsService.GetValue<bool>(DefaultSettingsNames.Debug);
            DrawingPrecision = workspace.SettingsService.GetValue<int>(DefaultSettingsNames.DrawingPrecision);
            DrawingUnits = workspace.SettingsService.GetValue<UnitFormat>(DefaultSettingsNames.DrawingUnits);
            EntitySelectionRadius = workspace.SettingsService.GetValue<double>(DisplaySettingsNames.EntitySelectionRadius);
            HotPointColor = workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.HotPointColor);
            HotPointSize = workspace.SettingsService.GetValue<double>(DisplaySettingsNames.HotPointSize);
            SnapAngles = workspace.SettingsService.GetValue<double[]>(DisplaySettingsNames.SnapAngles);
            SnapPointColor = workspace.SettingsService.GetValue<CadColor>(DisplaySettingsNames.SnapPointColor);
            SnapPointSize = workspace.SettingsService.GetValue<double>(DisplaySettingsNames.SnapPointSize);
            PointDisplaySize = workspace.SettingsService.GetValue<double>(DisplaySettingsNames.PointDisplaySize);
            TextCursorSize = workspace.SettingsService.GetValue<int>(DisplaySettingsNames.TextCursorSize);
            Theme = workspace.SettingsService.GetValue<string>(DisplaySettingsNames.Theme);
            CommandShortcuts = workspace.Commands.Where(c => c.Key != Key.None).Select(c => new CommandShortcut(c.Name, c.Modifier, c.Key)).ToArray();
        }
    }

    public class ClientTransform
    {
        public double[] Transform { get; }
        public double[] CanvasTransform { get; }
        public double DisplayXTransform { get; }
        public double DisplayYTransform { get; }

        public ClientTransform(double[] transform, double[] canvasTransform, double displayXTransform, double displayYTransform)
        {
            Transform = transform;
            CanvasTransform = canvasTransform;
            DisplayXTransform = displayXTransform;
            DisplayYTransform = displayYTransform;
        }
    }

    public class ClientUpdate
    {
        private bool hasSelectionStateUpdate;
        private SelectionState? selectionState;

        public bool IsDirty { get; set; }
        public ClientTransform Transform { get; set; }
        public ClientDrawing Drawing { get; set; }
        public ClientDrawing SelectedEntitiesDrawing { get; set; }
        public ClientDrawing RubberBandDrawing { get; set; }
        public ClientPropertyPane PropertyPane { get; set; }
        public TransformedSnapPoint? TransformedSnapPoint { get; set; }
        public CursorState? CursorState { get; set; }
        public ClientPoint[] HotPoints { get; set; }
        public bool HasSelectionStateUpdate => hasSelectionStateUpdate;
        public SelectionState? SelectionState
        {
            get => selectionState;
            set
            {
                selectionState = value;
                hasSelectionStateUpdate = true;
            }
        }
        public ClientSettings Settings { get; set; }
        public string Prompt { get; set; }
        public string[] OutputLines { get; set; }
    }

    public class ClientLayer
    {
        public string Name { get; }
        public string Color { get; }
        public bool IsVisible { get; }

        public ClientLayer(Layer layer)
        {
            Name = layer.Name;
            Color = layer.Color.HasValue
                ? layer.Color.GetValueOrDefault().ToRGBString()
                : string.Empty;
            IsVisible = layer.IsVisible;
        }
    }

    public class ClientLayerParameters
    {
        public List<ClientLayer> Layers { get; } = new List<ClientLayer>();

        public ClientLayerParameters(LayerDialogParameters layerDialogParameters)
        {
            foreach (var layer in layerDialogParameters.Layers)
            {
                Layers.Add(new ClientLayer(layer));
            }
        }
    }

    public class ClientChangedLayer
    {
        public string OldLayerName { get; set; }
        public string NewLayerName { get; set; }
        public string Color { get; set; }
        public bool IsVisible { get; set; }
    }

    public class ClientLayerResult
    {
        public List<ClientChangedLayer> ChangedLayers { get; } = new List<ClientChangedLayer>();

        public LayerDialogResult ToDialogResult()
        {
            var result = new LayerDialogResult();
            foreach (var changedLayer in ChangedLayers)
            {
                result.Layers.Add(new LayerDialogResult.LayerResult()
                {
                    OldLayerName = changedLayer.OldLayerName,
                    NewLayerName = changedLayer.NewLayerName,
                    Color = string.IsNullOrEmpty(changedLayer.Color)
                        ? new CadColor?()
                        : CadColor.Parse(changedLayer.Color),
                    IsVisible = changedLayer.IsVisible,
                });
            }

            return result;
        }
    }
}
