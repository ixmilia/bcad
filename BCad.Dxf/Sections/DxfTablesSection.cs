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

        public DxfLayerTable LayerTable { get; private set; }
        public DxfViewPortTable ViewPortTable { get; private set; }

        public DxfTablesSection()
        {
            this.LayerTable = new DxfLayerTable();
            this.ViewPortTable = new DxfViewPortTable();
        }

        protected internal override IEnumerable<DxfCodePair> GetSpecificPairs()
        {
            foreach (var table in new DxfTable[] { LayerTable, ViewPortTable })
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
                switch (table.TableType)
                {
                    case DxfTableType.Layer:
                        section.LayerTable = (DxfLayerTable)table;
                        break;
                    case DxfTableType.ViewPort:
                        section.ViewPortTable = (DxfViewPortTable)table;
                        break;
                    default:
                        throw new DxfReadException("Unexpected table type " + table.TableType);
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
