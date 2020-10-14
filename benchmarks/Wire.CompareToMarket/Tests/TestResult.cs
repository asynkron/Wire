using System;

namespace Wire.PerfTest.Tests
{
    internal class TestResult
    {
        public string TestName { get; set; }
        public TimeSpan SerializationTime { get; set; }
        public TimeSpan DeserializationTime { get; set; }
        public TimeSpan RoundtripTime => SerializationTime + DeserializationTime;
        public int PayloadSize { get; set; }
        public bool Success { get; set; }
    }
}