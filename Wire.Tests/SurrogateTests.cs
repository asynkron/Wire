using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class SurrogateTests
    {
        [TestMethod]
        public void CanSerializeWithSurrogate()
        {
            bool surrogateHasBeenInvoked = false;
            var surrogates = new[]
            {
                Surrogate.Create<Foo, FooSurrogate>(FooSurrogate.FromFoo, surrogate =>
                {
                    surrogateHasBeenInvoked = true;
                    return surrogate.Restore();
                }),
            };
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(surrogates: surrogates));
            var foo = new Foo()
            {
                Bar = "I will be replaced!"
            };

            serializer.Serialize(foo,stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Foo>(stream);
            Assert.AreEqual(foo.Bar,actual.Bar);
            Assert.IsTrue(surrogateHasBeenInvoked);
        }
    }
}
