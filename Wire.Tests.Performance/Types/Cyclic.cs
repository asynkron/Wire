using System;

namespace Wire.Tests.Performance.Types
{
    [Serializable]
    public class CyclicA
    {
        public CyclicB B { get; set; }
    }

    [Serializable]
    public class CyclicB
    {
        public CyclicA A { get; set; }
    }
}