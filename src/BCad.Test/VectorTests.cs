using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BCad.Test
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void NormalizeTest()
        {
            var v = new Vector(3, 4, 0);
            Assert.AreEqual(1.0, v.Normalize().Length);
        }
    }
}
