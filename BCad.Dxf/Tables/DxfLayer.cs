using System.Collections.Generic;

namespace BCad.Dxf.Tables
{
    public class DxfLayer
    {
        public const string LayerText = "LAYER";

        public string Name { get; set; }

        public DxfColor Color { get; set; }

        public DxfLayer()
            : this("UNDEFINED")
        {
        }

        public DxfLayer(string name)
            : this(name, DxfColor.ByBlock)
        {
        }

        public DxfLayer(string name, DxfColor color)
        {
            Name = name;
            Color = color;
        }

        internal IEnumerable<DxfCodePair> GetValuePairs()
        {
            yield return new DxfCodePair(0, LayerText);
            yield return new DxfCodePair(2, Name);
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
                }
            }

            return layer;
        }
    }
}
