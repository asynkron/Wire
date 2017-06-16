// -----------------------------------------------------------------------
//   <copyright file="TestMessage.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;

namespace Wire.Tests.Performance.Types
{
    public class TestMessage
    {
        public virtual string StringProp { get; set; }

        public virtual int IntProp { get; set; }

        public virtual Guid GuidProp { get; set; }

        public virtual DateTime DateProp { get; set; }
    }
}