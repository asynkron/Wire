// -----------------------------------------------------------------------
//   <copyright file="SerializerOptions.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Wire.SerializerFactories;

namespace Wire
{
    public class SerializerOptions
    {
        private static readonly Surrogate[] EmptySurrogates = new Surrogate[0];
        private static readonly ValueSerializerFactory[] EmptyValueSerializerFactories = new ValueSerializerFactory[0];

        private static readonly ValueSerializerFactory[] DefaultValueSerializerFactories =
        {
            new ConsistentArraySerializerFactory(),
            new MethodInfoSerializerFactory(),
            new PropertyInfoSerializerFactory(),
            new ConstructorInfoSerializerFactory(),
            new FieldInfoSerializerFactory(),
            new DelegateSerializerFactory(),
            new ToSurrogateSerializerFactory(),
            new FromSurrogateSerializerFactory(),
            new FSharpMapSerializerFactory(),
            new FSharpListSerializerFactory(),
            //order is important, try dictionaries before enumerables as dicts are also enumerable
            new ExceptionSerializerFactory(),
            new ImmutableCollectionsSerializerFactory(),
            new ExpandoObjectSerializerFactory(),
            new DefaultDictionarySerializerFactory(),
            new LinkedListSerializerFactory(),
            new DictionarySerializerFactory(),
            new HashSetSerializerFactory(),
            new ArraySerializerFactory(),
            new ISerializableSerializerFactory(), 
            new EnumerableSerializerFactory(),
            new EnumSerializerFactory()
        };

        internal readonly Type[] KnownTypes;
        internal readonly Dictionary<Type, ushort> KnownTypesDict = new Dictionary<Type, ushort>();

        internal readonly bool PreserveObjectReferences;
        internal readonly Surrogate[] Surrogates;
        internal readonly ValueSerializerFactory[] ValueSerializerFactories;

        public SerializerOptions(bool preserveObjectReferences = false,
            IEnumerable<Surrogate>? surrogates = null, IEnumerable<ValueSerializerFactory>? serializerFactories = null,
            IEnumerable<Type>? knownTypes = null)
        {
            Surrogates = surrogates?.ToArray() ?? EmptySurrogates;

            //use the default factories + any user defined
            ValueSerializerFactories =
                DefaultValueSerializerFactories.Concat(serializerFactories?.ToArray() ?? EmptyValueSerializerFactories)
                    .ToArray();

            KnownTypes = knownTypes?.ToArray() ?? new Type[] { };
            for (var i = 0; i < KnownTypes.Length; i++) KnownTypesDict.Add(KnownTypes[i], (ushort) i);

            PreserveObjectReferences = preserveObjectReferences;
        }
    }
}