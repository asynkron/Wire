using System.Collections.Generic;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Xunit.Abstractions;

namespace Wire.Tests.Performance.Serialization
{
    public class SerializeCollectionsBenchmark : PerfTestBase
    {
        #if !NBENCH
        public SerializeCollectionsBenchmark(ITestOutputHelper output) : base(output)
        {
        }
        #endif

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark byte array serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1000000)]
        public void Serialize_ByteArray()
        {
            SerializeAndCount(new byte[] {123, 134, 11, 122, 1 });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark string array serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 400000)]
        public void Serialize_StringArray()
        {
            SerializeAndCount(new string[] {"abc", "cbd0", "sdsd4", "4dfg", "sfsdf44g" });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark dictionary serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 150000)]
        public void Serialize_Dictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                ["abc"] = "aaa",
                ["dsdf"] = "asdab",
                ["fms0"] = "sdftu"
            };
            SerializeAndCount(dictionary);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark list serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 250000)]
        public void Serialize_List()
        {
            SerializeAndCount(new List<string> { "asdad", "asdabs3", "sfsdf44g", "asdf4r", "sfsdf44g" });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark linked list serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 90000)]
        public void Serialize_LinkedList()
        {
            var list = new LinkedList<string>(new [] { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" });
            SerializeAndCount(list);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark hash set serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 620)] //FIXME: optimize hash sets... seriously
        public void Serialize_HashSet()
        {
            var set = new HashSet<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" };
            SerializeAndCount(set);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark sorted set serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 55000)]
        public void Serialize_SortedSet()
        {
            var set = new SortedSet<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" };
            SerializeAndCount(set);
        }
    }
}