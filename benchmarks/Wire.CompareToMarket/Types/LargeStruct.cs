// -----------------------------------------------------------------------
//   <copyright file="LargeStruct.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using ProtoBuf;

namespace Wire.PerfTest.Types
{
    [Serializable]
    [ProtoContract]
    public struct LargeStruct
    {
        private static void A(bool b)
        {
            if (!b)
            {
                throw new Exception();
            }
        }
        
        [ProtoMember(1)]
        public ulong m_val1;
        [ProtoMember(2)]
        public ulong m_val2;
        [ProtoMember(3)]
        public ulong m_val3;
        [ProtoMember(4)]
        public ulong m_val4;

        private static ulong counter;

        public LargeStruct(ulong m_val1, ulong m_val2, ulong m_val3, ulong m_val4)
        {
            this.m_val1 = m_val1;
            this.m_val2 = m_val2;
            this.m_val3 = m_val3;
            this.m_val4 = m_val4;
        }

        public static LargeStruct Create()
        {
            return new LargeStruct
            {
                m_val1 = counter++,
                m_val2 = ulong.MaxValue - counter++,
                m_val3 = counter++,
                m_val4 = ulong.MaxValue - counter++
            };
        }

        public static void Compare(LargeStruct a, LargeStruct b)
        {
            A(a.m_val1 == b.m_val1);
            A(a.m_val2 == b.m_val2);
            A(a.m_val3 == b.m_val3);
            A(a.m_val4 == b.m_val4);
        }
    }
}