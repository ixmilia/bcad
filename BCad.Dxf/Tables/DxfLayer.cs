using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfLayer : DxfSymbolTableFlags
    {
        public const string LayerText = "LAYER";

        public string Name { get; set; }

        public DxfColor Color { get; set; }

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

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            yield return new DxfCodePair(0, LayerText);
            yield return new DxfCodePair(2, Name);
            yield return new DxfCodePair(70, (short)Flags);
            yield return new DxfCodePair(62, Color.RawValue);
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
