// -----------------------------------------------------------------------
//   <copyright file="NoAllocBitConverter.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wire.Internal
{
    /// <summary>
    ///     Provides methods not allocating the byte buffer but using <see cref="SerializerSession.GetBuffer" /> to lease a
    ///     buffer.
    /// </summary>
    internal static class BitConverterEx
    {
        internal static readonly UTF8Encoding Utf8 = (UTF8Encoding) Encoding.UTF8;
    }
}