using System.IO;

namespace Wire.ValueSerializers
{
    public class BoolSerializer : SessionIgnorantValueSerializer<bool>
    {
        public const byte Manifest = 6;
        public static readonly BoolSerializer Instance = new BoolSerializer();

        public BoolSerializer() :
            base(Manifest, () => WriteValueImpl, () => ReadValueImpl)
        {
        }

        public static bool ReadValueImpl(Stream stream)
        {
            var b = stream.ReadByte();
            return b != 0;
        }

        public static void WriteValueImpl(Stream stream, bool b)
        {
            stream.WriteByte((byte) (b ? 1 : 0));
        }
    }
}