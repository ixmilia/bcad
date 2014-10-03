using System.Collections.Generic;
using System.Linq;
using IxMilia.Dxf.Sections;

namespace IxMilia.Dxf.Tables
{
    public class DxfAppIdTable : DxfTable
    {
        public override DxfTableType TableType
        {
            get { return DxfTableType.AppId; }
        }

        public List<DxfAppId> ApplicationIds { get; private set; }

        public DxfAppIdTable()
        {
            ApplicationIds = new List<DxfAppId>();
        }

        internal override IEnumerable<DxfCodePair> GetValuePairs(DxfAcadVersion version)
        {
            if (ApplicationIds.Count == 0)
                yield break;
            foreach (var common in CommonCodePairs(version))
            {
                yield return common;
            }

            foreach (var appId in ApplicationIds.OrderBy(d => d.Name))
            {
                foreach (var pair in appId.GetValuePairs())
                    yield return pair;
            }

            yield return new DxfCodePair(0, DxfSection.EndTableText);
        }

        internal static DxfAppIdTable AppIdTableFromBuffer(DxfCodePairBufferReader buffer)
        {
            var table = new DxfAppIdTable();
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfTablesSection.IsTableEnd(pair))
                {
                    break;
                }

                if (pair.Code == 0 && pair.StringValue == DxfTable.AppIdText)
                {
                    var appId = DxfAppId.FromBuffer(buffer);
                    table.ApplicationIds.Add(appId);
                }
            }

            return table;
        }
    }
}
