using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ArraySerializer : ValueSerializer 
    {
        private readonly byte[] _manifest;
        private readonly Type _elementType;

        public ArraySerializer(Type type)
        {

            _elementType = type.GetElementType();

            var bytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new byte[] { 255 } //same as object serializer
                    .Concat(BitConverter.GetBytes(bytes.Length))
                    .Concat(bytes)
                    .ToArray(); //serializer id 255 + assembly qualified name
        }

        public override object ReadValue(Stream stream, SerializerSession session)
        {
            var length = stream.ReadInt32(session);
            var array = Array.CreateInstance(_elementType, length); //create the array

            for (var i = 0; i < length; i++)
            {
                var s = session.Serializer.GetSerializerByManifest(stream, session);
                var value = s.ReadValue(stream, session); //read the element value
                array.SetValue(value, i); //set the element value
            }
            return array;
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest);
        }

        public override void WriteValue(Stream stream, object value, SerializerSession session)
        {
            var array = value as Array;
            var elementSerializer = session.Serializer.GetSerializerByType(_elementType);
            stream.WriteInt32(array.Length);
            for (var i = 0; i < array.Length; i++) //write the elements
            {
                var elementValue = array.GetValue(i);
                if (elementValue == null)
                {
                    NullSerializer.Instance.WriteManifest(stream,null,session);
                }
                else
                {
                    var vType = elementValue.GetType();
                    var s2 = elementSerializer;
                    if (vType != _elementType)
                    {
                        //value is of subtype, lookup the serializer for that type
                        s2 = session.Serializer.GetSerializerByType(vType);
                    }
                    //lookup serializer for subtype
                    s2.WriteManifest(stream, vType, session);
                    s2.WriteValue(stream, elementValue, session);
                }
            }
        }
    }
}