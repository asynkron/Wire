using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Jil;
using Newtonsoft.Json;
using NFX.IO;
using NFX.Serialization.Slim;

namespace Wire.PerfTest.Tests
{
    class TestResult
    {
        public string TestName { get; set; }
        public TimeSpan SerializationTime { get; set; }
        public TimeSpan DeserializationTime { get; set; }
        public TimeSpan RoundtripTime => SerializationTime + DeserializationTime;
        public int PayloadSize { get; set; }
        public bool Success { get; set; }

    }
    internal abstract class TestBase<T>
    {
        private string _fastestDeserializer;
        private TimeSpan _fastestDeserializerTime = TimeSpan.MaxValue;

        private string _fastestSerializer;
        private TimeSpan _fastestSerializerTime = TimeSpan.MaxValue;

        private string _fastestRoundtrip;
        private TimeSpan _fastestRoundtripTime = TimeSpan.MaxValue;

        protected int Repeat;
        private string _smallestPayload;
        private int _smallestPayloadSize = int.MaxValue;
        protected T Value;

        private List<TestResult> _results = new List<TestResult>();

        protected abstract T GetValue();

        public void Run(int repeat)
        {
            Repeat = repeat;
            Value = GetValue();
            Console.WriteLine();
            var testName = GetType().Name;

            Console.WriteLine($"# Test {testName}");
            Console.WriteLine();
            Console.WriteLine("## Running cold");
            Console.WriteLine("```");
            SerializePreRegister();
            SerializeVersionInteolerant();
            Serialize();
            SerializeVersionInteolerantPreserveObjects();
            SerializeNFXSlim();
            SerializeNFXSlimPreregister();
            // SerializeFsPickler();
            SerializeJil();
            SerializeNetJson();
            SerializeNetSerializer();
            SerializeProtoBufNet();
            SerializeJsonNet();
            SerializeBinaryFormatter();
            Console.WriteLine("```");
            Console.WriteLine();
            Console.WriteLine("## Running hot");
            _results.Clear();
            Console.WriteLine("```");
            SerializePreRegister();
            SerializeVersionInteolerant();
            Serialize();
            SerializeVersionInteolerantPreserveObjects();
            SerializeNFXSlim();
            SerializeNFXSlimPreregister();
            //   SerializeFsPickler();
            SerializeJil();
            SerializeNetJson();
            SerializeNetSerializer();
            SerializeProtoBufNet();
            SerializeJsonNet();
            SerializeBinaryFormatter();
            Console.WriteLine("```");
            Console.WriteLine($"* **Fastest Serializer**: {_fastestSerializer} - {(long)_fastestSerializerTime.TotalMilliseconds} ms");
            Console.WriteLine($"* **Fastest Deserializer**: {_fastestDeserializer} - {(long)_fastestDeserializerTime.TotalMilliseconds} ms");
            Console.WriteLine($"* **Fastest Roundtrip**: {_fastestRoundtrip} - {(long)_fastestRoundtripTime.TotalMilliseconds} ms");
            Console.WriteLine($"* **Smallest Payload**: {_smallestPayload} - {_smallestPayloadSize} bytes");

            SaveTestResult($"{testName}_roundtrip.txt", _results.OrderBy(r => r.RoundtripTime.TotalMilliseconds));
            SaveTestResult($"{testName}_serialize.txt", _results.OrderBy(r => r.SerializationTime.TotalMilliseconds));
            SaveTestResult($"{testName}_deserialize.txt", _results.OrderBy(r => r.DeserializationTime.TotalMilliseconds));
            SaveTestResult($"{testName}_payload.txt", _results.OrderBy(r => r.PayloadSize));
        }

        private void SaveTestResult(string file, IEnumerable<TestResult> result )
        {
            var sb = new StringBuilder();
            sb.AppendLine("test, roundtrip, serialize, deserialize, size");
            foreach (var row in result)
            {
                sb.AppendLine(
                    $"{row.TestName}, {(long)row.RoundtripTime.TotalMilliseconds}, {(long) row.SerializationTime.TotalMilliseconds}, {(long) row.DeserializationTime.TotalMilliseconds}, {row.PayloadSize}");
            }

            File.WriteAllText(file, sb.ToString());

        }

        private void SerializeNetJson()
        {
            var s = new MemoryStream();
            var res = NetJSON.NetJSON.Serialize(Value);
            var size = Encoding.UTF8.GetBytes(res).Length;


            RunTest("NET-JSON", () =>
            {
                NetJSON.NetJSON.Serialize(Value);
            }, () => { NetJSON.NetJSON.Deserialize<T>(res); }, size);
        }

        private void SerializeJsonNet()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
                //ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                //PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            var data = JsonConvert.SerializeObject(Value, settings);
            RunTest("Json.NET", () => { JsonConvert.SerializeObject(Value, settings); }, () =>
            {
                JsonConvert.DeserializeObject(data, settings);
            }, Encoding.UTF8.GetBytes(data).Length);
        }

        private void RunTest(string testName, Action serialize, Action deserialize, int size)
        {
            try
            {
                if (size < _smallestPayloadSize && size > 0)
                {
                    _smallestPayloadSize = size;
                    _smallestPayload = testName;
                }

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
                if (sw.Elapsed < _fastestSerializerTime)
                {
                    _fastestSerializerTime = sw.Elapsed;
                    _fastestSerializer = testName;
                }
                Console.WriteLine($"   {"Serialize".PadRight(30, ' ')} {sw.ElapsedMilliseconds} ms");
                var sw2 = Stopwatch.StartNew();
                for (var i = 0; i < Repeat; i++)
                {
                    deserialize();
                }
                sw2.Stop();
                if (sw2.Elapsed < _fastestDeserializerTime)
                {
                    _fastestDeserializerTime = sw2.Elapsed;
                    _fastestDeserializer = testName;
                }
                if (sw2.Elapsed + sw.Elapsed < _fastestRoundtripTime)
                {
                    _fastestRoundtripTime = sw2.Elapsed + sw.Elapsed;
                    _fastestRoundtrip = testName;
                }
                Console.WriteLine($"   {"Deserialize".PadRight(30, ' ')} {sw2.ElapsedMilliseconds} ms");
                Console.WriteLine($"   {"Size".PadRight(30, ' ')} {size} bytes");
                Console.WriteLine(
                    $"   {"Total".PadRight(30, ' ')} {sw.ElapsedMilliseconds + sw2.ElapsedMilliseconds} ms");
                var testResult = new TestResult()
                {
                    TestName = testName,
                    DeserializationTime = sw2.Elapsed,
                    SerializationTime = sw.Elapsed,
                    PayloadSize = size,
                    Success = true
                };
                _results.Add(testResult);
            }
            catch
            {
                Console.WriteLine($"    FAILURE");
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

        private void SerializeNFXSlimPreregister()
        {
            var s = new MemoryStream();
            var serializer = new SlimSerializer(SlimFormat.Instance, new[] {typeof(T)});
            serializer.Serialize(s, Value);
            var bytes = s.ToArray();

            RunTest("NFX Slim Serializer - KnownTypes", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize(s);
            }, bytes.Length);
        }

        private void SerializeNFXSlim()
        {
            var s = new MemoryStream();
            var serializer = new SlimSerializer(SlimFormat.Instance);
            serializer.Serialize(s, Value);
            var bytes = s.ToArray();

            RunTest("NFX Slim Serializer", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize(s);
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
                JSON.Serialize(Value);
            }, () => { JSON.Deserialize(res, typeof(T)); }, bytes.Length);
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
            RunTest("Wire - KnownTypes", () =>
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
            RunTest("Wire - Default", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s);
            }, bytes.Length);
        }

        private void SerializeVersionInteolerantPreserveObjects()
        {
            var serializer = new Serializer(new SerializerOptions(false, true));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            RunTest("Wire - Object Identity", () =>
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
            RunTest("Wire - Version Tolerant", () =>
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