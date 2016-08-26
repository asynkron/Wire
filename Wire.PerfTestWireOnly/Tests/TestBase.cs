using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


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

            for (int i = 0; i < 100; i++)
            {
                SerializeKnownTypesReuseSession();
                //SerializeKnownTypes();
                //SerializeNetSerializer();
            }


            Console.WriteLine("## Running cold");
            Console.WriteLine("```");
            SerializeKnownTypesReuseSession();
            SerializeKnownTypes();
            SerializeDefault();
            SerializeVersionTolerant();
            SerializeVersionPreserveObjects();
            Console.WriteLine("```");
            Console.WriteLine();
            Console.WriteLine("## Running hot");
            _results.Clear();
            Console.WriteLine("```");
            SerializeKnownTypesReuseSession();
            SerializeKnownTypes();
            SerializeDefault();
            SerializeVersionTolerant();
            SerializeVersionPreserveObjects();
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

        private void SerializeKnownTypesReuseSession()
        {
            var types = typeof(T).Namespace.StartsWith("System") ? null : new[] { typeof(T) };
            var serializer = new Serializer(new SerializerOptions(knownTypes: types));
            var ss = serializer.GetSerializerSession();
            var ds = serializer.GetDeserializerSession();
            var s = new MemoryStream();
            serializer.Serialize(Value, s,ss);
            var bytes = s.ToArray();
            RunTest("Wire - KnownTypes, Reuse Sessions", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream,ss);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s,ds);
            }, bytes.Length);
        }

        private void SerializeKnownTypes()
        {
            var types = typeof(T).Namespace.StartsWith("System")? null: new[] {typeof(T)};
            var serializer = new Serializer(new SerializerOptions(knownTypes: types));
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

        private void SerializeDefault()
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

        private void SerializeVersionPreserveObjects()
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

        private void SerializeVersionTolerant()
        {
            var serializer = new Serializer(new SerializerOptions(true));
            var s = new MemoryStream();
            serializer.Serialize(Value, s);
            var bytes = s.ToArray();
            s.Position = 0;
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
