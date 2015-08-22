using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wire.FSharpTestTypes;

namespace Wire.Tests
{
    [TestClass]
    public class FSharpTests : TestBase
    {
        [TestMethod]
        public void CanSerializeSimpleDU()
        {
            var expected = DU1.NewA(1);
            Serialize(expected);
            Reset();
            var actual = Deserialize<DU1>();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void CanSerializeNestedDU()
        {
            var expected = DU2.NewC(DU1.NewA(1));
            Serialize(expected);
            Reset();
            var actual = Deserialize<DU2>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeOptionalDU()
        {
            var expected = DU2.NewE(FSharpOption<DU1>.Some(DU1.NewA(1)));
            Serialize(expected);
            Reset();
            var actual = Deserialize<DU2>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeOption()
        {
            var expected = FSharpOption<string>.Some("hello");
            Serialize(expected);
            Reset();
            var actual = Deserialize<FSharpOption<string>>();
            Assert.AreEqual(expected, actual);
        }
    }
}
