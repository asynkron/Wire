// -----------------------------------------------------------------------
//   <copyright file="ByteArrayKey.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

namespace Wire.Internal
{
    internal struct ByteArrayKey
    {
        public readonly byte[] Bytes;
        private readonly int _hashCode;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            var other = (ByteArrayKey) obj;
            return Compare(Bytes, other.Bytes);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static int GetHashCode(byte[] bytes)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < bytes.Length; i++) hash = hash * 23 + bytes[i];
                return hash;
            }
        }

        private ByteArrayKey(byte[] bytes)
        {
            Bytes = bytes;
            _hashCode = GetHashCode(bytes);
        }

        public static ByteArrayKey Create(byte[] bytes)
        {
            return new ByteArrayKey(bytes);
        }


        public static unsafe bool Compare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length) return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                var l = a1.Length;
                for (var i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*(long*) x1 != *(long*) x2)
                        return false;
                if ((l & 4) != 0)
                {
                    if (*(int*) x1 != *(int*) x2) return false;
                    x1 += 4;
                    x2 += 4;
                }

                if ((l & 2) != 0)
                {
                    if (*(short*) x1 != *(short*) x2) return false;
                    x1 += 2;
                    x2 += 2;
                }

                if ((l & 1) == 0) return true;
                return *x1 == *x2;
            }
        }
    }
}