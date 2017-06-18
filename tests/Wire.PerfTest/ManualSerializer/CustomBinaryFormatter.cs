// -----------------------------------------------------------------------
//   <copyright file="CustomBinaryFormatter.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Wire.PerfTest.ManualSerializer
{
    public interface ICustomBinarySerializable
    {
        void WriteDataTo(BinaryWriter writer);
        void SetDataFrom(BinaryReader reader);
    }

    public class CustomBinaryFormatter : IFormatter
    {
        private readonly Dictionary<int, Type> _byId = new Dictionary<int, Type>();
        private readonly Dictionary<Type, int> _byType = new Dictionary<Type, int>();
        private readonly byte[] _copyBuffer;
        private readonly byte[] _lengthBuffer = new byte[4];
        private readonly BinaryReader _reader;
        private readonly MemoryStream _readStream;
        private readonly BinaryWriter _writer;
        private readonly MemoryStream _writeStream;

        public CustomBinaryFormatter()
        {
            _copyBuffer = new byte[20000];
            _writeStream = new MemoryStream(10000);
            _readStream = new MemoryStream(10000);
            _writer = new BinaryWriter(_writeStream);
            _reader = new BinaryReader(_readStream);
        }

        public object Deserialize(Stream serializationStream)
        {
            if (serializationStream.Read(_lengthBuffer, 0, 4) != 4)
            {
                throw new SerializationException("Could not read length from the stream.");
            }
            var length = new IntToBytes(_lengthBuffer[0], _lengthBuffer[1], _lengthBuffer[2], _lengthBuffer[3]);
            //TODO make this support partial reads from stream
            if (serializationStream.Read(_copyBuffer, 0, length.I32) != length.I32)
            {
                throw new SerializationException("Could not read " + length + " bytes from the stream.");
            }
            _readStream.Seek(0L, SeekOrigin.Begin);
            _readStream.Write(_copyBuffer, 0, length.I32);
            _readStream.Seek(0L, SeekOrigin.Begin);
            var typeid = _reader.ReadInt32();
            Type t;
            if (!_byId.TryGetValue(typeid, out t))
            {
                throw new SerializationException("TypeId " + typeid + " is not a registerred type id");
            }
            var obj = FormatterServices.GetUninitializedObject(t);
            var deserialize = (ICustomBinarySerializable) obj;
            deserialize.SetDataFrom(_reader);
            if (_readStream.Position != length.I32)
            {
                throw new SerializationException("object of type " + t +
                                                 " did not read its entire buffer during deserialization. This is most likely an inbalance between the writes and the reads of the object.");
            }
            return deserialize;
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            int key;
            if (!_byType.TryGetValue(graph.GetType(), out key))
            {
                throw new SerializationException(graph.GetType() + " has not been registered with the serializer");
            }
            var c = (ICustomBinarySerializable) graph; //this will always work due to generic constraint on the Register
            _writeStream.Seek(0L, SeekOrigin.Begin);
            _writer.Write(key);
            c.WriteDataTo(_writer);
            var length = new IntToBytes((int) _writeStream.Position);
            serializationStream.WriteByte(length.B0);
            serializationStream.WriteByte(length.B1);
            serializationStream.WriteByte(length.B2);
            serializationStream.WriteByte(length.B3);
            serializationStream.Write(_writeStream.GetBuffer(), 0, (int) _writeStream.Position);
        }

        public ISurrogateSelector SurrogateSelector { get; set; }

        public SerializationBinder Binder { get; set; }

        public StreamingContext Context { get; set; }

        public void Register<T>(int typeId) where T : ICustomBinarySerializable
        {
            _byId.Add(typeId, typeof(T));
            _byType.Add(typeof(T), typeId);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IntToBytes
    {
        public IntToBytes(int value) { B0 = B1 = B2 = B3 = 0; I32 = value; }
        public IntToBytes(byte b0, byte b1, byte b2, byte b3)
        {
            I32 = 0;
            B0 = b0;
            B1 = b1;
            B2 = b2;
            B3 = b3;
        }
        [FieldOffset(0)]
        public int I32;
        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
        [FieldOffset(2)]
        public byte B2;
        [FieldOffset(3)]
        public byte B3;
    }
}