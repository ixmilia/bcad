using System.Collections.Generic;

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

        public static ClientPoint FromPoint(Point p)
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

    public class ClientDrawing
    {
        public List<ClientLine> Lines { get; set; }
    }

    public class ClientUpdate
    {
        public double[] Transform { get; set; }
        public ClientDrawing Drawing { get; set; }
    }
}
