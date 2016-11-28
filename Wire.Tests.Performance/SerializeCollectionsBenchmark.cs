using System.Collections.Generic;
using System.IO;
using NBench;

namespace Wire.Tests.Performance
{
    public class SerializeCollectionsBenchmark
    {
        #region init

        public const int Iterations = 5;

        private Serializer _serializer;
        private MemoryStream _stream;

        [PerfSetup]
        public void Setup()
        {
            _serializer = new Serializer();
            _stream = new MemoryStream();
        }

        [PerfCleanup]
        public void Cleanup()
        {
            _stream.Dispose();
            _stream = null;
            _serializer = null;
        }

        #endregion

        [PerfBenchmark(Description = "Benchmark byte array serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ByteArray()
        {
            _serializer.Serialize(new byte[] { 123, 134, 11 }, _stream);
        }

        [PerfBenchmark(Description = "Benchmark string array serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_StringArray()
        {
            _serializer.Serialize(new string[] { "abc", "cbd0", "sdsd4", "4dfg" }, _stream);
        }

        [PerfBenchmark(Description = "Benchmark dictionary serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Dictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                ["abc"] = "aaa",
                ["dsdf"] = "asdab"
            };
            _serializer.Serialize(dictionary, _stream);
        }

        [PerfBenchmark(Description = "Benchmark list serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_List()
        {
            _serializer.Serialize(new List<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" }, _stream);
        }

        [PerfBenchmark(Description = "Benchmark linked list serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_LinkedList()
        {
            var list = new LinkedList<string>(new [] { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" });
            _serializer.Serialize(list, _stream);
        }

        [PerfBenchmark(Description = "Benchmark hash set serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_HashSet()
        {
            var set = new HashSet<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" };
            _serializer.Serialize(set, _stream);
        }

        [PerfBenchmark(Description = "Benchmark sorted set serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_SortedSet()
        {
            var set = new SortedSet<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" };
            _serializer.Serialize(set, _stream);
        }
    }
}