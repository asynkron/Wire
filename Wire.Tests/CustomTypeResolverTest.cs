using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wire.ValueSerializers;

namespace Wire.Tests
{
    class CustomTypeResolver : ITypeResolver
    {
        private static byte[] GetManifestNameFromType(Type type)
        {
            var typeName = "@" + type.AssemblyQualifiedName + "@";
            var typeNameBytes = Encoding.UTF8.GetBytes(typeName);
            return new[] { ObjectSerializer.ManifestFull }
                .Concat(BitConverter.GetBytes(typeNameBytes.Length))
                .Concat(typeNameBytes)
                .ToArray(); //serializer id 255 + @ + assembly qualified name + @
        }

        private static Type GetTypeFromManifestName(Stream stream, DeserializerSession session)
        {
            var bytes = (byte[])ByteArraySerializer.Instance.ReadValue(stream, session);
            var typeName = Encoding.UTF8.GetString(bytes);
            if (typeName[0] != '@' || typeName[typeName.Length - 1] != '@')
                throw new TypeLoadException("Invalid type name: " + typeName);
            return Type.GetType(typeName.Substring(1, typeName.Length - 2), true);
        }

        public void WriteType(Stream stream, Type type, SerializerSession session)
        {
            if (session.ShouldWriteTypeManifest(type))
            {
                stream.Write(GetManifestNameFromType(type));
            }
            else
            {
                var typeIdentifier = session.GetTypeIdentifier(type);
                stream.Write(new[] { ObjectSerializer.ManifestIndex });
                stream.WriteUInt16((ushort)typeIdentifier);
            }
        }

        public Type ReadType(Stream stream, int firstManifest, DeserializerSession session)
        {
            if (firstManifest == ObjectSerializer.ManifestFull)
            {
                var type = GetTypeFromManifestName(stream, session);
                session.TrackDeserializedType(type);
                return type;
            }
            else
            {
                var typeId = stream.ReadUInt16(session);
                var type = session.GetTypeFromTypeId(typeId);
                return type;
            }
        }
    }

    [TestClass]
    public class CustomTypeResolverTest
    {
        private Serializer serializer;
        private MemoryStream stream;

        [TestInitialize]
        public void Setup()
        {
            var option = new SerializerOptions(typeResolver: new CustomTypeResolver());
            serializer = new Serializer(option);
            stream = new MemoryStream();
        }

        public void Reset()
        {
            stream.Position = 0;
        }

        public void Serialize(object o)
        {
            serializer.Serialize(o, stream);
        }

        public T Deserialize<T>()
        {
            return serializer.Deserialize<T>(stream);
        }

        [TestMethod]
        public void CanSerializePolymorphicObjectWithCustomTypeResolver()
        {
            var expected = new Something
            {
                Else = new OtherElse
                {
                    Name = "Foo",
                    More = "Bar"
                }
            };
            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeObjectWithCustomTypeResolver()
        {
            var expected = new Something
            {
                BoolProp = true,
                Int32Prop = 123,
                NullableInt32PropHasValue = 888,
                StringProp = "hello"
            };


            Serialize(expected);
            Reset();
            var actual = Deserialize<Something>();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanSerializeTypeWithCustomTypeResolver()
        {
            var expected = Tuple.Create(GetType(), GetType(), (Type)null);
            Serialize(expected);
            Reset();
            var actual = Deserialize<Tuple<Type, Type, Type>>();
            Assert.AreEqual(expected, actual);
        }
    }
}
