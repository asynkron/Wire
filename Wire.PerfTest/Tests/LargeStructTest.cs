// -----------------------------------------------------------------------
//   <copyright file="LargeStructTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

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