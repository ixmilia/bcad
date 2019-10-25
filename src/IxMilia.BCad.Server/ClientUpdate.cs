using System.Collections.Generic;
using IxMilia.BCad.Display;

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
        public string FileName { get; }
        public List<ClientLine> Lines { get; } = new List<ClientLine>();
        public List<ClientEllipse> Ellipses { get; } = new List<ClientEllipse>();

        public ClientDrawing(string fileName)
        {
            FileName = fileName;
        }
    }

    public class ClientUpdate
    {
        public bool IsDirty { get; set; }
        public double[] Transform { get; set; }
        public ClientDrawing Drawing { get; set; }
        public ClientDrawing RubberBandDrawing { get; set; }
        public CursorState? CursorState { get; set; }
        public string Prompt { get; set; }
        public string[] OutputLines { get; set; }
    }
}
