// -----------------------------------------------------------------------
//   <copyright file="TypicalPersonArrayTest.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Wire.PerfTest.Types;

namespace Wire.PerfTest.Tests
{
    class TypicalPersonArrayTest : TestBase<TypicalPersonData[]>
    {
        protected override TypicalPersonData[] GetValue()
        {
            var l = new List<TypicalPersonData>();
            for (var i = 0; i < 100; i++)
            {
                l.Add(TypicalPersonData.MakeRandom());
            }
            return l.ToArray();
        }
    }
}
