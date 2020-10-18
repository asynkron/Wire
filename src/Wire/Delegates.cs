// -----------------------------------------------------------------------
//   <copyright file="Delegates.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Buffers;
using System.IO;
using Wire.Buffers;

namespace Wire
{
    //Reads an entire object from a stream, including manifests
    public delegate object ObjectReader(Stream stream, DeserializerSession session);

    //Writes an entire object to a stream, including manifests
    public delegate void ObjectWriter<TBufferWriter>(ref Writer<TBufferWriter> writer, object obj,
        SerializerSession session) where TBufferWriter : IBufferWriter<byte>;


    public delegate object TypedArray(object obj);
}