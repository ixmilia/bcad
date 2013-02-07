using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Igs;
using BCad.Igs.Entities;
using Xunit;

namespace BCad.Test.IgsTests
{
    public class IgsEntityReaderTests
    {

        #region Private methods

        private static IgsEntity ParseSingleEntity(string content)
        {
            var file = IgsReaderTests.CreateFile(content.Trim('\r', '\n'));
            Assert.Equal(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [Fact]
        public void UnsupportedEntityReadTest()
        {
            // entity id 888 is invalid
            var file = IgsReaderTests.CreateFile(@"
     888       1       0       0       0                               0D      1
     888       0       0       0       0                               0D      2
888,11,22,33,44,55,66;                                                  P      1
".Trim('\r', '\n'));
            Assert.Equal(0, file.Entities.Count);
        }

        [Fact]
        public void LineReadTest()
        {
            var line = (IgsLine)ParseSingleEntity(@"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110,11,22,33,44,55,66;                                                  P      1
");
            Assert.Equal(11.0, line.X1);
            Assert.Equal(22.0, line.Y1);
            Assert.Equal(33.0, line.Z1);
            Assert.Equal(44.0, line.X2);
            Assert.Equal(55.0, line.Y2);
            Assert.Equal(66.0, line.Z2);
            Assert.Equal(IgsColorNumber.Color3, line.Color);
        }
    }
}
