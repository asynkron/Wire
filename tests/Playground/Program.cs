using System;
using System.IO;
using Wire;

namespace Playground
{

    public class SomeClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public SomeOther Prop3 { get; set; }
    }

    public class SomeOther
    {
        public bool BoolProp { get; set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Serializer();
            var some = new SomeClass
            {
                Prop1 = "hello",
                Prop2 = 123,
                Prop3 = new SomeOther
                {
                    BoolProp = false
                }
            };
            var stream = new MemoryStream();
            s.Serialize(some,stream);
        }
    }
}