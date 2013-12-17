using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Dxf.Entities;

namespace BCad.Dxf.Entities
{
    public enum DxfHorizontalTextJustification
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Aligned = 3,
        Middle = 4,
        Fit = 5
    }

    public enum DxfVerticalTextJustification
    {
        Baseline = 0,
        Bottom = 1,
        Middle = 2,
        Top = 3
    }

    public enum DxfPolylineCurvedAndSmoothSurfaceType
    {
        None = 0,
        QuadraticBSpline = 5,
        CubicBSpline = 6,
        Bezier = 8
    }

    public enum DxfImageClippingBoundaryType
    {
        Rectangular = 1,
        Polygonal = 2
    }

    public enum DxfLeaderPathType
    {
        StraightLineSegments = 0,
        Spline = 1
    }

    public enum DxfLeaderCreationAnnotationType
    {
        WithTextAnnotation = 0,
        WithToleranceAnnotation = 1,
        WithBlockReferenceAnnotation = 2,
        NoAnnotation = 3
    }

    public enum DxfLeaderHooklineDirection
    {
        OppositeFromHorizontalVector = 0,
        SameAsHorizontalVector = 1
    }

    public enum DxfOleObjectType
    {
        Link = 1,
        Embedded = 2,
        Static = 3
    }

    public enum DxfTileModeDescriptor
    {
        InTiledViewport = 0,
        InNonTiledViewport = 1
    }

    public abstract partial class DxfEntity
    {
        public abstract DxfEntityType EntityType { get; }

        protected virtual void AddTrailingCodePairs(List<DxfCodePair> pairs)
        {
        }

        protected virtual void PostParse()
        {
        }

        public IEnumerable<DxfCodePair> GetValuePairs()
        {
            var pairs = new List<DxfCodePair>();
            AddValuePairs(pairs);
            AddTrailingCodePairs(pairs);
            return pairs;
        }

        protected static bool BoolShort(short s)
        {
            return s != 0;
        }

        protected static short BoolShort(bool b)
        {
            return (short)(b ? 1 : 0);
        }

        private static short NotBoolShort(bool b)
        {
            return BoolShort(!b);
        }

        private static void SwallowEntity(DxfCodePairBufferReader buffer)
        {
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                    break;
                buffer.Advance();
            }
        }
    }

    public partial class DxfPolyline
    {
        public double Elevation
        {
            get { return Location.Z; }
            set { Location.Z = value; }
        }

        private List<DxfVertex> vertices = new List<DxfVertex>();
        private DxfSeqend seqend = new DxfSeqend();

        public List<DxfVertex> Vertices { get { return vertices; } }

        public DxfSeqend Seqend
        {
            get { return seqend; }
            set { seqend = value; }
        }

        protected override void AddTrailingCodePairs(List<DxfCodePair> pairs)
        {
            foreach (var vertex in Vertices)
            {
                pairs.AddRange(vertex.GetValuePairs());
            }

            if (Seqend != null)
            {
                pairs.AddRange(Seqend.GetValuePairs());
            }
        }
    }

    public partial class DxfLeader
    {
        private List<DxfPoint> vertices = new List<DxfPoint>();
        public List<DxfPoint> Vertices
        {
            get { return vertices; }
        }

        protected override void PostParse()
        {
            Debug.Assert((VertexCount == VerticesX.Count) && (VertexCount == VerticesY.Count) && (VertexCount == VerticesZ.Count));
            for (int i = 0; i < VertexCount; i++)
            {
                Vertices.Add(new DxfPoint(VerticesX[i], VerticesY[i], VerticesZ[i]));
            }

            VerticesX.Clear();
            VerticesY.Clear();
            VerticesZ.Clear();
        }
    }

    public partial class DxfImage
    {
        private List<DxfPoint> clippingVertices = new List<DxfPoint>();
        public List<DxfPoint> ClippingVertices
        {
            get { return clippingVertices; }
        }

        protected override void PostParse()
        {
            Debug.Assert((ClippingVertexCount == ClippingVerticesX.Count) && (ClippingVertexCount == ClippingVerticesY.Count));
            clippingVertices.AddRange(ClippingVerticesX.Zip(ClippingVerticesY, (x, y) => new DxfPoint(x, y, 0.0)));
            ClippingVerticesX.Clear();
            ClippingVerticesY.Clear();
        }
    }

    public partial class DxfInsert
    {
        private List<DxfAttribute> attributes = new List<DxfAttribute>();
        private DxfSeqend seqend = new DxfSeqend();

        public List<DxfAttribute> Attributes { get { return attributes; } }

        public DxfSeqend Seqend
        {
            get { return seqend; }
            set { seqend = value; }
        }

        protected override void AddTrailingCodePairs(List<DxfCodePair> pairs)
        {
            foreach (var attribute in Attributes)
            {
                pairs.AddRange(attribute.GetValuePairs());
            }

            if (Seqend != null)
            {
                pairs.AddRange(Seqend.GetValuePairs());
            }
        }
    }

    public partial class DxfLwPolyline
    {
        public class DxfLwPolylineVertex
        {
            public DxfPoint Location { get; set; }
            public double StartingWidth { get; set; }
            public double EndingWidth { get; set; }
            public double Bulge { get; set; }
        }

        private List<DxfLwPolylineVertex> vertices = new List<DxfLwPolylineVertex>();
        public List<DxfLwPolylineVertex> Vertices
        {
            get { return vertices; }
        }

        protected override void PostParse()
        {
            Debug.Assert((VertexCount == VertexCoordinateX.Count) && (VertexCount == VertexCoordinateY.Count));
            // TODO: how to read optional starting/ending width and bulge in this way?
            vertices.AddRange(VertexCoordinateX.Zip(VertexCoordinateY, (x, y) => new DxfLwPolylineVertex() { Location = new DxfPoint(x, y, 0.0) }));
        }
    }
}
