using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BCad.Dxf.Entities
{
    public enum DxfEntityType
    {
        Attribute,
        Line,
        Circle,
        Arc,
        Ellipse,
        Text,
        Polyline,
        Seqend,
        Vertex
    }

    public abstract class DxfEntity
    {
        public const string LineType = "LINE";
        public const string CircleType = "CIRCLE";
        public const string ArcType = "ARC";
        public const string EllipseType = "ELLIPSE";
        public const string TextType = "TEXT";
        public const string PolylineType = "POLYLINE";
        public const string SeqendType = "SEQEND";
        public const string VertexType = "VERTEX";

        public abstract DxfEntityType EntityType { get; }

        public abstract string SubclassMarker { get; }

        public string Handle { get; set; }

        public string Layer { get; set; }

        public DxfColor Color { get; set; }

        public DxfEntity()
        {
            Color = DxfColor.ByBlock;
        }

        public IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                if (!string.IsNullOrEmpty(Handle))
                    yield return new DxfCodePair(5, Handle);
                if (!string.IsNullOrEmpty(Layer))
                    yield return new DxfCodePair(8, Layer);
                if (!Color.IsByLayer)
                    yield return new DxfCodePair(62, Color.RawValue);
                if (!string.IsNullOrEmpty(SubclassMarker))
                    yield return new DxfCodePair(100, SubclassMarker);
                foreach (var pair in GetEntitySpecificPairs())
                    yield return pair;
            }
        }

        internal abstract IEnumerable<DxfCodePair> GetEntitySpecificPairs();

        protected internal bool TrySetSharedCode(DxfCodePair pair)
        {
            switch (pair.Code)
            {
                case 5: // handle
                    this.Handle = pair.HandleValue;
                    break;
                case 8: // layer
                    this.Layer = pair.StringValue;
                    break;
                case 62: // color
                    this.Color = DxfColor.FromRawValue(pair.ShortValue);
                    break;
                default:
                    return false;
            }

            return true;
        }

        internal static DxfEntity FromBuffer(DxfCodePairBufferReader buffer)
        {
            var first = buffer.Peek();
            buffer.Advance();
            DxfEntity entity = null;
            switch (first.StringValue)
            {
                case ArcType:
                    entity = DxfArc.ArcFromBuffer(buffer);
                    break;
                case CircleType:
                    entity = DxfCircle.CircleFromBuffer(buffer);
                    break;
                case EllipseType:
                    entity = DxfEllipse.EllipseFromBuffer(buffer);
                    break;
                case LineType:
                    entity = DxfLine.LineFromBuffer(buffer);
                    break;
                case PolylineType:
                    entity = DxfPolyline.PolylineFromBuffer(buffer);
                    break;
                case SeqendType:
                    entity = DxfSeqend.SeqendFromBuffer(buffer);
                    break;
                case TextType:
                    entity = DxfText.TextFromBuffer(buffer);
                    break;
                case VertexType:
                    entity = DxfVertex.VertexFromBuffer(buffer);
                    break;
                default:
                    throw new DxfReadException("Unexpected entity type " + first.StringValue);
            }

            return entity;
        }

        public string EntityTypeString
        {
            get
            {
                string name = null;
                switch (EntityType)
                {
                    case DxfEntityType.Line:
                        name = LineType;
                        break;
                    case DxfEntityType.Circle:
                        name = CircleType;
                        break;
                    case DxfEntityType.Arc:
                        name = ArcType;
                        break;
                    case DxfEntityType.Ellipse:
                        name = EllipseType;
                        break;
                    case DxfEntityType.Text:
                        name = TextType;
                        break;
                    case DxfEntityType.Polyline:
                        name = PolylineType;
                        break;
                    case DxfEntityType.Seqend:
                        name = SeqendType;
                        break;
                    case DxfEntityType.Vertex:
                        name = VertexType;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return name;
            }
        }
    }
}
