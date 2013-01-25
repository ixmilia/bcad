using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Entities
{
    public class DxfVertex : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Vertex; } }

        public override string SubclassMarker { get { return "AcDbVertex"; } }

        public DxfPoint Location { get; set; }

        public double StartingWidth { get; set; }

        public double EndingWidth { get; set; }

        public double Bulge { get; set; }

        public double CurveFitTangentDirection { get; set; }

        public short PolyfaceMeshVertexIndex1 { get; set; }

        public short PolyfaceMeshVertexIndex2 { get; set; }

        public short PolyfaceMeshVertexIndex3 { get; set; }

        public short PolyfaceMeshVertexIndex4 { get; set; }

        public bool IsExtraCreatedByCurveFit
        {
            get { return GetBit(vertexFlags, 1); }
            set { vertexFlags = SetBit(vertexFlags, 1, value); }
        }
        public bool IsCurveFitTangentDefined
        {
            get { return GetBit(vertexFlags, 2); }
            set { vertexFlags = SetBit(vertexFlags, 2, value); }
        }

        public bool IsSplineVertexCreatedBySplineFitting
        {
            get { return GetBit(vertexFlags, 4); }
            set { vertexFlags = SetBit(vertexFlags, 4, value); }
        }

        public bool IsSplineFrameControlPoint
        {
            get { return GetBit(vertexFlags, 5); }
            set { vertexFlags = SetBit(vertexFlags, 5, value); }
        }

        public bool Is3DPolylineVertex
        {
            get { return GetBit(vertexFlags, 6); }
            set { vertexFlags = SetBit(vertexFlags, 6, value); }
        }

        public bool Is3DPolygonMesh
        {
            get { return GetBit(vertexFlags, 7); }
            set { vertexFlags = SetBit(vertexFlags, 7, value); }
        }

        public bool IsPolyfaceMeshVertex
        {
            get { return GetBit(vertexFlags, 8); }
            set { vertexFlags = SetBit(vertexFlags, 8, value); }
        }

        private int vertexFlags = 0;

        public DxfVertex()
            : this(DxfPoint.Origin)
        {
        }

        public DxfVertex(DxfPoint location)
        {
            this.Location = location;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            yield return new DxfCodePair(0, DxfEntity.VertexType);
            yield return new DxfCodePair(10, Location.X);
            yield return new DxfCodePair(20, Location.Y);
            yield return new DxfCodePair(30, Location.Z);
            if (StartingWidth != 0.0)
                yield return new DxfCodePair(40, StartingWidth);
            if (EndingWidth != 0.0)
                yield return new DxfCodePair(41, EndingWidth);
            if (Bulge != 0.0)
                yield return new DxfCodePair(42, Bulge);
            if (CurveFitTangentDirection != 0.0)
                yield return new DxfCodePair(50, CurveFitTangentDirection);
            if (vertexFlags != 0)
                yield return new DxfCodePair(70, (short)vertexFlags);
            if (PolyfaceMeshVertexIndex1 != 0.0)
                yield return new DxfCodePair(71, PolyfaceMeshVertexIndex1);
            if (PolyfaceMeshVertexIndex2 != 0.0)
                yield return new DxfCodePair(72, PolyfaceMeshVertexIndex2);
            if (PolyfaceMeshVertexIndex3 != 0.0)
                yield return new DxfCodePair(73, PolyfaceMeshVertexIndex3);
            if (PolyfaceMeshVertexIndex4 != 0.0)
                yield return new DxfCodePair(74, PolyfaceMeshVertexIndex4);
        }

        internal static DxfVertex VertexFromBuffer(DxfCodePairBufferReader buffer)
        {
            var vertex = new DxfVertex();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                if (!vertex.TrySetSharedCode(pair))
                {
                    switch (pair.Code)
                    {
                        case 10:
                            vertex.Location.X = pair.DoubleValue;
                            break;
                        case 20:
                            vertex.Location.Y = pair.DoubleValue;
                            break;
                        case 30:
                            vertex.Location.Z = pair.DoubleValue;
                            break;
                        case 40:
                            vertex.StartingWidth = pair.DoubleValue;
                            break;
                        case 41:
                            vertex.EndingWidth = pair.DoubleValue;
                            break;
                        case 42:
                            vertex.Bulge = pair.DoubleValue;
                            break;
                        case 50:
                            vertex.CurveFitTangentDirection = pair.DoubleValue;
                            break;
                        case 70:
                            vertex.vertexFlags = pair.ShortValue;
                            break;
                        case 71:
                            vertex.PolyfaceMeshVertexIndex1 = pair.ShortValue;
                            break;
                        case 72:
                            vertex.PolyfaceMeshVertexIndex2 = pair.ShortValue;
                            break;
                        case 73:
                            vertex.PolyfaceMeshVertexIndex3 = pair.ShortValue;
                            break;
                        case 74:
                            vertex.PolyfaceMeshVertexIndex4 = pair.ShortValue;
                            break;
                        default:
                            // unknown or unsupported attribute
                            break;
                    }
                }
            }

            return vertex;
        }
    }
}
