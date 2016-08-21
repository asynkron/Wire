using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Jil;
using Newtonsoft.Json;

namespace Wire.PerfTest.Tests
{
    internal abstract class TestBase<T>
    {
        protected T Value;

        protected abstract T GetValue();

        protected int Repeat;

        public void Run(int repeat)
        {
            Repeat = repeat;
            Value = GetValue();

            Console.WriteLine("Run this in Release mode with no debugger attached for correct numbers!!");
            Console.WriteLine();
            Console.WriteLine("Running cold");

            // for (int i = 0; i < 20; i++)
            // {
            //     SerializePreRegister();
            // }
            //return;

            SerializePreRegister();
            SerializeVersionInteolerant();
            Serialize();
            SerializeVersionInteolerantPreserveObjects();
           // SerializeFsPickler();
            SerializeJil();
            SerializeNetSerializer();
            SerializeProtoBufNet();
            SerializeJsonNet();
            SerializeBinaryFormatter();
            Console.WriteLine();
            Console.WriteLine("Running hot");
            SerializePreRegister();
            SerializeVersionInteolerant();
            Serialize();
            SerializeVersionInteolerantPreserveObjects();

         //   SerializeFsPickler();
            SerializeJil();
            SerializeNetSerializer();
            SerializeProtoBufNet();
            SerializeJsonNet();
            SerializeBinaryFormatter();
        }

        private void SerializeJsonNet()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                //ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                //PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            var data = JsonConvert.SerializeObject(Value, settings);
            RunTest("Json.NET", () => { JsonConvert.SerializeObject(Value, settings); }, () =>
            {
                var o = JsonConvert.DeserializeObject(data, settings);
            }, Encoding.UTF8.GetBytes(data).Length);
        }

        private void RunTest(string testName, Action serialize, Action deserialize, int size)
        {
            try
            {
                var tmp = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{testName}");
                Console.ForegroundColor = tmp;
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Repeat; i++)
                {
                    serialize();
                }
                sw.Stop();
                Console.WriteLine($"   {"Serialize".PadRight(30, ' ')} {sw.ElapsedMilliseconds} ms");
                var sw2 = Stopwatch.StartNew();
                for (var i = 0; i < Repeat; i++)
                {
                    deserialize();
                }
                sw2.Stop();
                Console.WriteLine($"   {"Deserialize".PadRight(30, ' ')} {sw2.ElapsedMilliseconds} ms");
                Console.WriteLine($"   {"Size".PadRight(30, ' ')} {size} bytes");
                Console.WriteLine(
                    $"   {"Total".PadRight(30, ' ')} {sw.ElapsedMilliseconds + sw2.ElapsedMilliseconds} ms");
            }
            catch
            {
                Console.WriteLine($"{testName} failed");
            }
        }

        private void SerializeProtoBufNet()
        {
            var s = new MemoryStream();
            ProtoBuf.Serializer.Serialize(s, Value);
            var bytes = s.ToArray();
            RunTest("Protobuf.NET", () =>
            {
                var stream = new MemoryStream();
                ProtoBuf.Serializer.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                ProtoBuf.Serializer.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void SerializeNetSerializer()
        {
            var s = new MemoryStream();
            var serializer = new NetSerializer.Serializer(new[] {typeof(T)});
            serializer.Serialize(s, Value);
            var bytes = s.ToArray();

            RunTest("Net Serializer", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize(s);
            }, bytes.Length);
        }

        private void SerializeJil()
        {
            var s = new MemoryStream();
            var res = JSON.Serialize(Value);

            var bytes = s.ToArray();
            RunTest("Jil", () =>
            {
                var stream = new MemoryStream();
                JSON.Serialize(Value);
            }, () => { JSON.Deserialize(res, typeof(T)); }, bytes.Length);
        }

        private void SerializeBinaryFormatter()
        {
            var bf = new BinaryFormatter();
            var s = new MemoryStream();

            bf.Serialize(s, Value);
            var bytes = s.ToArray();
            RunTest("Binary formatter", () =>
            {
                var stream = new MemoryStream();
                bf.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                var o = bf.Deserialize(s);
            }, bytes.Length);
        }

        private void SerializePreRegister()
        {
            var serializer = new Serializer(new SerializerOptions(knownTypes: new[] {typeof(T)}));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            RunTest("Wire - preregister types", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void SerializeVersionInteolerant()
        {
            var serializer = new Serializer(new SerializerOptions(false));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            RunTest("Wire - no version data", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void SerializeFsPickler()
        {
            var pickler = MBrace.FsPickler.FsPickler.CreateBinarySerializer();
            var s = new MemoryStream();
            pickler.Serialize(s, Value);
            var bytes = s.ToArray();
            RunTest("FsPickler", () =>
            {
                var stream = new MemoryStream();
                pickler.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                pickler.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void SerializeVersionInteolerantPreserveObjects()
        {
            var serializer = new Serializer(new SerializerOptions(false, true));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            RunTest("Wire - preserve object refs", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void Serialize()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            RunTest("Wire - version tolerant", () =>
            {
                var stream = new MemoryStream();

                serializer.Serialize(Value, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s);
            }, bytes.Length);
        }
    }
}