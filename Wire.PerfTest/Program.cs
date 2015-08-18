using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Akka.Actor;
using Newtonsoft.Json;
using ProtoBuf;

namespace Wire.PerfTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Run this in Release mode with no debugger attached for correct numbers!!");
            Console.WriteLine();
            Console.WriteLine("Running cold");
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();
            SerializePoco();
            SerializePocoJsonNet();
            SerializePocoBinaryFormatter();
            SerializePocoAkka();
            Console.WriteLine();
            Console.WriteLine("Running hot");
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();
            SerializePoco();
            SerializePocoJsonNet();
            SerializePocoBinaryFormatter();
            SerializePocoAkka();
            Console.ReadLine();
        }

        private static Poco poco = new Poco
        {
            Age = 123,
            Name = "Hello"
        };

        private static void SerializePocoJsonNet()
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {

                JsonConvert.SerializeObject(poco);
            }
            sw.Stop();
            Console.WriteLine($"Json.NET:\t\t\t{sw.ElapsedMilliseconds}");
        }

        private static void SerializePocoProtoBufNet()
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize(stream, poco);
            }
            sw.Stop();
            Console.WriteLine($"Protobuf.NET:\t\t\t{sw.ElapsedMilliseconds}");
        }

        private static void SerializePocoBinaryFormatter()
        {
            var bf = new BinaryFormatter();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                bf.Serialize(stream, poco);
            }
            sw.Stop();
            Console.WriteLine($"BinaryFormatter\t\t\t{sw.ElapsedMilliseconds}");
        }

        private static void SerializePocoVersionInteolerant()
        {
            var serializer = new Serializer(new SerializerOptions(false));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();
                serializer.Serialize(poco, stream);
            }
            sw.Stop();
            Console.WriteLine($"Wire - no version tolerance:\t{sw.ElapsedMilliseconds}");
        }

        private static void SerializePoco()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                var stream = new MemoryStream();

                serializer.Serialize(poco, stream);
            }
            sw.Stop();
            Console.WriteLine($"Wire - version tolerant:\t{sw.ElapsedMilliseconds}");
        }

        private static void SerializePocoAkka()
        {
            var sys = ActorSystem.Create("foo");
            var s = sys.Serialization.FindSerializerForType(typeof (Poco));
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {

                s.ToBinary(poco);
            }
            sw.Stop();
            Console.WriteLine($"Akka.NET Json.NET settings:\t{sw.ElapsedMilliseconds}");
        }
    }


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