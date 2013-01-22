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
            Normal = DxfVector.ZAxis;
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

        internal static DxfPolyline PolylineFromBuffer(DxfCodePairBufferReader buffer)
        {
            var poly = new DxfPolyline();
            poly.SequenceEnd = null;
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!poly.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                        case 20:
                            Debug.Assert(pair.DoubleValue == 0.0); // dummy value
                            break;
                        case 30:
                            poly.Elevation = pair.DoubleValue;
                            break;
                        case 210:
                            poly.Normal.X = pair.DoubleValue;
                            break;
                        case 220:
                            poly.Normal.Y = pair.DoubleValue;
                            break;
                        case 230:
                            poly.Normal.Z = pair.DoubleValue;
                            break;
                    }
                }
            }

            // now read verticies
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    if (pair.StringValue == DxfEntity.SeqendType)
                    {
                        buffer.Advance();
                        var seq = DxfSeqend.SeqendFromBuffer(buffer);
                        poly.SequenceEnd = seq;
                        break;
                    }
                    else if (pair.StringValue == DxfEntity.VertexType)
                    {
                        buffer.Advance();
                        var vertex = DxfVertex.VertexFromBuffer(buffer);
                        poly.Vertices.Add(vertex);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return poly;
        }
    }
}
