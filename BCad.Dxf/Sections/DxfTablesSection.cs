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

        public DxfTablesSection(IEnumerable<DxfCodePair> pairs)
            : this()
        {
            foreach (var t in SplitTables(pairs))
            {
                switch (DxfTable.TableNameToType(t.TableName))
                {
                    case DxfTableType.Layer:
                        layerTable = new DxfLayerTable(DxfSection.SplitAtZero(t.ValuePairs).Select(l => DxfLayer.FromPairs(l)));
                        break;
                    case DxfTableType.ViewPort:
                        var viewPorts = DxfSection.SplitAtZero(t.ValuePairs)
                            .Select(v => DxfViewPort.FromPairs(v))
                            .Where(v => v.Name != null);
                        viewPortTable = new DxfViewPortTable(viewPorts);
                        break;
                }
            }
        }

        private static IEnumerable<DxfSimpleTable> SplitTables(IEnumerable<DxfCodePair> pairs)
        {
            var l = pairs.ToList();
            var tables = new List<DxfSimpleTable>();
            var su = new SingleUseEnumerable<DxfCodePair>(l);

            while (su.Count > 0)
            {
                var item = su.GetItem();
                if (item == null)
                    throw new DxfReadException("Unexpected end of section");
                if (!IsTableStart(item))
                    throw new DxfReadException("Expected start of table, got " + item);
                var header = su.GetItem();
                if (header == null)
                    throw new DxfReadException("Unexpected end of section");
                if (header.Code != 2)
                    throw new DxfReadException("Expected table type");
                string name = header.StringValue;
                var values = su.GetItems().TakeWhile(p => !IsTableEnd(p)).ToList();
                tables.Add(new DxfSimpleTable(name, values));
            }

            if (su.Count > 0)
                throw new DxfReadException("Unexpected values after table");

            return tables;
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
