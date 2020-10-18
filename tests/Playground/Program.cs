using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Wire;
using Wire.Buffers;
using Wire.Buffers.Adaptors;

namespace Playground
{
    
    public class TypicalMessage
    {
       public  string StringProp { get; set; }

        public  int IntProp { get; set; }

        public  Guid GuidProp { get; set; }

        public  DateTime DateProp { get; set; }
    }
    
    public sealed class SomeClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public SomeOther Prop3 { get; set; }
    }

    public sealed class SomeOther
    {
        public bool BoolProp { get; set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Serializer(new SerializerOptions(
                knownTypes:new List<Type> {typeof(SomeClass),typeof(SomeOther),typeof(TypicalMessage)})
            );
            var some = new SomeClass
            {
                Prop1 = "hello",
                Prop2 = 123,
                Prop3 = new SomeOther
                {
                    BoolProp = false
                }
            };

            var msg = new TypicalMessage()
            {
                DateProp = DateTime.Now,
                GuidProp = Guid.NewGuid(),
                IntProp = 12,
                StringProp = "fskdjfksjkfjsdklfjslkfjslkfjslkfjsldkjflsj"
            };
            // var ms = new MemoryStream();
            // var bufferWriter = new MemoryStreamBufferWriter(ms);

            var bytes = new byte[20000];


            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 50_000_000; i++)
            {
                var bufferWriter = new SingleSegmentBuffer(bytes);
                s.Serialize(msg,bufferWriter);    
            }
            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            

          //  var bytes = ms.ToArray();
        }
    }
}