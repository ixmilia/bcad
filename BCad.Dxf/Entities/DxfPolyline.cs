using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Dxf.Sections;
using System.Diagnostics;

namespace BCad.Dxf.Entities
{
    public class DxfPolyline : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Polyline; } }

        public override string SubclassMarker { get { return "AcDb2dPolyline"; } }

        public double Elevation { get; set; }

        public DxfVector Normal { get; set; }

        public List<DxfVertex> Vertices { get; private set; }

        public DxfSeqend SequenceEnd { get; private set; }

        public DxfPolyline()
        {
            Vertices = new List<DxfVertex>();
            SequenceEnd = new DxfSeqend();
        }

        protected override IEnumerable<DxfCodePair> GetEntitySpecificPairs()
        {
            yield return new DxfCodePair(10, 0.0);
            yield return new DxfCodePair(20, 0.0);
            yield return new DxfCodePair(30, Elevation);
            if (Normal != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }

            foreach (var vertex in Vertices)
            {
                foreach (var pair in vertex.ValuePairs)
                {
                    yield return pair;
                }
            }

            foreach (var pair in SequenceEnd.ValuePairs)
            {
                yield return pair;
            }
        }

        public static DxfPolyline FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            var poly = new DxfPolyline();
            poly.PopulateDefaultAndCommonValues(pairs);
            var subEntities = DxfSection.SplitAtZero(pairs);
            var first = subEntities.First();
            foreach (var pair in first)
            {
                switch (pair.Code)
                {
                    case 10:
                        //Debug.Assert(pair.DoubleValue == 0.0);
                        break;
                    case 20:
                        //Debug.Assert(pair.DoubleValue == 0.0);
                        break;
                    case 30:
                        poly.Elevation = pair.DoubleValue;
                        break;
                }
            }

            foreach (var vertex in subEntities)
            {
                switch (vertex.First().StringValue)
                {
                    case VertexType:
                        var vert = DxfVertex.FromPairs(vertex);
                        poly.Vertices.Add(vert);
                        break;
                    default:
                        // probably SEQEND
                        break;
                }
            }

            return poly;
        }
    }
}
