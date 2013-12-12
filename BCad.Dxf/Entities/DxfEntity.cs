using System;
using System.Collections.Generic;
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

    public abstract partial class DxfEntity
    {
        public abstract DxfEntityType EntityType { get; }

        protected virtual void AddTrailingCodePairs(List<DxfCodePair> pairs)
        {
        }

        public IEnumerable<DxfCodePair> GetValuePairs()
        {
            var pairs = new List<DxfCodePair>();
            AddValuePairs(pairs);
            AddTrailingCodePairs(pairs);
            return pairs;
        }

        private static bool BoolShort(short s)
        {
            return s != 0;
        }

        private static short BoolShort(bool b)
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
}
