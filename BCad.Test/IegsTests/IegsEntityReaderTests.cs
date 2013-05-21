using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Iegs;
using BCad.Iegs.Entities;
using Xunit;

namespace BCad.Test.IegsTests
{
    public class IegsEntityReaderTests
    {

        #region Private methods

        private static IegsEntity ParseSingleEntity(string content)
        {
            var file = IegsReaderTests.CreateFile(content.Trim('\r', '\n'));
            Assert.Equal(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [Fact]
        public void UnsupportedEntityReadTest()
        {
            // entity id 888 is invalid
            var file = IegsReaderTests.CreateFile(@"
     888       1       0       0       0                               0D      1
     888       0       0       0       0                               0D      2
888,11,22,33,44,55,66;                                                  P      1
".Trim('\r', '\n'));
            Assert.Equal(0, file.Entities.Count);
        }

        [Fact]
        public void LineReadTest()
        {
            var line = (IegsLine)ParseSingleEntity(@"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110,11,22,33,44,55,66;                                                  P      1
");
            Assert.Equal(11.0, line.P1.X);
            Assert.Equal(22.0, line.P1.Y);
            Assert.Equal(33.0, line.P1.Z);
            Assert.Equal(44.0, line.P2.X);
            Assert.Equal(55.0, line.P2.Y);
            Assert.Equal(66.0, line.P2.Z);
            Assert.Equal(IegsColorNumber.Color3, line.Color);
        }
    }
}
