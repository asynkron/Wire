using System;

namespace Wire.Tests.Performance.Types
{
    public struct LargeStruct
    {
        private static void A(bool b)
        {
            if (!b)
                throw new Exception();
        }
        
        public ulong m_val1;
        public ulong m_val2;
        public ulong m_val3;
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