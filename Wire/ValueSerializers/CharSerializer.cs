using System;
using System.IO;

namespace Wire.ValueSerializers
{
    public class CharSerializer : SessionAwareValueSerializer<char>
    {
        public const byte Manifest = 15;
        public const int Size = sizeof(char);
        public static readonly CharSerializer Instance = new CharSerializer();

        public CharSerializer() : base(Manifest, () => WriteValueImpl, ()=>ReadValueImpl)
        {
        }
        
        public static char ReadValueImpl(Stream stream, DeserializerSession session)
        {
            var buffer = session.GetBuffer(Size);
            stream.Read(buffer, 0, Size);
            return (char) BitConverter.ToSingle(buffer, 0);
        }

        public static void WriteValueImpl(Stream stream, char ch, SerializerSession session)
        {
            var bytes = NoAllocBitConverter.GetBytes(ch, session);
            stream.Write(bytes, 0, Size);
        }
    }
}