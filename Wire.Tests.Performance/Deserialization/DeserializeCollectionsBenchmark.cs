using System.Collections.Generic;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Xunit.Abstractions;

namespace Wire.Tests.Performance.Deserialization
{
    public sealed class ByteArrayDeserializationBenchmark : PerfTestBase
    {
        public ByteArrayDeserializationBenchmark(ITestOutputHelper output)
            : base(output) { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new byte[] { 123, 134, 11, 122, 1 });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark byte array deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_ByteArray()
        {
            DeserializeAndCount<byte[]>();
        }
    }

    public sealed class StringArrayDeserializationBenchmark : PerfTestBase
    {
        public StringArrayDeserializationBenchmark(ITestOutputHelper output)
           : base(output) { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new string[] {"abc", "cbd0", "sdsd4", "4dfg", "sfsdf44g"});
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark string array deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_StringArray()
        {
            DeserializeAndCount<string[]>();
        }
    }

    public sealed class DictionaryDeserializationBenchmark : PerfTestBase
    {
        public DictionaryDeserializationBenchmark(ITestOutputHelper output)
           : base(output)
        { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new Dictionary<string, string>
            {
                ["abc"] = "aaa",
                ["dsdf"] = "asdab",
                ["fms0"] = "sdftu"
            });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark dictionary deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_Dictionary()
        {
            DeserializeAndCount<Dictionary<string, string>>();
        }
    }

    public sealed class ListDeserializationBenchmark : PerfTestBase
    {
        public ListDeserializationBenchmark(ITestOutputHelper output)
           : base(output)
        { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new List<string> {"asdad", "asdabs3", "sfsdf44g", "asdf4r", "sfsdf44g"});
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark list deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_StringList()
        {
            DeserializeAndCount<List<string>>();
        }
    }

    public sealed class LinkedListDeserializationBenchmark : PerfTestBase
    {
        public LinkedListDeserializationBenchmark(ITestOutputHelper output)
           : base(output)
        { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new LinkedList<string>(new[] {"asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g"}));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark linked list deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_LinkedList()
        {
            DeserializeAndCount<LinkedList<string>>();
        }
    }

    public sealed class HashSetDeserializationBenchmark : PerfTestBase
    {
        public HashSetDeserializationBenchmark(ITestOutputHelper output)
           : base(output)
        { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new HashSet<string> { "asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g" });
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark hash set deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_HashSet()
        {
            DeserializeAndCount<HashSet<string>>();
        }
    }

    public sealed class SortedSetDeserializationBenchmark : PerfTestBase
    {
        public SortedSetDeserializationBenchmark(ITestOutputHelper output)
           : base(output)
        { }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            InitStreamWith(new SortedSet<string> {"asdad", "asdabs3", "dfsdf9", "asdf4r", "sfsdf44g"});
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark sorted set deserialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: counter is not working")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Deserialize_SortedSet()
        {
            DeserializeAndCount<SortedSet<string>>();
        }
    }
}