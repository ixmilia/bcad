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
        Solid,
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
        public const string SolidType = "SOLID";
        public const string VertexType = "VERTEX";

        public const string BYLAYER = "BYLAYER";

        public abstract DxfEntityType EntityType { get; }

        public abstract string SubclassMarker { get; }

        public string Handle { get; set; }

        public string Layer { get; set; }

        public string LinetypeName { get; set; }

        public double LinetypeScale { get; set; }

        public DxfColor Color { get; set; }

        public bool IsInPaperSpace { get; set; }

        public bool IsVisible { get; set; }

        public DxfEntity()
        {
            Color = DxfColor.ByBlock;
            LinetypeScale = 1.0;
            IsVisible = true;
        }

        abstract internal IEnumerable<DxfCodePair> GetValuePairs();

        protected internal IEnumerable<DxfCodePair> GetCommonValuePairs()
        {
            yield return new DxfCodePair(0, this.EntityTypeString);
            if (!string.IsNullOrEmpty(Handle))
                yield return new DxfCodePair(5, Handle);
            if (!string.IsNullOrEmpty(LinetypeName) && LinetypeName != BYLAYER)
                yield return new DxfCodePair(6, LinetypeName);
            if (!string.IsNullOrEmpty(Layer))
                yield return new DxfCodePair(8, Layer);
            if (LinetypeScale != 1.0)
                yield return new DxfCodePair(48, LinetypeScale);
            if (!IsVisible)
                yield return new DxfCodePair(60, 1);
            if (!Color.IsByLayer)
                yield return new DxfCodePair(62, Color.RawValue);
            if (IsInPaperSpace)
                yield return new DxfCodePair(67, (short)1);
            if (!string.IsNullOrEmpty(SubclassMarker))
                yield return new DxfCodePair(100, SubclassMarker);
        }

        protected internal bool TrySetSharedCode(DxfCodePair pair)
        {
            switch (pair.Code)
            {
                case 5: // handle
                    this.Handle = pair.HandleValue;
                    break;
                case 6: // linetype
                    this.LinetypeName = pair.StringValue;
                    break;
                case 8: // layer
                    this.Layer = pair.StringValue;
                    break;
                case 48: // linetype scale
                    this.LinetypeScale = pair.DoubleValue;
                    break;
                case 60:
                    this.IsVisible = pair.ShortValue == 0;
                    break;
                case 62: // color
                    this.Color = DxfColor.FromRawValue(pair.ShortValue);
                    break;
                case 67:
                    this.IsInPaperSpace = pair.ShortValue == 1;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected static bool GetBit(int bitField, int bitNumber)
        {
            return ((1 << (bitNumber - 1)) & bitField) != 0;
        }

        protected static int SetBit(int bitField, int bitNumber, bool value)
        {
            if (value)
                return bitField | (1 << (bitNumber - 1));
            else
                return bitField & ~(1 << (bitNumber - 1));
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
                case SolidType:
                    entity = DxfSolid.SolidFromBuffer(buffer);
                    break;
                case TextType:
                    entity = DxfText.TextFromBuffer(buffer);
                    break;
                case VertexType:
                    entity = DxfVertex.VertexFromBuffer(buffer);
                    break;
                default:
                    SwallowEntity(buffer);
                    entity = null;
                    break;
            }

            return entity;
        }

        internal static void SwallowEntity(DxfCodePairBufferReader buffer)
        {
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                    break;
                buffer.Advance();
            }
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
                    case DxfEntityType.Solid:
                        name = SolidType;
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
