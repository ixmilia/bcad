using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Igs;
using Xunit;

namespace BCad.Test.IgsTests
{
    public class IgsReaderTests
    {
        private static IgsFile CreateFile(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content.Trim());
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var file = IgsFile.Load(stream);
            return file;
        }

        [Fact]
        public void GlobalParseTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51;                                                               G      1
");
            Assert.Equal("identifier", file.Identification);
            Assert.Equal(@"C:\path\to\full\filename.igs", file.FullFileName);
            Assert.Equal(@"BCAD", file.SystemIdentifier);
            Assert.Equal(@"1.0", file.SystemVersion);
            Assert.Equal(16, file.IntegerSize);
            Assert.Equal(7, file.SingleSize);
            Assert.Equal(22, file.DecimalDigits);
            Assert.Equal(10, file.DoubleMagnitude);
            Assert.Equal(51, file.DoublePrecision);
        }
    }
}
