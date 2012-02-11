using System.Collections.Generic;
using System.Linq;

namespace BCad.Dxf.Tables
{
    public abstract class DxfTable
    {
        public const string AppIdText = "APPID";
        public const string BlockRecordText = "BLOCK_RECORD";
        public const string DimStyleText = "DIMSTYLE";
        public const string LayerText = "LAYER";
        public const string LTypeText = "LTYPE";
        public const string StyleText = "STYLE";
        public const string UcsText = "UCS";
        public const string ViewText = "VIEW";
        public const string ViewPortText = "VPORT";

        public abstract DxfTableType TableType { get; }

        public IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                var pairs = GetTableValuePairs();
                if (pairs.Count() > 0)
                {
                    foreach (var p in pairs)
                        yield return p;
                }
            }
        }

        public abstract IEnumerable<DxfCodePair> GetTableValuePairs();

        public string TableTypeName
        {
            get { return TableTypeToName(TableType); }
        }

        public static DxfTableType TableNameToType(string name)
        {
            var type = DxfTableType.AppId;
            switch (name)
            {
                case AppIdText:
                    type = DxfTableType.AppId;
                    break;
                case BlockRecordText:
                    type = DxfTableType.BlockRecord;
                    break;
                case DimStyleText:
                    type = DxfTableType.DimStyle;
                    break;
                case LayerText:
                    type = DxfTableType.Layer;
                    break;
                case LTypeText:
                    type = DxfTableType.LType;
                    break;
                case StyleText:
                    type = DxfTableType.Style;
                    break;
                case UcsText:
                    type = DxfTableType.Ucs;
                    break;
                case ViewText:
                    type = DxfTableType.View;
                    break;
                case ViewPortText:
                    type = DxfTableType.ViewPort;
                    break;
            }
            return type;
        }

        public static string TableTypeToName(DxfTableType type)
        {
            string name = "NONE";
            switch (type)
            {
                case DxfTableType.AppId:
                    name = AppIdText;
                    break;
                case DxfTableType.BlockRecord:
                    name = BlockRecordText;
                    break;
                case DxfTableType.DimStyle:
                    name = DimStyleText;
                    break;
                case DxfTableType.Layer:
                    name = LayerText;
                    break;
                case DxfTableType.LType:
                    name = LTypeText;
                    break;
                case DxfTableType.Style:
                    name = StyleText;
                    break;
                case DxfTableType.Ucs:
                    name = UcsText;
                    break;
                case DxfTableType.View:
                    name = ViewText;
                    break;
                case DxfTableType.ViewPort:
                    name = ViewPortText;
                    break;
            }
            return name;
        }
    }
}
