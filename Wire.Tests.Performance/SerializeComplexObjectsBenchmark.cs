using System.IO;
using NBench;
using Wire.Tests.Performance.Types;

namespace Wire.Tests.Performance
{
    public class SerializeComplexObjectsBenchmark
    {
        #region init

        public const int Iterations = 5;

        private Serializer _serializer;
        private MemoryStream _stream;

        private LargeStruct _testStruct;
        private TypicalPersonData _testPerson; 

        [PerfSetup]
        public void Setup()
        {
            _serializer = new Serializer();
            _stream = new MemoryStream();
            _testStruct = LargeStruct.Create();
            _testPerson = TypicalPersonData.MakeRandom();
        }

        [PerfCleanup]
        public void Cleanup()
        {
            _stream.Dispose();
            _stream = null;
            _serializer = null;
            _testStruct = default(LargeStruct);
            _testPerson = null;
        }

        #endregion
        
        [PerfBenchmark(Description = "Benchmark struct serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_LargeStruct()
        {
            _serializer.Serialize(_testStruct, _stream);
        }

        [PerfBenchmark(Description = "Benchmark large object serialization",
            RunMode = RunMode.Iterations, TestMode = TestMode.Test, NumberOfIterations = Iterations)]
        [TimingMeasurement]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 300)]
        public void Serialize_LargeObject()
        {
            _serializer.Serialize(_testPerson, _stream);
        }
    }
}