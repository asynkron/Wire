using System;
using System.Diagnostics;
using System.IO;

namespace Wire.PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SerializePoco();
            SerializeLoco();
            Console.ReadLine();
        }

        private static void SerializePoco()
        {
            Serializer serializer = new Serializer();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 3000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco()
                {
                    Age = 123,
                    Name = "Hej"
                };
                serializer.Serialize(poco, stream);

                //stream.Position = 0;
                //var res = serializer.Deserialize<Poco>(stream);
                //Console.WriteLine(res.Age);
                //Console.WriteLine(res.Name);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void SerializeLoco()
        {
            Serializer serializer = new Serializer();
            
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 3000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                var loco = new Loco
                {
                    Poco = poco,
                    YesNo = true,
                };
                serializer.Serialize(loco, stream);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
    }


    //0 = no manifest
    //1 = typename

    public class Loco
    {
        public bool YesNo { get; set; }
        public Poco Poco { get; set; }
    }

    public class Poco
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
