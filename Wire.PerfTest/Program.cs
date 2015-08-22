using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Akka.Actor;
using Newtonsoft.Json;
using ProtoBuf;

namespace Wire.PerfTest
{
    internal class Program
    {
        private static readonly Poco poco = new Poco
        {
            Age = 123,
            Name = "Hello",
            Foo = "kfksdjfksjdfkjsdkfjskfjksd",
            Yoo = DateTime.Now
        };

        private static void Main(string[] args)
        {
            SerializePocoVersionInteolerant();
          //  return;
            Console.WriteLine("Run this in Release mode with no debugger attached for correct numbers!!");
            Console.WriteLine();
            //DeserializeOnly();
            //SerializePocoVersionInteolerant();
            Console.WriteLine("Running cold");
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();

            SerializePoco();
            SerializePocoVersionInteolerantPreserveObjects();
            SerializePocoJsonNet();
            SerializePocoBinaryFormatter();
            SerializePocoAkka();
            Console.WriteLine();
            Console.WriteLine("Running hot");
            start:
            SerializePocoVersionInteolerant();
            SerializePocoProtoBufNet();
            SerializePoco();
            SerializePocoVersionInteolerantPreserveObjects();
            SerializePocoJsonNet();
            SerializePocoBinaryFormatter();
            SerializePocoAkka();
        //    TestSerializerSingleValues();
        //    Console.WriteLine("Press ENTER to repeat.");
            Console.ReadLine();
          //  goto start;
        }


        //private static void DeserializeOnly()
        //{
        //    var serializer = new Serializer(new SerializerOptions(false));         
        //    var s = new MemoryStream();
        //    serializer.Serialize(poco, s);
        //    RunTest("Wire - no version data", () =>
        //    {
        //        s.Position = 0;
        //        var p = serializer.Deserialize<Poco>(s);
        //    });
        //}

        //private static void TestSerializerSingleValues()
        //{
        //    Console.WriteLine("");
        //    Console.WriteLine("Testing individual ValueSerializers.");
        //    var serializer = new Serializer(new SerializerOptions(false));
        //    var stream = new MemoryStream(4096);
        //    Action<object> testSerialize = o =>
        //    {
        //        Console.Write("{0}: ", o.GetType().Name);
        //        var sw = Stopwatch.StartNew();
        //        for (var i = 0; i < 1000000; i++)
        //        {
        //            serializer.Serialize(o, stream);
        //            stream.Position = 0;
        //        }
        //        sw.Stop();
        //        Console.WriteLine((double) sw.ElapsedTicks/1000000);
        //    };
        //    testSerialize((byte) 255);
        //    testSerialize((short) 1234);
        //    testSerialize(12345679);
        //    testSerialize(123456789L);
        //    testSerialize(123.45f);
        //    testSerialize(123.45);
        //    testSerialize(123.45m);
        //    testSerialize(DateTime.UtcNow);
        //    testSerialize(new[] {'a'});
        //    testSerialize(new byte[] {0});
        //    testSerialize("1");
        //    testSerialize(new Poco {Name = "a"});
        //    testSerialize(new Poco {Name = null});
        //}

        private static void SerializePocoJsonNet()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            var data = JsonConvert.SerializeObject(poco, settings);
            RunTest("Json.NET", () =>
            {
                JsonConvert.SerializeObject(poco, settings);
            }, () =>
            {
                var o = JsonConvert.DeserializeObject(data, settings);
            });
        }

        private static void RunTest(string testName, Action serialize, Action deserialize)
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{testName}");
            Console.ForegroundColor = tmp;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                serialize();
            }
            sw.Stop();
            Console.WriteLine($"   {"Serialize".PadRight(30,' ')} {sw.ElapsedMilliseconds}");
            sw = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                deserialize();
            }
            sw.Stop();
            Console.WriteLine($"   {"Deseralize".PadRight(30, ' ')} {sw.ElapsedMilliseconds}");
        }

        private static void SerializePocoProtoBufNet()
        {
            var s = new MemoryStream();
            ProtoBuf.Serializer.Serialize(s, poco);
            RunTest("Protobuf.NET", () =>
            {
                var stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize(stream, poco);
            }, () =>
            {
                s.Position = 0;
                ProtoBuf.Serializer.Deserialize<Poco>(s);
            });
        }

        private static void SerializePocoBinaryFormatter()
        {
            var bf = new BinaryFormatter();
            var s = new MemoryStream();
            bf.Serialize(s, poco);
            RunTest("Binary formatter", () =>
            {
                var stream = new MemoryStream();
                bf.Serialize(stream, poco);
            }, () =>
            {
                s.Position = 0;
                var o = bf.Deserialize(s);
            });
        }

        private static void SerializePocoVersionInteolerant()
        {
            var serializer = new Serializer(new SerializerOptions(false));
            var s = new MemoryStream();
            serializer.Serialize(poco, s);
            RunTest("Wire - no version data", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            });
        }

        private static void SerializePocoVersionInteolerantPreserveObjects()
        {
            var serializer = new Serializer(new SerializerOptions(false, preserveObjectReferences: true));
            var s = new MemoryStream();
            serializer.Serialize(poco, s);
            RunTest("Wire - preserve object refs", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            });
        }

        private static void SerializePoco()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var s = new MemoryStream();
            serializer.Serialize(poco, s);
            RunTest("Wire - version tolerant", () =>
            {
                var stream = new MemoryStream();

                serializer.Serialize(poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            });
        }

        private static void SerializePocoAkka()
        {
            var sys = ActorSystem.Create("foo");
            var s = sys.Serialization.FindSerializerForType(typeof (Poco));
            var data = s.ToBinary(poco);
            RunTest("Akka.NET Json.NET settings", () =>
            {
                s.ToBinary(poco);
            }, () =>
            {
                s.FromBinary(data, typeof (Poco));
            });
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

        [ProtoMember(3)]
        public string Foo { get; set; }

        [ProtoMember(4)]
        public DateTime Yoo { get; set; }
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
            return new Poco
            {
                Age = int.Parse(parts[0]),
                Name = parts[1]
            };
        }
    }

    public class Lista : IEnumerable<string>
    {
        public List<string> List { get; set; }
        public string Name { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public void AddRange(IEnumerable<string> data)
        {
            List.AddRange(data);
        }
    }
}