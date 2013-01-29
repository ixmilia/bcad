using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCad.Dxf.Sections;
using System.Diagnostics;

namespace BCad.Dxf.Entities
{
    public enum CurvedAndSmoothSurfaceType
    {
        None = 0,
        QuadraticBSpline = 5,
        CubicBSpline = 6,
        Bezier = 8
    }

    public class DxfPolyline : DxfEntity
    {
        public override DxfEntityType EntityType { get { return DxfEntityType.Polyline; } }

        public override string SubclassMarker { get { return "AcDb2dPolyline"; } }

        public double Elevation { get; set; }

        public DxfVector Normal { get; set; }

        public double Thickness { get; set; }

        public double DefaultStartingWidth { get; set; }

        public double DefaultEndingWidth { get; set; }

        public short PolygonMeshMVertexCount { get; set; }

        public short PolygonMeshNVertexCount { get; set; }

        public short SmoothSurfaceMDensity { get; set; }

        public short SmoothSurfaceNDensity { get; set; }

        public CurvedAndSmoothSurfaceType SurfaceType { get; set; }

        private int bitField = 0;

        public bool IsClosed
        {
            get { return GetBit(bitField, 1); }
            set { bitField = SetBit(bitField, 1, value); }
        }

        public bool ContainsCurveFitVerticies
        {
            get { return GetBit(bitField, 2); }
            set { bitField = SetBit(bitField, 2, value); }
        }

        public bool ContainsSplineFitVerticies
        {
            get { return GetBit(bitField, 3); }
            set { bitField = SetBit(bitField, 3, value); }
        }

        public bool Is3DPolyline
        {
            get { return GetBit(bitField, 4); }
            set { bitField = SetBit(bitField, 4, value); }
        }

        public bool Is3DPolygonMesh
        {
            get { return GetBit(bitField, 5); }
            set { bitField = SetBit(bitField, 5, value); }
        }

        public bool Is3DMeshClosedInNDirection
        {
            get { return GetBit(bitField, 6); }
            set { bitField = SetBit(bitField, 6, value); }
        }

        public bool IsPolyfaceMesh
        {
            get { return GetBit(bitField, 7); }
            set { bitField = SetBit(bitField, 7, value); }
        }

        public bool IsContinuousLinetipePattern
        {
            get { return GetBit(bitField, 8); }
            set { bitField = SetBit(bitField, 8, value); }
        }

        public List<DxfVertex> Vertices { get; private set; }

        public DxfSeqend SequenceEnd { get; private set; }

        public DxfPolyline()
        {
            Vertices = new List<DxfVertex>();
            SequenceEnd = new DxfSeqend();
            Normal = DxfVector.ZAxis;
            SurfaceType = CurvedAndSmoothSurfaceType.None;
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in base.GetCommonValuePairs())
                yield return pair;
            yield return new DxfCodePair(10, 0.0);
            yield return new DxfCodePair(20, 0.0);
            yield return new DxfCodePair(30, Elevation);
            if (Thickness != 0.0)
                yield return new DxfCodePair(39, Thickness);
            if (bitField != 0)
                yield return new DxfCodePair(70, (short)bitField);
            if (DefaultStartingWidth != 0.0)
                yield return new DxfCodePair(40, DefaultStartingWidth);
            if (DefaultEndingWidth != 0.0)
                yield return new DxfCodePair(41, DefaultEndingWidth);
            if (PolygonMeshMVertexCount != 0)
                yield return new DxfCodePair(71, PolygonMeshMVertexCount);
            if (PolygonMeshNVertexCount != 0)
                yield return new DxfCodePair(72, PolygonMeshNVertexCount);
            if (SmoothSurfaceMDensity != 0)
                yield return new DxfCodePair(73, SmoothSurfaceMDensity);
            if (SmoothSurfaceNDensity != 0)
                yield return new DxfCodePair(74, SmoothSurfaceNDensity);
            if (SurfaceType != CurvedAndSmoothSurfaceType.None)
                yield return new DxfCodePair(75, (short)SurfaceType);
            if (Normal != DxfVector.ZAxis)
            {
                yield return new DxfCodePair(210, Normal.X);
                yield return new DxfCodePair(220, Normal.Y);
                yield return new DxfCodePair(230, Normal.Z);
            }

            foreach (var vertex in Vertices)
            {
                foreach (var pair in vertex.GetValuePairs())
                {
                    yield return pair;
                }
            }

            foreach (var pair in SequenceEnd.GetValuePairs())
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
                        case 39:
                            poly.Thickness = pair.DoubleValue;
                            break;
                        case 40:
                            poly.DefaultStartingWidth = pair.DoubleValue;
                            break;
                        case 41:
                            poly.DefaultEndingWidth = pair.DoubleValue;
                            break;
                        case 70:
                            poly.bitField = pair.ShortValue;
                            break;
                        case 71:
                            poly.PolygonMeshMVertexCount = pair.ShortValue;
                            break;
                        case 72:
                            poly.PolygonMeshNVertexCount = pair.ShortValue;
                            break;
                        case 73:
                            poly.SmoothSurfaceMDensity = pair.ShortValue;
                            break;
                        case 74:
                            poly.SmoothSurfaceNDensity = pair.ShortValue;
                            break;
                        case 75:
                            poly.SurfaceType = (CurvedAndSmoothSurfaceType)pair.ShortValue;
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
