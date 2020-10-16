// -----------------------------------------------------------------------
//   <copyright file="DecimalSerializer.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Wire.ValueSerializers
{
    public class DecimalSerializer : SessionAwareValueSerializer<decimal>
    {
        public const byte Manifest = 14;
        public static readonly DecimalSerializer Instance = new DecimalSerializer();
        
        public DecimalSerializer() : base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }
        
        public override int PreallocatedByteBufferSize => Int32Serializer.Size;

        private static void WriteValueImpl(Stream stream, decimal value, byte[] bytes)
        {
            var data = decimal.GetBits(value);
            Int32Serializer.WriteValueImpl(stream, data[0], bytes);
            Int32Serializer.WriteValueImpl(stream, data[1], bytes);
            Int32Serializer.WriteValueImpl(stream, data[2], bytes);
            Int32Serializer.WriteValueImpl(stream, data[3], bytes);
        }

        private static decimal ReadValueImpl(Stream stream, byte[] bytes)
        {
            var parts = new[]
            {
                Int32Serializer.ReadValueImpl(stream, bytes),
                Int32Serializer.ReadValueImpl(stream, bytes),
                Int32Serializer.ReadValueImpl(stream, bytes),
                Int32Serializer.ReadValueImpl(stream, bytes)
            };
            var sign = (parts[3] & 0x80000000) != 0;

            var scale = (byte) ((parts[3] >> 16) & 0x7F);
            var newValue = new decimal(parts[0], parts[1], parts[2], sign, scale);
            return newValue;
        }
    }
}