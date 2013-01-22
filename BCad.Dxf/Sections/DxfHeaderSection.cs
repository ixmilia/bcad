using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BCad.Dxf.Sections
{
    public class DxfHeaderSection : DxfSection
    {
        public string CurrentLayer { get; set; }
        public DxfAcadVersion Version { get; set; }

        private const string CLAYER = "$CLAYER";
        private const string ACADVER = "$ACADVER";

        public DxfHeaderSection()
        {
            CurrentLayer = null;
            Version = DxfAcadVersion.R14;
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

                yield return new DxfCodePair(9, ACADVER);
                yield return new DxfCodePair(1, DxfAcadVersionStrings.VersionToString(Version));
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
                        case ACADVER:
                            EnsureCode(pair, 1);
                            section.Version = DxfAcadVersionStrings.StringToVersion(pair.StringValue);
                            break;
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
