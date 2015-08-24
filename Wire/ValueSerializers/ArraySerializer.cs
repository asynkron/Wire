using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Wire.ValueSerializers
{
    public class ArraySerializer : ValueSerializer
    {
        private readonly Type _elementType;
        private readonly byte[] _manifest;
        private readonly Type _type;
        public const byte Manifest = 255;
        public ArraySerializer(Type type)
        {
            _type = type;
            _elementType = type.GetElementType();

            var bytes = Encoding.UTF8.GetBytes(type.AssemblyQualifiedName);

            //precalculate the entire manifest for this serializer
            //this helps us to minimize calls to Stream.Write/WriteByte 
            _manifest =
                new[] { Manifest } //same as object serializer
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
                var value = stream.ReadObject(session);
                array.SetValue(value, i); //set the element value
            }
            return array;
        }

        public override Type GetElementType()
        {
            return _type; //not a typo, should not be _elementType;
        }

        public override void WriteManifest(Stream stream, Type type, SerializerSession session)
        {
            stream.Write(_manifest);
        }

        public override void WriteValue(Stream stream, object arr, SerializerSession session)
        {
            var array = arr as Array;
            var elementSerializer = session.Serializer.GetSerializerByType(_elementType);
            stream.WriteInt32(array.Length);
            var preserveObjectReferences = session.Serializer.Options.PreserveObjectReferences;

            for (var i = 0; i < array.Length; i++) //write the elements
            {
                var value = array.GetValue(i);
                stream.WriteObject(value, _elementType, elementSerializer, preserveObjectReferences, session);
            }
        }
    }
}