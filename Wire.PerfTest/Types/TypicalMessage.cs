// -----------------------------------------------------------------------
//   <copyright file="TypicalMessage.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using ProtoBuf;
using ZeroFormatter;

namespace Wire.PerfTest.Types
{
    [ProtoContract]
    [Serializable]
    [ZeroFormattable]
    public class TypicalMessage
    {
        [ProtoMember(1)]
        [Index(0)]
        public virtual string StringProp { get; set; }

        [ProtoMember(2)]
        [Index(1)]
        public virtual int IntProp { get; set; }

        [ProtoMember(3)]
        [Index(2)]
        public virtual Guid GuidProp { get; set; }

        [ProtoMember(4)]
        [Index(3)]
        public virtual DateTime DateProp { get; set; }
    }
}
