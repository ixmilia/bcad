using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCad.Dwg;
using Xunit;

namespace BCad.Test.DwgTests
{
    public class LoadTests
    {
        [Fact]
        public void LoadTest()
        {
            //var fileName = @"C:\Users\brettfo\Source\Repos\libredwg\examples\example.dwg";
            var fileName = @"C:\Users\brettfo\Desktop\brettfo2.dwg";
            var file = DwgFile.Load(new FileStream(fileName, FileMode.Open));
        }
    }
}
