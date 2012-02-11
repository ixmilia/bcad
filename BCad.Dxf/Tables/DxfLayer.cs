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
            : this(name, DxfColor.FromIndex(7))
        {
        }

        public DxfLayer(string name, DxfColor color)
        {
            Name = name;
            Color = color;
        }

        public IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                yield return new DxfCodePair(2, Name);
                yield return new DxfCodePair(62, Color.RawValue);
            }
        }

        public static DxfLayer FromPairs(IEnumerable<DxfCodePair> pairs)
        {
            DxfLayer layer = new DxfLayer();
            foreach (var p in pairs)
            {
                switch (p.Code)
                {
                    case 2:
                        layer.Name = p.StringValue;
                        break;
                    case 62:
                        layer.Color.RawValue = p.ShortValue;
                        break;
                }
            }
            return layer;
        }
    }
}
