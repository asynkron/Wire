using Wire.PerfTest.Types;

namespace Wire.PerfTest.Tests
{
    internal class LargeStructTest : TestBase<LargeStruct>
    {
        protected override LargeStruct GetValue()
        {
            return LargeStruct.Create();
        }
    }
}