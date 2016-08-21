using System;
using Wire.PerfTest.Types;

namespace Wire.PerfTest.Tests
{
    class TypicalMessageTest : TestBase<TypicalMessage>
    {
        protected override TypicalMessage GetValue()
        {
            return new TypicalMessage()
            {
                StringProp = "hello",
                GuidProp = Guid.NewGuid(),
                IntProp = 123,
                DateProp = DateTime.UtcNow
            };
        }
    }
}
