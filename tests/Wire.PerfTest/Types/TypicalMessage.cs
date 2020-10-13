// -----------------------------------------------------------------------
//   <copyright file="TypicalMessage.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using ProtoBuf;

namespace Wire.PerfTest.Types
{
    [ProtoContract, Serializable]
    public class TypicalMessage
    {
        [ProtoMember(1)] public virtual string StringProp { get; set; }

        [ProtoMember(2)] public virtual int IntProp { get; set; }

        [ProtoMember(3)] public virtual Guid GuidProp { get; set; }

        [ProtoMember(4)] public virtual DateTime DateProp { get; set; }
    }
}
