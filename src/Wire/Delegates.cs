// -----------------------------------------------------------------------
//   <copyright file="Delegates.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Wire
{
    //Reads an entire object from a stream, including manifests
    public delegate object ObjectReader(Stream stream, DeserializerSession session);

    //Writes an entire object to a stream, including manifests
    public delegate void ObjectWriter(Stream stream, object obj, SerializerSession session);


    public delegate object TypedArray(object obj);
}