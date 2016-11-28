using System;
using System.IO;
using NBench;

namespace Wire.Tests.Performance
{
    public class SerializePrimitivesBenchmark
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

        [PerfBenchmark(Description = "Benchmark System.Byte serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Byte()
        {
            _serializer.Serialize((byte)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Int16 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Int16()
        {
            _serializer.Serialize((short)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Int32 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Int32()
        {
            _serializer.Serialize((int)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Int64 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Int64()
        {
            _serializer.Serialize((long)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.SByte serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_SByte()
        {
            _serializer.Serialize((sbyte)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.UInt16 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_UInt16()
        {
            _serializer.Serialize((ushort)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.UInt32 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_UInt32()
        {
            _serializer.Serialize((uint)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.UInt64 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_UInt64()
        {
            _serializer.Serialize((ulong)123, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Boolean serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Boolean()
        {
            _serializer.Serialize(true, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Single serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Single()
        {
            _serializer.Serialize((float)123.56, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Double serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Double()
        {
            _serializer.Serialize((double)123.56, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Decimal serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Decimal()
        {
            _serializer.Serialize((decimal)123.56, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.String serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_String()
        {
            var x = new string('x', Iterations);
            _serializer.Serialize(x, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Guid serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Guid()
        {
            _serializer.Serialize(Guid.NewGuid(), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.DateTime serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_DateTime()
        {
            _serializer.Serialize(DateTime.UtcNow, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.TimeSpan serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_TimeSpan()
        {
            _serializer.Serialize(DateTime.UtcNow.TimeOfDay, _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`1 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple1()
        {
            _serializer.Serialize(Tuple.Create(123), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`2 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple2()
        {
            _serializer.Serialize(Tuple.Create(123, true), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`3 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple3()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x"), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`4 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple4()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x", 123.3f), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`5 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple5()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x", 123.3f, "asdasdac"), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`6 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple6()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`7 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple7()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false, (byte)0xf), _stream);
        }

        [PerfBenchmark(Description = "Benchmark System.Tuple`8 serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_Tuple8()
        {
            _serializer.Serialize(Tuple.Create(123, true, "x", 123.3f, "asdasdac", false, (byte)0xf, 1234), _stream);
        }
    }
}
