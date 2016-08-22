using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wire.PerfTest.Tests
{
    class GuidArrayTest : TestBase<Guid[]>
    {
        protected override Guid[] GetValue()
        {
            var l = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                l.Add(Guid.NewGuid());
            }
            return l.ToArray();
        }
    }
}
