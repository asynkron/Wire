using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Newtonsoft.Json;
using ProtoBuf;
using Wire.SerializerFactories;
using Wire.ValueSerializers;

namespace Wire.PerfTest
{
   internal class Program
    {
        private static readonly Poco Poco = new Poco
        {
            IntProp = 123,
            StringProp = "Hello",
            GuidProp = Guid.NewGuid(),
            DateProp = DateTime.Now
        };

        private static void Main(string[] args)
        {
            Console.WriteLine("Run this in Release mode with no debugger attached for correct numbers!!");
            Console.WriteLine();
            Console.WriteLine("Running cold");

            SerializePocoPreRegister();
            SerializePocoPreRegisterManualSerializer();
            //SerializePocoVersionInteolerant();
            //SerializePoco();
            ////SerializePocoVersionInteolerantPreserveObjects();

            SerializePocoProtoBufNet();
            SerializePocoBond();
            ////////SerializePocoJsonNet();
            ////////SerializePocoBinaryFormatter();
            ////Console.WriteLine();
            ////Console.WriteLine("Running hot");
            SerializePocoPreRegister();
            SerializePocoPreRegisterManualSerializer();
            //SerializePocoVersionInteolerant();
            ////SerializePoco();
            //////SerializePocoVersionInteolerantPreserveObjects();

            SerializePocoProtoBufNet();
            SerializePocoBond();
            ////////SerializePocoJsonNet();
            ////////SerializePocoBinaryFormatter();
            //Console.ReadLine();
        }

        private static void SerializePocoJsonNet()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            var data = JsonConvert.SerializeObject(Poco, settings);
            RunTest("Json.NET", () => { JsonConvert.SerializeObject(Poco, settings); }, () =>
            {
                var o = JsonConvert.DeserializeObject(data, settings);
            }, Encoding.UTF8.GetBytes(data).Length);
        }

        private static void RunTest(string testName, Action serialize, Action deserialize, int size)
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
            Console.WriteLine($"   {"Serialize".PadRight(30, ' ')} {sw.ElapsedMilliseconds} ms");
            var sw2 = Stopwatch.StartNew();
            for (var i = 0; i < 1000000; i++)
            {
                deserialize();
            }
            sw2.Stop();
            Console.WriteLine($"   {"Deserialize".PadRight(30, ' ')} {sw2.ElapsedMilliseconds} ms");
            Console.WriteLine($"   {"Size".PadRight(30, ' ')} {size} bytes");
            Console.WriteLine($"   {"Total".PadRight(30, ' ')} {sw.ElapsedMilliseconds + sw2.ElapsedMilliseconds} ms");
        }

        private static void SerializePocoBond()
        {
            var poco = new Poco2()
            {
                StringProp = "Hello",
                IntProp = 123,
                DateProp = DateTime.UtcNow.Ticks,
                GuidProp = Guid.NewGuid(),
                DateTimeKind = (byte)DateTimeKind.Utc
            };

            var serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(Poco2));
            var deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(typeof(Poco2));

            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);
            
            Serialize.To(writer, poco);
            var bytes = output.Data.ToArray();

            //this is not a fair comparison, 
            var output2 = new OutputBuffer();
            RunTest("Bond", () =>
            {
                
                var writer2 = new CompactBinaryWriter<OutputBuffer>(output2);
                serializer.Serialize(poco,writer2);
            }, () =>
            {
                var input = new InputBuffer(output.Data);
                var reader = new CompactBinaryReader<InputBuffer>(input);
                deserializer.Deserialize<Poco2>(reader);
            }, bytes.Length);
        }

        private static void SerializePocoProtoBufNet()
        {
            var s = new MemoryStream();
            ProtoBuf.Serializer.Serialize(s, Poco);
            var bytes = s.ToArray();
            RunTest("Protobuf.NET", () =>
            {
                var stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize(stream, Poco);
            }, () =>
            {
                s.Position = 0;
                ProtoBuf.Serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }

        private static void SerializePocoBinaryFormatter()
        {
            var bf = new BinaryFormatter();
            var s = new MemoryStream();

            bf.Serialize(s, Poco);
            var bytes = s.ToArray();
            RunTest("Binary formatter", () =>
            {
                var stream = new MemoryStream();
                bf.Serialize(stream, Poco);
            }, () =>
            {
                s.Position = 0;
                var o = bf.Deserialize(s);
            }, bytes.Length);
        }

        private static void SerializePocoPreRegister()
        {
            var serializer = new Serializer(new SerializerOptions(knownTypes: new[] {typeof(Poco)}));
            var s = new MemoryStream();
            serializer.Serialize(Poco, s);
            var bytes = s.ToArray();
            RunTest("Wire - preregister types", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }

        private class PocoSerializerFactory : ValueSerializerFactory
        {
            public override bool CanSerialize(Serializer serializer, Type type) => type == typeof(Poco);
            public override bool CanDeserialize(Serializer serializer, Type type) => CanSerialize(serializer, type);

            public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
            {
                var s = new ObjectSerializer(type);
                if (typeMapping.TryAdd(type, s))
                {
                    var stringS = StringSerializer.Instance;
                    var intS = Int32Serializer.Instance;
                    var guidS = GuidSerializer.Instance;
                    var dateS = DateTimeSerializer.Instance;

                    s.Initialize(
                        (stream, session) =>
                        {
                            var poco = new Poco();//  (Poco)typeof(Poco).GetEmptyObject();
                            poco.StringProp = (string) stringS.ReadValue(stream, session);
                            poco.IntProp = (int) intS.ReadValue(stream, session);
                            poco.GuidProp = (Guid) guidS.ReadValue(stream, session);
                            poco.DateProp = (DateTime) dateS.ReadValue(stream, session);
                            return poco;
                        },
                        (stream, obj, session) =>
                        {
                            var poco = (Poco)obj;
                            stringS.WriteValue(stream, poco.StringProp, session);
                            intS.WriteValue(stream, poco.IntProp, session);
                            guidS.WriteValue(stream, poco.GuidProp, session);
                            dateS.WriteValue(stream, poco.DateProp, session);
                        });
                }
                return s;
            }
        }

        private static void SerializePocoPreRegisterManualSerializer()
        {
            var serializer = new Serializer(new SerializerOptions(
                knownTypes: new[] { typeof(Poco) },
                serializerFactories: new[] { new PocoSerializerFactory() }));
            var s = new MemoryStream();
            serializer.Serialize(Poco, s);
            var bytes = s.ToArray();
            RunTest("Wire - preregister types (manual serializer)", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }

        private static void SerializePocoVersionInteolerant()
        {
            var serializer = new Serializer(new SerializerOptions(false));
            var s = new MemoryStream();
            serializer.Serialize(Poco, s);
            var bytes = s.ToArray();
            RunTest("Wire - no version data", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }

        private static void SerializePocoVersionInteolerantPreserveObjects()
        {
            var serializer = new Serializer(new SerializerOptions(false, true));
            var s = new MemoryStream();
            serializer.Serialize(Poco, s);
            var bytes = s.ToArray();
            RunTest("Wire - preserve object refs", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }

        private static void SerializePoco()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var s = new MemoryStream();
            serializer.Serialize(Poco, s);
            var bytes = s.ToArray();
            RunTest("Wire - version tolerant", () =>
            {
                var stream = new MemoryStream();

                serializer.Serialize(Poco, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<Poco>(s);
            }, bytes.Length);
        }
    }


    public class Loco
    {
        public bool YesNo { get; set; }
        public Poco Poco { get; set; }
    }

    [ProtoContract]
    [Serializable]
    [Schema]
    public class Poco
    {
        [Id(0), Required]
        [ProtoMember(1)]
        public string StringProp { get; set; }

        [Id(1), Required]
        [ProtoMember(2)]
        public int IntProp { get; set; }

        [Id(2), Required]
        [ProtoMember(3)]
        public Guid GuidProp { get; set; }

        [Id(3), Required]
        [ProtoMember(4)]
        public DateTime DateProp { get; set; }
    }

    [Schema]
    public class Poco2
    {
        [Id(0), Required]
        public string StringProp { get; set; }

        [Id(1), Required]
        public int IntProp { get; set; }

        [Id(2), Required]
        public GUID GuidProp { get; set; }

        [Id(3), Required]
        public long DateProp { get; set; }

        [Id(4), Required]
        public byte DateTimeKind { get; set; }
    }

    public class PocoSurrogate
    {
        public string Data { get; set; }

        public Poco Restore()
        {
            var parts = Data.Split('|');
            return new Poco
            {
                IntProp = int.Parse(parts[0]),
                StringProp = parts[1]
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