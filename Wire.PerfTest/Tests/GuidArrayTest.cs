// -----------------------------------------------------------------------
//   <copyright file="GuidArrayTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Wire.PerfTest.Tests
{
    class GuidArrayTest : TestBase<Guid[]>
    {
        protected override Guid[] GetValue()
        {
            var l = new List<Guid>();
            for (var i = 0; i < 100; i++)
            {
                l.Add(Guid.NewGuid());
            }
            return l.ToArray();
        }
    }
}
