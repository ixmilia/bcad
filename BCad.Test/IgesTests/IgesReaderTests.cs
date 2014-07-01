using System;
using System.IO;
using BCad.Iges;
using BCad.Iges.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test.IgesTests
{
    [TestClass]
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

        [TestMethod]
        public void GlobalParseTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,8Hprotocol;                             G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.AreEqual(',', file.FieldDelimiter);
            Assert.AreEqual(';', file.RecordDelimiter);
            Assert.AreEqual("identifier", file.Identification);
            Assert.AreEqual(@"C:\path\to\full\filename.igs", file.FullFileName);
            Assert.AreEqual(@"BCAD", file.SystemIdentifier);
            Assert.AreEqual(@"1.0", file.SystemVersion);
            Assert.AreEqual(16, file.IntegerSize);
            Assert.AreEqual(7, file.SingleSize);
            Assert.AreEqual(22, file.DecimalDigits);
            Assert.AreEqual(10, file.DoubleMagnitude);
            Assert.AreEqual(51, file.DoublePrecision);
            Assert.AreEqual("ident2", file.Identifier);
            Assert.AreEqual(0.75, file.ModelSpaceScale);
            Assert.AreEqual(IgesUnits.Centimeters, file.ModelUnits);
            Assert.IsNull(file.CustomModelUnits);
            Assert.AreEqual(4, file.MaxLineWeightGraduations);
            Assert.AreEqual(0.8, file.MaxLineWeight);
            Assert.AreEqual(new DateTime(1983, 11, 23, 13, 08, 11), file.TimeStamp);
            Assert.AreEqual(0.001, file.MinimumResolution);
            Assert.AreEqual(500.0, file.MaxCoordinateValue);
            Assert.AreEqual("Brett", file.Author);
            Assert.AreEqual("IxMilia", file.Organization);
            Assert.AreEqual(IgesVersion.v5_0, file.IgesVersion);
            Assert.AreEqual(IgesDraftingStandard.BSI, file.DraftingStandard);
            Assert.AreEqual(new DateTime(1987, 5, 8, 12, 34, 56), file.ModifiedTime);
            Assert.AreEqual("protocol", file.ApplicationProtocol);
        }

        [TestMethod]
        public void GlobalParseWithLeadingWhitespaceTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811, 1.0E-003,500,5HBretG      2
t,7HIxMilia, 8,4,13H870508.123456, 8Hprotocol;                          G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.AreEqual(0.001, file.MinimumResolution); // leading space on double
            Assert.AreEqual(IgesVersion.v5_0, file.IgesVersion); // leading space on int
            Assert.AreEqual("protocol", file.ApplicationProtocol); // leading space on string
        }

        [TestMethod]
        public void GlobalParseWithMissingStringField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,;                                       G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.AreEqual(null, file.ApplicationProtocol);
        }

        [TestMethod]
        public void GlobalParseWithMissingIntField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,,13H870508.123456,8Hprotocol;                              G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.AreEqual(IgesDraftingStandard.None, file.DraftingStandard);
        }

        [TestMethod]
        public void GlobalParseWithMissingDoubleField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,,500,5HBrett,7HIxMilG      2
ia,8,4,13H870508.123456,8Hprotocol;                                     G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.AreEqual(0.0, file.MinimumResolution);
        }

        [TestMethod]
        public void FileWithNonStandardDelimitersTest()
        {
            var file = CreateFile(@"
                                                                        S      1
1H//1H#/10Hidentifier/12Hfilename.igs#                                  G      1
");
            Assert.AreEqual('/', file.FieldDelimiter);
            Assert.AreEqual('#', file.RecordDelimiter);
            Assert.AreEqual("identifier", file.Identification);
            Assert.AreEqual("filename.igs", file.FullFileName);
        }

        [TestMethod]
        public void StringContainingDelimiterValuesTest()
        {
            var file = CreateFile(@"
                                                                        S      1
1H,,1H;,6H,;,;,;;                                                       G      1
");
            Assert.AreEqual(",;,;,;", file.Identification);
        }

        [TestMethod]
        public void MissingStartSectionTest()
        {
            var file = CreateFile(@"
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,,500,5HBrett,7HIxMilG      2
ia,8,4,13H870508.123456,8Hprotocol;                                     G      3
S      0G      3D      0P      0                                        T      1
");
            Assert.AreEqual(',', file.FieldDelimiter);
        }

        [TestMethod]
        public void MissingGlobalSectionTest()
        {
            var file = CreateFile(@"
                                                                        S      1
S      1G      0D      0P      0                                        T      1
");
            Assert.AreEqual(',', file.FieldDelimiter);
        }

        [TestMethod]
        public void OnlyTerminateLineTest()
        {
            var file = CreateFile(@"
S      0G      0D      0P      0                                        T      1
");
            Assert.AreEqual(',', file.FieldDelimiter);
        }

        [TestMethod]
        public void EmptyFileTest()
        {
            var file = CreateFile(string.Empty);
            Assert.AreEqual(',', file.FieldDelimiter);
        }
    }
}
