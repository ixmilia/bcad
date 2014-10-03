using System.Collections.Generic;
using IxMilia.Dxf.Tables;

namespace IxMilia.Dxf
{
    public class DxfLayer : DxfSymbolTableFlags
    {
        private const string AcDbLayerTableRecordText = "AcDbLayerTableRecord";

        public const string LayerText = "LAYER";

        public string Name { get; set; }

        public DxfColor Color { get; set; }

        public string LinetypeName { get; set; }

        public bool IsFrozen
        {
            get { return DxfHelpers.GetFlag(Flags, 1); }
            set { DxfHelpers.SetFlag(value, ref Flags, 1); }
        }

        public bool IsFrozenInNewViewports
        {
            get { return DxfHelpers.GetFlag(Flags, 2); }
            set { DxfHelpers.SetFlag(value, ref Flags, 2); }
        }

        public bool IsLocked
        {
            get { return DxfHelpers.GetFlag(Flags, 4); }
            set { DxfHelpers.SetFlag(value, ref Flags, 4); }
        }

        public DxfLayer()
            : this("UNDEFINED")
        {
        }

        public DxfLayer(string name)
            : this(name, DxfColor.ByBlock)
        {
        }

        public DxfLayer(string name, DxfColor color)
            : base()
        {
            Name = name;
            Color = color;
        }

        protected override string TableType { get { return DxfTable.LayerText; } }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            foreach (var pair in CommonCodePairs())
                yield return pair;
            yield return new DxfCodePair(100, AcDbLayerTableRecordText);
            yield return new DxfCodePair(2, Name);
            yield return new DxfCodePair(70, (short)Flags);
            yield return new DxfCodePair(62, Color.RawValue);
            yield return new DxfCodePair(6, LinetypeName);
        }

        internal static DxfLayer FromBuffer(DxfCodePairBufferReader buffer)
        {
            var layer = new DxfLayer();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                if (pair.Code == 0)
                {
                    break;
                }

                buffer.Advance();
                switch (pair.Code)
                {
                    case 2:
                        layer.Name = pair.StringValue;
                        break;
                    case 6:
                        layer.LinetypeName = pair.StringValue;
                        break;
                    case 62:
                        layer.Color.RawValue = pair.ShortValue;
                        break;
                    case 70:
                        layer.Flags = pair.ShortValue;
                        break;
                }
            }

            return layer;
        }
    }
}
