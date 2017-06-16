// -----------------------------------------------------------------------
//   <copyright file="TypicalMessageArrayTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Wire.PerfTest.Types;

namespace Wire.PerfTest.Tests
{
    class TypicalMessageArrayTest : TestBase<TypicalMessage[]>
    {
        protected override TypicalMessage[] GetValue()
        {
            var l = new List<TypicalMessage>();

            for (var i = 0; i < 100; i++)
            {
                var v = new TypicalMessage
                {
                    StringProp = "hello",
                    GuidProp = Guid.NewGuid(),
                    IntProp = 123,
                    DateProp = DateTime.UtcNow
                };
                l.Add(v);
            }

            return l.ToArray();
        }
    }
}
