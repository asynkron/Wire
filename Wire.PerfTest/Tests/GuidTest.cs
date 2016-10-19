using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire.PerfTest.Tests
{
    class GuidTest : TestBase<Guid>
    {
        protected override Guid GetValue()
        {
            return Guid.NewGuid();
        }
    }
}
