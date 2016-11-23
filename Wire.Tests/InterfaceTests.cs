using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Wire.Tests
{
   
    public class InterfaceTests
    {
        public class Bar
        {
            public IFoo Foo { get; set; }
        }

        public interface IFoo
        {
            int A { get; set; }
            string B { get; set; }
        }
        public class Foo : IFoo
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        [Fact]
        public void CanSerializeInterfaceField()
        {
            var b = new Bar
            {
                Foo = new Foo()
                {
                    A = 123,
                    B = "hello"
                }
            };
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());
            serializer.Serialize(b, stream);
            stream.Position = 0;
            var res = serializer.Deserialize(stream);
        }
    }
}
