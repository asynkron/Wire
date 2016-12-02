using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Wire.Tests.Performance.Types;
using Xunit.Abstractions;

namespace Wire.Tests.Performance.Serialization
{
    public class SerializeComplexObjectsBenchmark : PerfTestBase
    {
        private LargeStruct _testStruct;
        private TypicalPersonData _testObject;
        private CyclicA _cyclic;
        
        public SerializeComplexObjectsBenchmark(ITestOutputHelper output) 
            : base(output, new SerializerOptions(versionTolerance: false, preserveObjectReferences: true))
        {
        }

        public override void Setup(BenchmarkContext context)
        {
            base.Setup(context);
            _testStruct = LargeStruct.Create();
            _testObject = TypicalPersonData.MakeRandom();

            var a = new CyclicA();
            var b = new CyclicB();
            a.B = b;
            b.A = a;

            _cyclic = a;
        }
        
        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark struct serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1600000)]
        public void Serialize_Struct()
        {
            SerializeAndCount(_testStruct);
        }
        
        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark big object serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 230000)]
        public void Serialize_LargeObject()
        {
            SerializeAndCount(_testObject);
        }

        [NBenchFact(Skip = "FIXME: stack overflow")]
        [PerfBenchmark(
            Description = "Benchmark cyclic reference serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test,
            Skip = "FIXME: stack overflow")]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 230000)]
        public void Serialize_CyclicReferences()
        {
            SerializeAndCount(_cyclic);
        }
    }
}