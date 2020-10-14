// -----------------------------------------------------------------------
//   <copyright file="GuidTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;

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
