// -----------------------------------------------------------------------
//   <copyright file="TypeEx.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Wire.Internal;

namespace Wire.Extensions
{
    public static class TypeEx
    {
        private const string VERSION_REGEX = @", Version=(\d+([.]\d+)?([.]\d+)?([.]\d+)?.*?)";

        //Why not inline typeof you ask?
        //Because it actually generates calls to get the type.
        //We prefetch all primitives here
        private static readonly Type Int32Type = typeof(int);
        private static readonly Type Int64Type = typeof(long);
        private static readonly Type Int16Type = typeof(short);
        private static readonly Type UInt32Type = typeof(uint);
        private static readonly Type UInt64Type = typeof(ulong);
        private static readonly Type UInt16Type = typeof(ushort);
        private static readonly Type ByteType = typeof(byte);
        private static readonly Type SByteType = typeof(sbyte);
        private static readonly Type BoolType = typeof(bool);
        private static readonly Type DateTimeType = typeof(DateTime);
        private static readonly Type StringType = typeof(string);
        private static readonly Type GuidType = typeof(Guid);
        private static readonly Type FloatType = typeof(float);
        private static readonly Type DoubleType = typeof(double);
        private static readonly Type DecimalType = typeof(decimal);
        private static readonly Type CharType = typeof(char);
        public static readonly Type RuntimeType = Type.GetType("System.RuntimeType");

        //HACK: the GetUnitializedObject actually exists in .NET Core, its just not public
        private static readonly Func<Type, object> getUninitializedObjectDelegate = GetFormatterDelegate();

        private static readonly ConcurrentDictionary<ByteArrayKey, Type> TypeNameLookup =
            new ConcurrentDictionary<ByteArrayKey, Type>(ByteArrayKeyComparer.Instance);

        private static readonly string CoreAssemblyName = GetCoreAssemblyName();

        public static bool IsWirePrimitive(this Type type)
        {
            return type == Int32Type ||
                   type == Int64Type ||
                   type == Int16Type ||
                   type == UInt32Type ||
                   type == UInt64Type ||
                   type == UInt16Type ||
                   type == ByteType ||
                   type == SByteType ||
                   type == DateTimeType ||
                   type == BoolType ||
                   type == StringType ||
                   type == GuidType ||
                   type == FloatType ||
                   type == DoubleType ||
                   type == DecimalType ||
                   type == CharType;
            //add TypeSerializer with null support
        }

        private static Func<Type, object> GetFormatterDelegate()
        {
            const string FormatterServices = "System.Runtime.Serialization.FormatterServices";

            var formatterType = typeof(string)
                .GetTypeInfo()
                .Assembly
                .GetType(FormatterServices);


            if (formatterType == null)
                // it's been moved to the Formatters assembly in .NET Core 3.x
                formatterType = Assembly.Load("System.Runtime.Serialization.Formatters")?.GetType(FormatterServices);

            return (Func<Type, object>) formatterType?.GetTypeInfo()
                .GetMethod("GetUninitializedObject",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                ?.CreateDelegate(typeof(Func<Type, object>));
        }

        public static object GetEmptyObject(this Type type)
        {
            return getUninitializedObjectDelegate(type);
        }

        public static bool IsOneDimensionalArray(this Type type)
        {
            return type.IsArray && type.GetArrayRank() == 1;
        }

        public static bool IsOneDimensionalPrimitiveArray(this Type type)
        {
            return type.IsArray && type.GetArrayRank() == 1 && type.GetElementType().IsWirePrimitive();
        }

        public static byte[] GetTypeManifest(IReadOnlyCollection<byte[]> fieldNames)
        {
            IEnumerable<byte> result = new[] {(byte) fieldNames.Count};
            foreach (var name in fieldNames)
            {
                var encodedLength = BitConverter.GetBytes(name.Length);
                result = result.Concat(encodedLength);
                result = result.Concat(name);
            }

            var versionTolerantHeader = result.ToArray();
            return versionTolerantHeader;
        }

        private static Type GetTypeFromManifestName(Stream stream, DeserializerSession session)
        {
            var bytes = stream.ReadLengthEncodedByteArray(session);
            var byteArr = ByteArrayKey.Create(bytes);
            return TypeNameLookup.GetOrAdd(byteArr, b =>
            {
                var shortName = StringEx.FromUtf8Bytes(b.Bytes, 0, b.Bytes.Length);
                var typename = ToQualifiedAssemblyName(shortName);
                return Type.GetType(typename, true);
            });
        }

        public static Type GetTypeFromManifestFull(Stream stream, DeserializerSession session)
        {
            var type = GetTypeFromManifestName(stream, session);
            session.TrackDeserializedType(type);
            return type;
        }



        public static Type GetTypeFromManifestIndex(int typeId, DeserializerSession session)
        {
            var type = session.GetTypeFromTypeId(typeId);
            return type;
        }

        public static bool IsNullable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNullableElement(this Type type)
        {
            return type.GetTypeInfo().GetGenericArguments()[0];
        }

        public static bool IsFixedSizeType(this Type type)
        {
            return type == Int16Type ||
                   type == Int32Type ||
                   type == Int64Type ||
                   type == BoolType ||
                   type == UInt16Type ||
                   type == UInt32Type ||
                   type == UInt64Type ||
                   type == CharType;
        }

        public static int GetTypeSize(this Type type)
        {
            if (type == Int16Type) return sizeof(short);
            if (type == Int32Type) return sizeof(int);
            if (type == Int64Type) return sizeof(long);
            if (type == BoolType) return sizeof(bool);
            if (type == UInt16Type) return sizeof(ushort);
            if (type == UInt32Type) return sizeof(uint);
            if (type == UInt64Type) return sizeof(ulong);
            if (type == CharType) return sizeof(char);

            throw new NotSupportedException();
        }

        private static string GetCoreAssemblyName()
        {
            var name = 1.GetType().AssemblyQualifiedName;
            var part = name.Substring(name.IndexOf(", Version", StringComparison.Ordinal));
            return part;
        }

        public static string GetShortAssemblyQualifiedName(this Type self)
        {
            var name = self.AssemblyQualifiedName;
            return ReplaceTokens(name);
        }

        public static string ToQualifiedAssemblyName(string shortName)
        {
            var res = shortName.Replace(",%core%", CoreAssemblyName);
            return res;
        }

        public static string ReplaceTokens(string input)
        {
            var name = input;
            name = name.Replace(CoreAssemblyName, ",%core%");
            name = name.Replace(", Culture=neutral", "");
            name = name.Replace(", PublicKeyToken=null", "");
            name = ReplaceVersion(name);
            return name;
        }

        private static string ReplaceVersion(string input)
        {
            var regex = new Regex(VERSION_REGEX);
            return regex.Replace(input, "");
        }
    }
}