using System.Collections.Generic;
using IxMilia.BCad.Dialogs;
using IxMilia.BCad.Display;
using IxMilia.BCad.Extensions;

namespace IxMilia.BCad.Server
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

        public static implicit operator ClientPoint(Point p)
        {
            return new ClientPoint(p.X, p.Y, p.Z);
        }
    }

    public struct ClientLine
    {
        public ClientPoint P1 { get; }
        public ClientPoint P2 { get; }
        public CadColor Color { get; }

        public ClientLine(ClientPoint p1, ClientPoint p2, CadColor color)
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
        public CadColor Color { get; }

        public ClientEllipse(double startAngle, double endAngle, double[] transform, CadColor color)
        {
            StartAngle = startAngle;
            EndAngle = endAngle;
            Transform = transform;
            Color = color;
        }
    }

    public class ClientDrawing
    {
        public string CurrentLayer { get; set; }
        public List<string> Layers { get; } = new List<string>();
        public string FileName { get; }
        public List<ClientLine> Lines { get; } = new List<ClientLine>();
        public List<ClientEllipse> Ellipses { get; } = new List<ClientEllipse>();

        public ClientDrawing(string fileName)
        {
            FileName = fileName;
        }
    }

    public class ClientSettings
    {
        public CadColor AutoColor => BackgroundColor.GetAutoContrastingColor();
        public CadColor BackgroundColor { get; set; }
        public int CursorSize { get; set; }
        public bool Debug { get; set; }
        public double EntitySelectionRadius { get; set; }
        public CadColor HotPointColor { get; set; }
        public CadColor SnapPointColor { get; set; }
        public double SnapPointSize { get; set; }
        public int TextCursorSize { get; set; }
    }

    public class ClientUpdate
    {
        private bool hasSelectionStateUpdate;
        private SelectionState? selectionState;

        public bool IsDirty { get; set; }
        public double[] Transform { get; set; }
        public ClientDrawing Drawing { get; set; }
        public ClientDrawing RubberBandDrawing { get; set; }
        public TransformedSnapPoint? TransformedSnapPoint { get; set; }
        public CursorState? CursorState { get; set; }
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
