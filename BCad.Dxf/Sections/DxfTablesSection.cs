using System.Collections.Generic;
using System.Linq;
using BCad.Dxf.Tables;

namespace BCad.Dxf.Sections
{
    internal class DxfTablesSection : DxfSection
    {
        public override DxfSectionType Type
        {
            get { return DxfSectionType.Tables; }
        }

        private DxfLayerTable layerTable;
        private DxfViewPortTable viewPortTable;

        public List<DxfLayer> Layers
        {
            get { return layerTable.Layers; }
        }

        public List<DxfViewPort> ViewPorts
        {
            get { return viewPortTable.ViewPorts; }
        }

        public IEnumerable<DxfTable> Tables
        {
            get
            {
                yield return layerTable;
                yield return viewPortTable;
            }
        }

        public DxfTablesSection()
        {
            layerTable = new DxfLayerTable();
            viewPortTable = new DxfViewPortTable();
        }

        internal static DxfTablesSection TablesSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            throw new System.NotImplementedException();
        }

        private static bool IsTableStart(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.TableText;
        }

        private static bool IsTableEnd(DxfCodePair pair)
        {
            return pair.Code == 0 && pair.StringValue == DxfSection.EndTableText;
        }

        public override IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                foreach (var t in Tables)
                {
                    var pairs = t.ValuePairs;
                    if (pairs.Count() > 0)
                    {
                        yield return new DxfCodePair(0, DxfSection.TableText);
                        yield return new DxfCodePair(2, t.TableTypeName);
                        foreach (var p in pairs)
                        {
                            yield return p;
                        }
                        yield return new DxfCodePair(0, DxfSection.EndTableText);
                    }
                }
            }
        }
    }
}
