// -----------------------------------------------------------------------
//   <copyright file="TypicalPersonTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using Wire.PerfTest.Types;

namespace Wire.PerfTest.Tests
{
    class TypicalPersonTest : TestBase<TypicalPersonData>
    {
        protected override TypicalPersonData GetValue()
        {
            return TypicalPersonData.MakeRandom();
        }
    }
}
