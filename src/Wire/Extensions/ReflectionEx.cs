// -----------------------------------------------------------------------
//   <copyright file="ReflectionEx.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wire.Extensions
{
    public static class BindingFlagsEx
    {
        public const BindingFlags All = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public const BindingFlags Static = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
    }

    public static class ReflectionEx
    {
        public static readonly Assembly CoreAssembly = typeof(int).Assembly;

        public static FieldInfo[] GetFieldInfosForType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var fieldInfos = new List<FieldInfo>();
            var current = type;
            while (current != null)
            {
                var tfields =
                    current
                        .GetFields(BindingFlagsEx.All)
                        .Where(f => !f.IsDefined(typeof(NonSerializedAttribute)))
                        .Where(f => !f.IsStatic)
                        .Where(f => f.FieldType != typeof(IntPtr))
                        .Where(f => f.FieldType != typeof(UIntPtr))
                        .Where(f => f.Name != "_syncRoot"); //HACK: ignore these 

                fieldInfos.AddRange(tfields);
                current = current.BaseType;
            }

            var fields = fieldInfos.OrderBy(f => f.Name).ToArray();
            return fields;
        }
    }
}