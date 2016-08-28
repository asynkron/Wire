using Akka.Actor;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wire.FSharpTestTypes;
using Microsoft.FSharp.Quotations;
using Microsoft.FSharp.Control;

namespace Wire.Tests
{
    [TestClass]
    public class FSharpTests : TestBase
    {
        


        [TestMethod]
        public void CanSerializeFSharpList()
        {
            var expected = ListModule.OfArray(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
            Serialize(expected);
            Reset();
            var actual = Deserialize<object>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeSimpleDU()
        {
            var expected = DU1.NewA(1);
            Serialize(expected);
            Reset();
            var actual = Deserialize<object>();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void CanSerializeNestedDU()
        {
            var expected = DU2.NewC(DU1.NewA(1));
            Serialize(expected);
            Reset();
            var actual = Deserialize<object>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeOptionalDU()
        {
            var expected = DU2.NewE(FSharpOption<DU1>.Some(DU1.NewA(1)));
            Serialize(expected);
            Reset();
            var actual = Deserialize<object>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeOption()
        {
            var expected = FSharpOption<string>.Some("hello");
            Serialize(expected);
            Reset();
            var actual = Deserialize<object>();
            Assert.AreEqual(expected, actual);
        }

        public class FooActor : UntypedActor
        {
            protected override void OnReceive(object message)
            {                
            }
        }

        [TestMethod]
        public void CanSerializeUser()
        {
                var expected = new User("foo", new FSharpOption<string>(null), "hello");
                Serialize(expected);
                Reset();
                var actual = Deserialize<User>();
               // Assert.AreEqual(expected, actual);
                Assert.AreEqual(expected.aref, actual.aref);
                Assert.AreEqual(expected.name, actual.name);
                Assert.AreEqual(expected.connections, actual.connections);

        }

        //FIXME: make F# quotations and Async serializable
        //[TestMethod]
        public void CanSerializeQuotation()
        {
            var expected = TestQuotations.Quotation;
            Serialize(expected);
            Reset();
            var actual = Deserialize<FSharpExpr<FSharpFunc<int, FSharpAsync<int>>>>();
            // Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
