using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Akka.Actor;

namespace Wire.PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
       //     SerializeStringArray();
            SerializePoco();
            SerializePocoAkka();
         //   SerializeLocoWithNullRef();
         //   SerializeLocoWithPolymorphicRef();
            Console.ReadLine();

        }

        private static void SerializeStringArray()
        {
            Serializer serializer = new Serializer();
                var stream = new MemoryStream();
            var strings = new[] { "abc", "def", null ,"ghi", "jkl", "lmmo" };
            serializer.Serialize(strings, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<string[]>(stream);

            stream = new MemoryStream();
            serializer.Serialize(strings.ToList(), stream);
            stream.Position = 0;
            var l = serializer.Deserialize<List<string>>(stream);

            foreach (var i in l)
            {
                Console.WriteLine(i);
            }
                //stream.Position = 0;
                //var res = serializer.Deserialize<Poco>(stream);
                //Console.WriteLine(res.Age);
                //Console.WriteLine(res.Name);
            
        }

        private static void SerializePoco()
        {

            Serializer serializer = new Serializer(new SerializerOptions(true));
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco()
                {
                    Age = 123,
                    Name = "Hej"
                };
                serializer.Serialize(poco, stream);
                var bytes = stream.ToArray();
                //stream.Position = 0;
                //var res = serializer.Deserialize<Poco>(stream);
                //Console.WriteLine(res.Age);
                //Console.WriteLine(res.Name);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void SerializePocoAkka()
        {
            var sys = ActorSystem.Create("foo");
            var s = sys.Serialization.FindSerializerForType(typeof (Poco));
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                var poco = new Poco()
                {
                    Age = 123,
                    Name = "Hej"
                };
                var bytes = s.ToBinary(poco);
                //stream.Position = 0;
                //var res = serializer.Deserialize<Poco>(stream);
                //Console.WriteLine(res.Age);
                //Console.WriteLine(res.Name);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void SerializeLocoWithNullRef()
        {
            Serializer serializer = new Serializer();
            
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                var loco = new Loco
                {
                    Poco = null,// poco,
                    YesNo = true,
                };
                serializer.Serialize(loco, stream);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void SerializeLocoWithPolymorphicRef()
        {
            Serializer serializer = new Serializer();

            //Stopwatch sw = Stopwatch.StartNew();
            //for (int i = 0; i < 100000; i++)
            //{
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                var loco = new Loco
                {
                    Poco = new Poco2
                    {
                        Age = 1232,
                        Name = "Wire",
                        Yes = true,
                    },
                    YesNo = true,
                };
                serializer.Serialize(loco, stream);
            //}
            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            stream.Flush();
            stream.Position = 0;
            var res = serializer.Deserialize<Loco>(stream);
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

    public class Poco2 : Poco
    {
        public bool Yes { get; set; }
    }
}
