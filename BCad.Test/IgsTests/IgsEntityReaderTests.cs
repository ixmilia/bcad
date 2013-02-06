using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Igs.Entities;
using Xunit;

namespace BCad.Test.IgsTests
{
    public class IgsEntityReaderTests
    {

        #region Private methods

        private static IgsEntity ParseSingleEntity(string directoryText, string parameterText)
        {
            var file = IgsReaderTests.CreateFile(string.Format(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4HBCAD,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H19831123.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,8Hprotocol;                             G      3
{0}
{1}
", directoryText.Trim(), parameterText.Trim()));
            Assert.Equal(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [Fact]
        public void LineReadTest()
        {
            var line = (IgsLine)ParseSingleEntity(@"
00000110       1       0       0       0       0       0       0       0D      1
     110       1       0       0       0       0       0       0       0D      2
",
@"
110,11,22,33,44,55,66;                                                  P      1
");
            Assert.Equal(11.0, line.X1);
            Assert.Equal(22.0, line.Y1);
            Assert.Equal(33.0, line.Z1);
            Assert.Equal(44.0, line.X2);
            Assert.Equal(55.0, line.Y2);
            Assert.Equal(66.0, line.Z2);
        }
    }
}
