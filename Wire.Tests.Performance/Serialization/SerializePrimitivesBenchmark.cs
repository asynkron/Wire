using System;
using System.Diagnostics;
using System.IO;
using NBench;
using Pro.NBench.xUnit.XunitExtensions;
using Xunit.Abstractions;

namespace Wire.Tests.Performance.Serialization
{
    public class SerializePrimitivesBenchmark : PerfTestBase
    {
        #if !NBENCH
        public SerializePrimitivesBenchmark(ITestOutputHelper output) : base(output)
        {
        }
        #endif

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark byte serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 3000000)]
        public void Serialize_Byte()
        {
            SerializeAndCount((byte)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Int16 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2400000)]
        public void Serialize_Int16()
        {
            SerializeAndCount((short)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Int32 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2300000)]
        public void Serialize_Int32()
        {
            SerializeAndCount((int)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Int64 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2200000)]
        public void Serialize_Int64()
        {
            SerializeAndCount((long)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.SByte serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 3000000)]
        public void Serialize_SByte()
        {
            SerializeAndCount((sbyte)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.UInt16 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2300000)]
        public void Serialize_UInt16()
        {
            SerializeAndCount((ushort)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.UInt32 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2100000)]
        public void Serialize_UInt32()
        {
            SerializeAndCount((uint)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.UInt64 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2100000)]
        public void Serialize_UInt64()
        {
            SerializeAndCount((ulong)123);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Boolean serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2100000)]
        public void Serialize_Boolean()
        {
            SerializeAndCount(true);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Single serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2200000)]
        public void Serialize_Single()
        {
            SerializeAndCount((float)123.56);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Double serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2200000)]
        public void Serialize_Double()
        {
            SerializeAndCount((double)123.56);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Decimal serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1400000)]
        public void Serialize_Decimal()
        {
            SerializeAndCount((decimal)123.56);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.String serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 800000)]
        public void Serialize_String()
        {
            var x = new string('x', 100);
            SerializeAndCount(x);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Guid serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1500000)]
        public void Serialize_Guid()
        {
            SerializeAndCount(Guid.NewGuid());
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.DateTime serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 2000000)]
        public void Serialize_DateTime()
        {
            SerializeAndCount(DateTime.UtcNow);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.TimeSpan serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1300000)] //FIXME: why is this so slower than DateTime?
        public void Serialize_TimeSpan()
        {
            SerializeAndCount(DateTime.UtcNow.TimeOfDay);
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`1 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1300000)]
        public void Serialize_Tuple1()
        {
            SerializeAndCount(Tuple.Create(123));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`2 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 1100000)]
        public void Serialize_Tuple2()
        {
            SerializeAndCount(Tuple.Create(123, true));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`3 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 900000)]
        public void Serialize_Tuple3()
        {
            SerializeAndCount(Tuple.Create(123, true, "x"));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`4 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 720000)]
        public void Serialize_Tuple4()
        {
            SerializeAndCount(Tuple.Create(123, true, "x", 123.3f));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`5 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 570000)]
        public void Serialize_Tuple5()
        {
            SerializeAndCount(Tuple.Create(123, true, "x", 123.3f, "asdasdac"));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`6 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 500000)]
        public void Serialize_Tuple6()
        {
            SerializeAndCount(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`7 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 500000)]
        public void Serialize_Tuple7()
        {
            SerializeAndCount(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false, (byte)0xf));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Tuple`8 serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 350000)]
        public void Serialize_Tuple8()
        {
            SerializeAndCount(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false, (byte)0xf, 1234));
        }

        [NBenchFact]
        [PerfBenchmark(
            Description = "Benchmark System.Type serialization",
            NumberOfIterations = StandardIterationCount,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = StandardRunTime,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(TestCounterName, MustBe.GreaterThan, 500000)]
        public void Serialize_Type()
        {
            SerializeAndCount(typeof(int));
        }
    }
}
