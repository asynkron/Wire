using System;
using System.Collections.Generic;
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
            SerializeDeserializeArray();
            SerializeDeserializeDictionary();
            SerializeDeserializeSurrogate();
            SerializeDeserialize2();
            Console.WriteLine("Run this in Release mode with no debugger attached for correct numbers!!");
            Console.WriteLine();
            Console.WriteLine("Running cold");
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();
            SerializePoco();
            SerializePocoJsonNet();
            //SerializePocoBinaryFormatter();
            //SerializePocoAkka();
            Console.WriteLine();
            Console.WriteLine("Running hot");
            start:
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();
            SerializePoco();
            SerializePocoJsonNet();
            //SerializePocoBinaryFormatter();
            //SerializePocoAkka();
            TestSerializerSingleValues();
            Console.WriteLine("Press ENTER to repeat.");
            Console.ReadLine();
            goto start;
        }

        private static readonly Poco poco = new Poco
        {
            Age = 123,
            Name = "Hello"
        };

        private static void SerializeDeserializeArray()
        {

            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(false));
            var array = new[] {new Poco(), new Poco2(), null, poco};
            serializer.Serialize(array, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Poco[]>(stream);
        }

        private static void SerializeDeserializeDictionary()
        {
            //TODO: fix this
            //var stream = new MemoryStream();
            //var serializer = new Serializer(new SerializerOptions(false));
            //serializer.Serialize(new Dictionary<string,Poco>()
            //{
            //    ["hello"] = poco
            //}, stream);
            //stream.Position = 0;
            //var res = serializer.Deserialize<Dictionary<string, Poco>>(stream);
        }

        private static void SerializeDeserializeSurrogate()
        {
            var surrogate = Surrogate.Create<Poco,PocoSurrogate>(
                p => new PocoSurrogate {Data = $"{p.Age}|{p.Name}"},
                s => s.Restore());
            
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(false,new [] {surrogate}));
            serializer.Serialize(poco,stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Poco>(stream);
        }

        private static void SerializeDeserialize2()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(true));
            serializer.Serialize(poco, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Poco>(stream);
        }

        private static void TestSerializerSingleValues()
        {
            Console.WriteLine("");
            Console.WriteLine("Testing individual ValueSerializers.");
            var serializer = new Serializer(new SerializerOptions(false));
            var stream = new MemoryStream(4096);
            Action<object> testSerialize = o =>
            {
                Console.Write("{0}: ", o.GetType().Name);
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < 1000000; i++)
                {
                    serializer.Serialize(o, stream);
                    stream.Position = 0;
                }
                sw.Stop();
                Console.WriteLine((double)sw.ElapsedTicks / 1000000);
            };
            testSerialize((byte)255);
            testSerialize((short)1234);
            testSerialize(12345679);
            testSerialize(123456789L);
            testSerialize(123.45f);
            testSerialize(123.45);
            testSerialize(123.45m);
            testSerialize(DateTime.UtcNow);
            testSerialize(new[] { 'a' });
            testSerialize(new byte[] { 0 });
            testSerialize("1");
            testSerialize(new Poco { Name = "a" });
            testSerialize(new Poco { Name = null });
        }

        private static void SerializePocoJsonNet()
        {

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {

                JsonConvert.SerializeObject(poco,new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
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


        //[ProtoMember(3)]
        //public string Age2 { get; set; } =
        //    "fklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjlklsdjkfl sjlfkjsdflsdj flksjl"
        //    ;


        //[ProtoMember(4)]
        //public int Age3 { get; set; }


        //[ProtoMember(5)]
        //public int Age4 { get; set; }


        //[ProtoMember(6)]
        //public int Age5 { get; set; }

        //[ProtoMember(3)]
        //public int? NullableInt { get; set; } = 2;
    }

    public class Poco2 : Poco
    {
        public bool Yes { get; set; }
    }

    public class PocoSurrogate
    {
        public string Data { get; set; }

        public Poco Restore()
        {
            var parts = Data.Split('|');
            return new Poco()
            {
                Age = int.Parse(parts[0]),
                Name = parts[1]
            };
        }
    }
}