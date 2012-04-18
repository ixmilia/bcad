using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Sections
{
    internal class DxfHeaderSection : DxfSection
    {
        public string CurrentLayer { get; set; }

        private const string CLAYER = "$CLAYER";

        public DxfHeaderSection()
        {
            CurrentLayer = null;
        }

        public DxfHeaderSection(IEnumerable<DxfCodePair> pairs)
            : this()
        {
            // a header variables come in pairs:
            // 9, $VARNAME
            // <code>, <value>
            var pairList = pairs.ToList();
            for (int i = 0; i < pairList.Count - 1; i += 2)
            {
                var variable = pairList[i];
                if (variable.Code != 9)
                {
                    throw new DxfReadException("Expected code 9 for header variable, got " + variable.Code);
                }

                var pair = pairList[i + 1];
                switch (variable.StringValue.ToUpperInvariant())
                {
                    case CLAYER:
                        EnsureCode(pair, 8);
                        CurrentLayer = pair.StringValue;
                        break;
                    default:
                        // unsupported variable
                        break;
                }
            }
        }

        private static void EnsureCode(DxfCodePair pair, int code)
        {
            if (pair.Code != code)
            {
                throw new DxfReadException(string.Format("Expected code {0}, got {1}", code, pair.Code));
            }
        }

        public override DxfSectionType Type
        {
            get { return DxfSectionType.Header; }
        }

        public override IEnumerable<DxfCodePair> ValuePairs
        {
            get
            {
                if (!string.IsNullOrEmpty(CurrentLayer))
                {
                    yield return new DxfCodePair(9, CLAYER);
                    yield return new DxfCodePair(8, CurrentLayer);
                }
            }
        }
    }
}
