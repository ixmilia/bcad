﻿using System;
using System.IO;
using System.Text;
using BCad.Iges;
using BCad.Iges.Entities;
using Xunit;

namespace BCad.Test.IgesTests
{
    public class IgesWriterTests
    {
        private static void VerifyFileText(IgesFile file, string expected, Action<string, string> verifier)
        {
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.ToArray();
            var actual = ASCIIEncoding.ASCII.GetString(bytes);
            verifier(expected.Trim('\r', '\n'), actual.Trim('\r', '\n'));
        }

        private static void VerifyFileExactly(IgesFile file, string expected)
        {
            VerifyFileText(file, expected, (ex, ac) => Assert.Equal(ex, ac));
        }

        private static void VerifyFileContains(IgesFile file, string expected)
        {
            VerifyFileText(file, expected, (ex, ac) => Assert.Contains(ex, ac));
        }

        [Fact]
        public void WriteEmptyFileTest()
        {
            var date = new DateTime(1983, 11, 23, 13, 8, 5);
            var file = new IgesFile()
            {
                ModifiedTime = date,
                TimeStamp = date
            };
            VerifyFileExactly(file, @"
                                                                        S      1
1H,,1H;,,,,,32,8,23,11,52,,1.,1,,0,1.,15H19831123.130805,1E-10,0.,,,11, G      1
0,15H19831123.130805,;                                                  G      2
S      1G      2D      0P      0                                        T      1
");
        }

        [Fact]
        public void WriteLineTest()
        {
            var file = new IgesFile();
            file.Entities.Add(new IgesLine()
            {
                P1 = new IgesPoint(1, 2, 3),
                P2 = new IgesPoint(4, 5, 6),
                Color = IgesColorNumber.Green
            });
            VerifyFileContains(file, @"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                                D      2
110,1.,2.,3.,4.,5.,6.;                                                 1P      1
");
        }

        [Fact]
        public void WriteLineWithSpanningParametersTest()
        {
            var file = new IgesFile();
            file.Entities.Add(new IgesLine()
            {
                P1 = new IgesPoint(1.1234512345, 2.1234512345, 3.1234512345),
                P2 = new IgesPoint(4.1234512345, 5.1234512345, 6.1234512345),
                Color = IgesColorNumber.Green
            });
            VerifyFileContains(file, @"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                                D      2
110,1.1234512345,2.1234512345,3.1234512345,4.1234512345,               1P      1
5.1234512345,6.1234512345;                                             1P      2
");
        }

        [Fact]
        public void WriteLineWithTransformationMatrixTest()
        {
            var file = new IgesFile();
            var trans = new IgesTransformationMatrix()
            {
                R11 = 1.0,
                R12 = 2.0,
                R13 = 3.0,
                T1 = 4.0,
                R21 = 5.0,
                R22 = 6.0,
                R23 = 7.0,
                T2 = 8.0,
                R31 = 9.0,
                R32 = 10.0,
                R33 = 11.0,
                T3 = 12.0
            };
            var line = new IgesLine()
            {
                P1 = new IgesPoint(1, 2, 3),
                P2 = new IgesPoint(4, 5, 6),
                TransformationMatrix = trans,
            };
            file.Entities.Add(line);
            VerifyFileContains(file, @"
     124       1       0       0       0                               0D      1
     124       0       0       0       0                                D      2
     110       2       0       0       0               1               0D      3
     110       0       0       1       0                                D      4
124,1.,2.,3.,4.,5.,6.,7.,8.,9.,10.,11.,12.;                            1P      1
110,1.,2.,3.,4.,5.,6.;                                                 3P      2
");
        }

        [Fact]
        public void WriteSubfigureEntityTest()
        {
            var trans = new IgesTransformationMatrix()
            {
                R11 = 1.0,
                R12 = 2.0,
                R13 = 3.0,
                T1 = 4.0,
                R21 = 5.0,
                R22 = 6.0,
                R23 = 7.0,
                T2 = 8.0,
                R31 = 9.0,
                R32 = 10.0,
                R33 = 11.0,
                T3 = 12.0
            };
            var sub = new IgesSubfigureDefinition();
            sub.Entities.Add(new IgesLine() { P1 = new IgesPoint(1, 2, 3), P2 = new IgesPoint(4, 5, 6), TransformationMatrix = trans });
            sub.Entities.Add(new IgesLine() { P1 = new IgesPoint(7, 8, 9), P2 = new IgesPoint(10, 11, 12) });
            var file = new IgesFile();
            file.Entities.Add(sub);
            VerifyFileContains(file, @"
     124       1       0       0       0                               0D      1
     124       0       0       0       0                                D      2
     110       2       0       0       0               1               0D      3
     110       0       0       1       0                                D      4
     110       3       0       0       0                               0D      5
     110       0       0       1       0                                D      6
     308       4       0       0       0                               0D      7
     308       0       0       0       0                                D      8
124,1.,2.,3.,4.,5.,6.,7.,8.,9.,10.,11.,12.;                            1P      1
110,1.,2.,3.,4.,5.,6.;                                                 3P      2
110,7.,8.,9.,10.,11.,12.;                                              5P      3
308,0,,2,3,5;                                                          7P      4
");
        }

        [Fact]
        public void WriteSpecificGlobalValuesTest()
        {
            var file = new IgesFile()
            {
                FieldDelimiter = ',',
                RecordDelimiter = ';',
                Identification = "identifier",
                FullFileName = @"C:\path\to\full\filename.igs",
                SystemIdentifier = "BCAD",
                SystemVersion = "1.0",
                IntegerSize = 16,
                SingleSize = 7,
                DecimalDigits = 22,
                DoubleMagnitude = 10,
                DoublePrecision = 51,
                Identifier = "ident2",
                ModelSpaceScale = 0.75,
                ModelUnits = IgesUnits.Centimeters,
                CustomModelUnits = null,
                MaxLineWeightGraduations = 4,
                MaxLineWeight = 0.8,
                TimeStamp = new DateTime(1983, 11, 23, 13, 8, 11),
                MinimumResolution = 0.001,
                MaxCoordinateValue = 500.0,
                Author = "Brett",
                Organization = "IxMilia",
                IgesVersion = IgesVersion.v5_0,
                DraftingStandard = IgesDraftingStandard.BSI,
                ModifiedTime = new DateTime(1987, 5, 8, 12, 34, 56),
                ApplicationProtocol = "protocol"
            };
            VerifyFileExactly(file, @"
                                                                        S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,0.001,500.,5HBrett, G      2
7HIxMilia,8,4,15H19870508.123456,8Hprotocol;                            G      3
S      1G      3D      0P      0                                        T      1
");
        }
    }
}
