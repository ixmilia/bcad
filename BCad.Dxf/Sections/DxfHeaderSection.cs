using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        internal static DxfHeaderSection HeaderSectionFromBuffer(DxfCodePairBufferReader buffer)
        {
            var section = new DxfHeaderSection();
            string keyName = null;
            while (buffer.ItemsRemain)
            {
                var pair = buffer.Peek();
                buffer.Advance();
                if (DxfCodePair.IsSectionEnd(pair))
                {
                    // done reading settings
                    break;
                }

                if (keyName == null)
                {
                    // what setting to set
                    Debug.Assert(pair.Code == 9);
                    keyName = pair.StringValue;
                }
                else
                {
                    // the value of the setting
                    switch (keyName)
                    {
                        case CLAYER:
                            EnsureCode(pair, 8);
                            section.CurrentLayer = pair.StringValue;
                            break;
                        default:
                            // unsupported variable
                            break;
                    }

                    keyName = null; // reset for next read
                }
            }

            if (keyName != null)
            {
                throw new DxfReadException("Expected value for key " + keyName);
            }

            return section;
        }

        private static void EnsureCode(DxfCodePair pair, int code)
        {
            if (pair.Code != code)
            {
                throw new DxfReadException(string.Format("Expected code {0}, got {1}", code, pair.Code));
            }
        }
    }
}
