using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Akka.Actor;
using ProtoBuf;

namespace Wire.PerfTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SerializePoco();
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();

            SerializePocoAkka();
            SerializePocoBinaryFormatter();
            Console.ReadLine();
        }

        private static void SerializePocoProtoBufNet()
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                ProtoBuf.Serializer.Serialize(stream, poco);
                var bytes = stream.ToArray();
                //stream.Position = 0;
                //var res = serializer.Deserialize<Poco>(stream);
                //Console.WriteLine(res.Age);
                //Console.WriteLine(res.Name);
            }
            sw.Stop();
            Console.WriteLine($"Protobuf.NET:\t\t\t{sw.Elapsed}");
        }

        private static void SerializePocoBinaryFormatter()
        {
            var bf = new BinaryFormatter();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                bf.Serialize(stream, poco);
            }
            sw.Stop();
            Console.WriteLine($"BinaryFormatter\t\t\t{sw.Elapsed}");
        }

        private static void SerializePocoVersionInteolerant()
        {
            var serializer = new Serializer(new SerializerOptions(false));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                serializer.Serialize(poco, stream);
            }
            sw.Stop();
            Console.WriteLine($"Wire - no version tolerance:\t{sw.Elapsed}");
        }

        private static void SerializePoco()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                serializer.Serialize(poco, stream);
            }
            sw.Stop();
            Console.WriteLine($"Wire - version tolerant:\t{sw.Elapsed}");
        }

        private static void SerializePocoAkka()
        {
            var sys = ActorSystem.Create("foo");
            var s = sys.Serialization.FindSerializerForType(typeof (Poco));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var poco = new Poco
                {
                    Age = 123,
                    Name = "Hej"
                };
                s.ToBinary(poco);
            }
            sw.Stop();
            Console.WriteLine($"Akka.NET Json.NET settings:\t{sw.Elapsed}");
        }
    }


    //0 = no manifest
    //1 = typename

    public class Loco
    {
        public bool YesNo { get; set; }
        public Poco Poco { get; set; }
    }

    [ProtoContract]
    [Serializable]
    public class Poco
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int Age { get; set; }
    }

    public class Poco2 : Poco
    {
        public bool Yes { get; set; }
    }
}