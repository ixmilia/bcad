using System.IO;
using BCad.Dxf;

namespace BCad.Test.DxfTests
{
    public abstract class AbstractDxfTests
    {
        protected static DxfFile Parse(string data)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine(data.Trim());
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return DxfFile.Load(ms);
            }
        }

        protected static DxfFile Section(string sectionName, string data)
        {
            return Parse(string.Format(@"
0
SECTION
2
{0}
{1}
0
ENDSEC
0
EOF
", sectionName, data.Trim()));
        }
    }
}
