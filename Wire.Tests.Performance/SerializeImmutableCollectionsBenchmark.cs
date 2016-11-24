using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using NBench;

namespace Wire.Tests.Performance
{
    public class SerializeImmutableCollectionsBenchmark
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

        [PerfBenchmark(Description = "Benchmark immutable array serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ImmutableArray()
        {
            var collection = ImmutableArray.CreateRange(new[] {"abc", "cbd0", "sdsd4", "4dfg"});
            _serializer.Serialize(collection, _stream);
        }

        [PerfBenchmark(Description = "Benchmark immutable list serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ImmutableList()
        {
            var collection = ImmutableList.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg" });
            _serializer.Serialize(collection, _stream);
        }

        [PerfBenchmark(Description = "Benchmark immutable hash set serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ImmutableHashSet()
        {
            var collection = ImmutableHashSet.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg" });
            _serializer.Serialize(collection, _stream);
        }

        [PerfBenchmark(Description = "Benchmark immutable sorted set serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ImmutableSortedSet()
        {
            var collection = ImmutableHashSet.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg" });
            _serializer.Serialize(collection, _stream);
        }

        [PerfBenchmark(Description = "Benchmark immutable dictionary serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_ImmutableDictionary()
        {
            var collection = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, string>("key1", "value1"), 
                new KeyValuePair<string, string>("key2", "value2"), 
                new KeyValuePair<string, string>("key3", "value3"), 
                new KeyValuePair<string, string>("key4", "value4"),
            });
            _serializer.Serialize(collection, _stream);
        }
    }
}
