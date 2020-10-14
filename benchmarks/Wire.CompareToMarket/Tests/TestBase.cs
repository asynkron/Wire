// -----------------------------------------------------------------------
//   <copyright file="TestBase.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Hagar;
using Hagar.Serializers;
using Hagar.Session;
using Newtonsoft.Json;
using NFX.IO;
using NFX.Serialization.Slim;
using ServiceStack.Text;

namespace Wire.PerfTest.Tests
{
    //Commented out serializers fail in different ways, throw or produce no output

    internal abstract class TestBase<T>
    {
        private readonly List<TestResult> _results = new List<TestResult>();
        protected int Repeat;
        protected T Value;

        protected abstract T GetValue();

        public void Run(int repeat)
        {
            Repeat = repeat;
            Value = GetValue();
            Console.WriteLine();
            var testName = GetType().Name;

            //for (int i = 0; i < 100; i++)
            //{
            //    SerializeKnownTypesReuseSession();
            //    SerializeKnownTypes();
            //    SerializeNetSerializer();
            //}

            Console.WriteLine($"# Test {testName}");
            Console.WriteLine();
            Console.WriteLine("## Running cold");
            Console.WriteLine("```");
            TestAll();
            Console.WriteLine("```");
            Console.WriteLine();
            Console.WriteLine("## Running hot");
            _results.Clear();
            Console.WriteLine("```");
            TestAll();
            Console.WriteLine("```");

            var fastestSerializer = _results.OrderBy(r => r.SerializationTime).First();
            var fastestDeserializer = _results.OrderBy(r => r.DeserializationTime).First();
            var fastestRoundtrip = _results.OrderBy(r => r.SerializationTime + r.DeserializationTime).First();
            var smallestPayload = _results.OrderBy(r => r.PayloadSize).First();

            Console.WriteLine(
                $"* **Fastest Serializer**: {fastestSerializer.TestName} - {(long) fastestSerializer.SerializationTime.TotalMilliseconds} ms");
            Console.WriteLine(
                $"* **Fastest Deserializer**: {fastestDeserializer.TestName} - {(long) fastestDeserializer.DeserializationTime.TotalMilliseconds} ms");
            Console.WriteLine(
                $"* **Fastest Roundtrip**: {fastestRoundtrip.TestName} - {(long) fastestRoundtrip.RoundtripTime.TotalMilliseconds} ms");
            Console.WriteLine(
                $"* **Smallest Payload**: {smallestPayload.TestName} - {smallestPayload.PayloadSize} bytes");

            SaveTestResult($"{testName}_roundtrip", _results.OrderBy(r => r.RoundtripTime.TotalMilliseconds));
            SaveTestResult($"{testName}_serialize", _results.OrderBy(r => r.SerializationTime.TotalMilliseconds));
            SaveTestResult($"{testName}_deserialize", _results.OrderBy(r => r.DeserializationTime.TotalMilliseconds));
            SaveTestResult($"{testName}_payload", _results.OrderBy(r => r.PayloadSize));
        }

        protected virtual void TestAll()
        {
            SerializeKnownTypesReuseSession();
            SerializeKnownTypes();
            SerializeDefault();

            SerializeVersionPreserveObjects();
            // SerializeNFXSlim();
            // SerializeNFXSlimPreregister();
            // SerializeSSText();
            SerializeNetSerializer();
            SerializeProtoBufNet();
            // SerializeJsonNet();
            // SerializeBinaryFormatter();
        }

        private void SaveTestResult(string testName, IEnumerable<TestResult> result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"## {testName}");
            sb.AppendLine("test | roundtrip | serialize | deserialize | size");
            sb.AppendLine("-----|----------:|----------:|------------:|-----:");
            foreach (var row in result)
            {
                sb.AppendLine(
                    $"{row.TestName} | {(long) row.RoundtripTime.TotalMilliseconds} | {(long) row.SerializationTime.TotalMilliseconds} | {(long) row.DeserializationTime.TotalMilliseconds} | {row.PayloadSize}");
            }
            var file = testName + ".MD";
            File.WriteAllText(file, sb.ToString());
        }

        //private void SerializeNetJson()
        //{
        //    var s = new MemoryStream();
        //    var res = NetJSON.NetJSON.Serialize(Value);
        //    var size = Encoding.UTF8.GetBytes(res).Length;

        //    RunTest("NET-JSON", () =>
        //    {
        //        NetJSON.NetJSON.Serialize(Value);
        //    }, () => { NetJSON.NetJSON.Deserialize<T>(res); }, size);
        //}

        private void SerializeJsonNet()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
                //ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                //PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            var data = JsonConvert.SerializeObject(Value, settings);
            RunTest("Json.NET", () => { JsonConvert.SerializeObject(Value, settings); },
                () => { JsonConvert.DeserializeObject(data, settings); }, Encoding.UTF8.GetBytes(data).Length);
        }

        protected void RunTest(string testName, Action serialize, Action deserialize, int size)
        {
            try
            {
                var tmp = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{testName}");
                Console.ForegroundColor = tmp;

                GC.Collect(2, GCCollectionMode.Forced, true);

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Repeat; i++)
                {
                    serialize();
                }
                sw.Stop();

                Console.WriteLine($"   {"Serialize".PadRight(30, ' ')} {sw.ElapsedMilliseconds} ms");

                GC.Collect(2, GCCollectionMode.Forced, true);

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
                var testResult = new TestResult
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

        //private void SerializeMessageShark()
        //{
        //    var bytes = MessageShark.MessageSharkSerializer.Serialize(Value);
        //    RunTest("MessageShark", () =>
        //    {
        //        MessageShark.MessageSharkSerializer.Serialize(Value);
        //    }, () =>
        //    {
        //        MessageShark.MessageSharkSerializer.Deserialize<T>(bytes);
        //    }, bytes.Length);
        //}

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
            var serializer = new SlimSerializer();
            serializer.TypeMode = TypeRegistryMode.Batch;
            serializer.Serialize(s, Value); //Serialize 1st in batch

            var deserializer = new SlimSerializer();
            deserializer.TypeMode = TypeRegistryMode.Batch;
            s.Position = 0; //Deserialize first in batch
            deserializer.Deserialize(s);

            s = new MemoryStream(); //serialize subsequent in batch
            serializer.Serialize(s, Value);
            var bytes = s.ToArray();

            var stream = new MemoryStream();

            RunTest("NFX Slim Serializer - KnownTypes", () =>
            {
                stream.Position = 0;
                serializer.Serialize(stream, Value);
            }, () =>
            {
                s.Position = 0;
                deserializer.Deserialize(s);
            }, bytes.Length);
        }

        private void SerializeSSText()
        {
            var serializer = new TypeSerializer<T>();
            var text = serializer.SerializeToString(Value);
            var bytes = Encoding.UTF8.GetBytes(text);

            RunTest("ServiceStack.Text", () => { serializer.SerializeToString(Value); },
                () => { serializer.DeserializeFromString(text); }, bytes.Length);
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

        //private void SerializeJil()
        //{
        //    var s = new MemoryStream();
        //    var res = JSON.Serialize(Value);

        //    var bytes = s.ToArray();
        //    RunTest("Jil", () =>
        //    {
        //        JSON.Serialize(Value);
        //    }, () => { JSON.Deserialize(res, typeof(T)); }, bytes.Length);
        //}

        //private void SerializeFsPickler()
        //{
        //    var pickler = MBrace.FsPickler.FsPickler.CreateBinarySerializer();
        //    var s = new MemoryStream();
        //    pickler.Serialize(s, Value);
        //    var bytes = s.ToArray();
        //    RunTest("FsPickler", () =>
        //    {
        //        var stream = new MemoryStream();
        //        pickler.Serialize(stream, Value);
        //    }, () =>
        //    {
        //        s.Position = 0;
        //        pickler.Deserialize<T>(s);
        //    }, bytes.Length);
        //}

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
            var types = typeof(T).Namespace.StartsWith("System") ? null : new[] {typeof(T)};
            var serializer = new Serializer(new SerializerOptions(knownTypes: types));
            using var ss = serializer.GetSerializerSession();
            using var ds = serializer.GetDeserializerSession();
            var s = new MemoryStream();
            serializer.Serialize(Value, s, ss);
            var bytes = s.ToArray();

            RunTest("Wire - KnownTypes + Reuse Sessions", () =>
            {
                var stream = new MemoryStream();
                serializer.Serialize(Value, stream, ss);
            }, () =>
            {
                s.Position = 0;
                serializer.Deserialize<T>(s, ds);
            }, bytes.Length);
        }

        private void SerializeKnownTypes()
        {
            var types = typeof(T).Namespace.StartsWith("System") ? null : new[] {typeof(T)};
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
            var serializer = new Serializer(new SerializerOptions());
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
            var serializer = new Serializer(new SerializerOptions(true));
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
    }

    internal abstract class CustomSerializerTestBase<T> : TestBase<T>
    {
        protected override void TestAll()
        {
            CustomTest();
            base.TestAll();
        }

        protected abstract void CustomTest();
    }
}