using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Tables;

namespace BCad.Dxf.Sections
{
    public class DxfTablesSection : DxfSection
    {
        public override DxfSectionType Type
        {
            get { return DxfSectionType.Tables; }
        }

        public DxfDimStyleTable DimStyleTable { get; private set; }
        public DxfLayerTable LayerTable { get; private set; }
        public DxfLinetypeTable LTypeTable { get; private set; }
        public DxfStyleTable StyleTable { get; private set; }
        public DxfViewPortTable ViewPortTable { get; private set; }

        public DxfTablesSection()
        {
            this.DimStyleTable = new DxfDimStyleTable();
            this.LayerTable = new DxfLayerTable();
            this.LTypeTable = new DxfLinetypeTable();
            this.StyleTable = new DxfStyleTable();
            this.ViewPortTable = new DxfViewPortTable();
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
            foreach (var table in new DxfTable[] { DimStyleTable, LayerTable, LTypeTable, StyleTable, ViewPortTable })
            {
                foreach (var pair in table.GetValuePairs())
                    yield return pair;
            }
        }

        internal static DxfTablesSection TablesSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfTablesSection();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    break;
                }

                if (!IsTableStart(pair))
                {
                    throw new DxfReadException("Expected start of table.");
                }

                var table = DxfTable.FromBuffer(buffer);
                if (table != null)
                {
                    switch (table.TableType)
                    {
                        case DxfTableType.DimStyle:
                            section.DimStyleTable = (DxfDimStyleTable)table;
                            break;
                        case DxfTableType.Layer:
                            section.LayerTable = (DxfLayerTable)table;
                            break;
                        case DxfTableType.LType:
                            section.LTypeTable = (DxfLinetypeTable)table;
                            break;
                        case DxfTableType.Style:
                            section.StyleTable = (DxfStyleTable)table;
                            break;
                        case DxfTableType.ViewPort:
                            section.ViewPortTable = (DxfViewPortTable)table;
                            break;
                        default:
                            throw new DxfReadException("Unexpected table type " + table.TableType);
                    }
                }
            }

            return section;
        }

        internal static bool IsTableStart(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.TableText;
        }

        internal static bool IsTableEnd(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.EndTableText;
        }
    }
}
