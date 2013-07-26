using BCad.Dxf.Sections;
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

        abstract internal IEnumerable<DxfCodePair> GetValuePairs();

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

        internal static DxfTable FromBuffer(DxfCodePairBufferReader buffer)
        {
            var pair = buffer.Peek();
            buffer.Advance();
            if (pair.Code != 2)
            {
                throw new DxfReadException("Expected table type.");
            }

            DxfTable result;
            switch (pair.StringValue)
            {
                case DxfTable.DimStyleText:
                    result = DxfDimStyleTable.DimStyleTableFromBuffer(buffer);
                    break;
                case DxfTable.LayerText:
                    result = DxfLayerTable.LayerTableFromBuffer(buffer);
                    break;
                case DxfTable.LTypeText:
                    result = DxfLinetypeTable.LinetypeTableFromBuffer(buffer);
                    break;
                case DxfViewPort.ViewPortText:
                    result = DxfViewPortTable.ViewPortTableFromBuffer(buffer);
                    break;
                default:
                    SwallowTable(buffer);
                    result = null;
                    break;
            }

            return result;
        }

        internal static void SwallowTable(DxfCodePairBufferReader buffer)
        {
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                    break;
            }
        }
    }
}
