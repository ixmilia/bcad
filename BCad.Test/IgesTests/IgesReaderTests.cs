using System;
using System.IO;
using BCad.Iges;
using Xunit;

namespace BCad.Test.IgesTests
{
    public class IgesReaderTests
    {
        internal static IgesFile CreateFile(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content.Trim('\r', '\n'));
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var file = IgesFile.Load(stream);
            return file;
        }

        [Fact]
        public void GlobalParseTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,8Hprotocol;                             G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(',', file.FieldDelimiter);
            Assert.Equal(';', file.RecordDelimiter);
            Assert.Equal("identifier", file.Identification);
            Assert.Equal(@"C:\path\to\full\filename.igs", file.FullFileName);
            Assert.Equal(@"BCAD", file.SystemIdentifier);
            Assert.Equal(@"1.0", file.SystemVersion);
            Assert.Equal(16, file.IntegerSize);
            Assert.Equal(7, file.SingleSize);
            Assert.Equal(22, file.DecimalDigits);
            Assert.Equal(10, file.DoubleMagnitude);
            Assert.Equal(51, file.DoublePrecision);
            Assert.Equal("ident2", file.Identifier);
            Assert.Equal(0.75, file.ModelSpaceScale);
            Assert.Equal(IgesUnits.Centimeters, file.ModelUnits);
            Assert.Null(file.CustomModelUnits);
            Assert.Equal(4, file.MaxLineWeightGraduations);
            Assert.Equal(0.8, file.MaxLineWeight);
            Assert.Equal(new DateTime(1983, 11, 23, 13, 08, 11), file.TimeStamp);
            Assert.Equal(0.001, file.MinimumResolution);
            Assert.Equal(500.0, file.MaxCoordinateValue);
            Assert.Equal("Brett", file.Author);
            Assert.Equal("IxMilia", file.Organization);
            Assert.Equal(IgesVersion.v5_0, file.IgesVersion);
            Assert.Equal(IgesDraftingStandard.BSI, file.DraftingStandard);
            Assert.Equal(new DateTime(1987, 5, 8, 12, 34, 56), file.ModifiedTime);
            Assert.Equal("protocol", file.ApplicationProtocol);
        }
    }
}
