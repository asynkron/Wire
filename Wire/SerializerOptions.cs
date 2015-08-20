using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Wire
{
    public class SerializerOptions
    {
        public readonly bool PreserveObjectReferences;
        public readonly Surrogate[] Surrogates;
        public readonly bool VersionTolerance;
        private static readonly Surrogate[] EmptySurrogates = new Surrogate[0];

        public SerializerOptions(bool versionTolerance = false,IEnumerable<Surrogate> surrogates = null,bool preserveObjectReferences = false)
        {
            VersionTolerance = versionTolerance;
            Surrogates = surrogates?.ToArray() ?? EmptySurrogates;
            PreserveObjectReferences = preserveObjectReferences;        
        }
    }

    public class Surrogate
    {
        public Type From { get;protected set; }
        public Type To { get; protected set; }
        public Func<object, object> FromSurrogate { get;protected set; }
        public Func<object, object> ToSurrogate { get; protected set; }

        public static Surrogate Create<TSource, TSurrogate>(Func<TSource, TSurrogate> toSurrogate, Func<TSurrogate,TSource> fromSurrogate)
        {
            return new Surrogate<TSource, TSurrogate>(toSurrogate, fromSurrogate);
        }
    }

    public class Surrogate<TSource, TSurrogate> : Surrogate
    {
        public Surrogate(Func<TSource, TSurrogate> toSurrogate,Func<TSurrogate,TSource> fromSurrogate)
        {
            ToSurrogate = from => toSurrogate((TSource)from);
            FromSurrogate = to => fromSurrogate((TSurrogate)to);
            From = typeof (TSource);
            To = typeof (TSurrogate);
        }

    }
}