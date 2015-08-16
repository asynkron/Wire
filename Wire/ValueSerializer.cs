using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Wire
{
    public abstract class ValueSerializer
    {
        public abstract void WriteManifest(Stream stream, Type type, SerializerSession session);
        public abstract void WriteValue(Stream stream, object value, SerializerSession session);
        public abstract object ReadValue(Stream stream, SerializerSession session);
    }

    public class Int64Serializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new Int64Serializer();

        private readonly byte[] _manifest = { 2 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((long)value);
            stream.Write(bytes, 0, bytes.Length);
        }


        public override object ReadValue(Stream stream, SerializerSession session)
        {
            byte[] buffer = session.GetBuffer(8);
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }
    }

    public class Int16Serializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new Int16Serializer();

        private readonly byte[] _manifest = { 3 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((short)value);
            stream.Write(bytes, 0, bytes.Length);
        }


        public override object ReadValue(Stream stream, SerializerSession session)
        {
            byte[] buffer = session.GetBuffer(2);
            stream.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
    }

    public class ByteSerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new ByteSerializer();

        private readonly byte[] _manifest = { 4 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((byte)value);
            stream.Write(bytes, 0, bytes.Length);
        }


        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return stream.ReadByte();
        }
    }

    public class DateTimeSerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new DateTimeSerializer();

        private readonly byte[] _manifest = { 5 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes(((DateTime)value).Ticks);
            stream.Write(bytes, 0, bytes.Length);
        }


        public override object ReadValue(Stream stream, SerializerSession session)
        {
            byte[] buffer = session.GetBuffer(8);
            stream.Read(buffer, 0, 8);
            var ticks = BitConverter.ToInt64(buffer, 0);
            return new DateTime(ticks);
        }
    }

    public class BoolSerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new BoolSerializer();

        private readonly byte[] _manifest = { 6 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var b = (bool)value;
            stream.WriteByte((byte)(b ? 1 : 0));
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            Int32 b = stream.ReadByte();
            if (b == 0)
                return false;
            return true;
        }
    }

    public class StringSerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new StringSerializer();

        private readonly byte[] _manifest = { 7 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            if (value == null)
            {
                Int32Serializer.Instance.WriteValue(stream, -1, session);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes((string)value);
                Int32Serializer.Instance.WriteValue(stream, bytes.Length, session);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var length = (int)Int32Serializer.Instance.ReadValue(stream, session);
            if (length == -1)
                return null;

            byte[] buffer = session.GetBuffer(length);

            stream.Read(buffer, 0, length);
            var res = Encoding.UTF8.GetString(buffer, 0, length);
            return res;
        }
    }

    public class Int32Serializer : ValueSerializer
    {
        public static readonly Int32Serializer Instance = new Int32Serializer();

        private readonly byte[] _manifest = { 8 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = BitConverter.GetBytes((int)value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            byte[] buffer = session.GetBuffer(4);
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
    }

    public class ByteArraySerializer : ValueSerializer
    {
        public static readonly ValueSerializer Instance = new ByteArraySerializer();

        private readonly byte[] _manifest = { 9 };
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var bytes = (byte[])value;
            Int32Serializer.Instance.WriteValue(stream, bytes.Length, session);
            stream.Write(bytes, 0, bytes.Length);
        }


        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var length = (int)Int32Serializer.Instance.ReadValue(stream, session);
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }
    }

    public class ConsistentArraySerializer : ValueSerializer
    {
        public static readonly ConsistentArraySerializer Instance = new ConsistentArraySerializer();

        private readonly byte[] _manifest = { 10 };

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var elementType = session.Serializer.GetArrayElementTypeFromManifest(stream, session);      //read the element type
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);    //get the element type serializer
            var length = (int)Int32Serializer.Instance.ReadValue(stream, session);          //read the array length
            var array = Array.CreateInstance(elementType, length);                          //create the array
            for(int i=0;i<length;i++)
            {
                var value = elementSerializer.ReadValue(stream, session);                   //read the element value
                array.SetValue(value, i);                                                   //set the element value
            }
            return array;
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest, 0, _manifest.Length);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var array = value as Array;
            var elementType = value.GetType().GetElementType();
            var elementSerializer = session.Serializer.GetSerializerByType(elementType);
            elementSerializer.WriteManifest(stream, elementType, session);                  //write array element type
            Int32Serializer.Instance.WriteValue(stream, array.Length, session);             //write array length
            for(int i=0;i<array.Length;i++)                                                 //write the elements
            {
                var elementValue = array.GetValue(i);
                elementSerializer.WriteValue(stream, elementValue, session);
            }
        }
    }

    public class ObjectSerializer : ValueSerializer
    {
        public Action<Stream, object, SerializerSession> Writer { get; set; }
        public Func<Stream, SerializerSession, object> Reader { get; set; }

        private static readonly ConcurrentDictionary<Type, byte[]> AssemblyQualifiedNames = new ConcurrentDictionary<Type, byte[]>();
        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.WriteByte(255);
            var bytes = AssemblyQualifiedNames.GetOrAdd(type, t =>
            {
                var name = t.AssemblyQualifiedName;
                var b = Encoding.UTF8.GetBytes(name);
                return b;
            });
            ByteArraySerializer.Instance.WriteValue(stream, bytes, session);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            Writer(stream, value, session);
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            return Reader(stream, session);
        }
    }
}
