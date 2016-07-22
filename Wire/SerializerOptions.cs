using System;
using System.Collections.Generic;
using System.Linq;
using Wire.SerializerFactories;

namespace Wire
{
    public class SerializerOptions
    {
        internal static readonly Surrogate[] EmptySurrogates = new Surrogate[0];
        internal static readonly ValueSerializerFactory[] EmptyValueSerializerFactories = new ValueSerializerFactory[0];

        private static readonly ValueSerializerFactory[] DefaultValueSerializerFactories =
        {
            new MethodInfoSerializerFactory(), 
            new DelegateSerializerFactory(), 
            new ToSurrogateSerializerFactory(),
            new FromSurrogateSerializerFactory(),
            new FSharpListSerializerFactory(), 
            //order is important, try dictionaries before enumerables as dicts are also enumerable
            new ExceptionSerializerFactory(), 
            new ImmutableCollectionsSerializerFactory(),
            new DefaultDictionarySerializerFactory(),
            new DictionarySerializerFactory(),
            new ArraySerializerFactory(),
#if SERIALIZATION
            new ISerializableSerializerFactory(), //TODO: this will mess up the indexes in the serializer payload
#endif
            new EnumerableSerializerFactory(),
        };

        internal readonly bool VersionTolerance;
        internal readonly bool PreserveObjectReferences;
        internal readonly bool UseDynamicCode;
        internal readonly Surrogate[] Surrogates;
        internal readonly ValueSerializerFactory[] ValueSerializerFactories;
        internal readonly Type[] KnownTypes;

        public SerializerOptions(
            bool versionTolerance = false,
            bool preserveObjectReferences = false,
            bool? useDynamicCode = null,
            IEnumerable<Surrogate> surrogates = null,
            IEnumerable<ValueSerializerFactory> serializerFactories = null,
            IEnumerable<Type> knownTypes = null)
        {
            VersionTolerance = versionTolerance;
            PreserveObjectReferences = preserveObjectReferences;
            UseDynamicCode = useDynamicCode ?? GetDefaultUseDynamicCode();

            Surrogates = surrogates?.ToArray() ?? EmptySurrogates;

            //use the default factories + any user defined
            ValueSerializerFactories =
                DefaultValueSerializerFactories.Concat(serializerFactories?.ToArray() ?? EmptyValueSerializerFactories)
                    .ToArray();

            KnownTypes = knownTypes?.ToArray() ?? new Type[] {};
        }

        private static bool GetDefaultUseDynamicCode()
        {
            // TODO: check if system supports dynamic code or not. (it should be false on iOS.)
            return true;
        }
    }
}