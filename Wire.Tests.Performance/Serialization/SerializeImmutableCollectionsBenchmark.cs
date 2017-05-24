using System.Collections.Generic;
using System.Collections.Immutable;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Xunit.Abstractions;

namespace Wire.Tests.Performance.Serialization
{
    public class SerializeImmutableCollectionsBenchmark : PerfTestBase
    {
        #if !NBENCH
        public SerializeImmutableCollectionsBenchmark(ITestOutputHelper output) : base(output)
        {
        }
        #endif

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark immutable string array serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 300000)]
        public void Serialize_ImmutableArray()
        {
            var collection = ImmutableArray.CreateRange(new[] {"abc", "cbd0", "sdsd4", "4dfg", "adafd0xd" });
            SerializeAndCount(collection);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark immutable list serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 190000)]
        public void Serialize_ImmutableList()
        {
            var collection = ImmutableList.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg", "adafd0xd" });
            SerializeAndCount(collection);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark immutable hash set serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 100000)]
        public void Serialize_ImmutableHashSet()
        {
            var collection = ImmutableHashSet.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg", "adafd0xd" });
            SerializeAndCount(collection);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark immutable sorted set serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 95000)]
        public void Serialize_ImmutableSortedSet()
        {
            var collection = ImmutableHashSet.CreateRange(new[] { "abc", "cbd0", "sdsd4", "4dfg", "adafd0xd" });
            SerializeAndCount(collection);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark immutable dictionary serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 70000)]
        public void Serialize_ImmutableDictionary()
        {
            var collection = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, string>("key1", "value1"), 
                new KeyValuePair<string, string>("key2", "value2"), 
                new KeyValuePair<string, string>("key3", "value3"), 
                new KeyValuePair<string, string>("key4", "value4"),
                new KeyValuePair<string, string>("key5", "value5"),
            });
            SerializeAndCount(collection);
        }
    }
}
