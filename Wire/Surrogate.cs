using System;

namespace Wire
{
    public class Surrogate<TSource, TSurrogate> : Surrogate
    {
        public Surrogate(Func<TSource, TSurrogate> toSurrogate, Func<TSurrogate, TSource> fromSurrogate)
        {
            ToSurrogate = from => toSurrogate((TSource) from);
            FromSurrogate = to => fromSurrogate((TSurrogate) to);
            From = typeof (TSource);
            To = typeof (TSurrogate);
        }
    }

    public class Surrogate
    {
        public Type From { get; protected set; }
        public Type To { get; protected set; }
        public Func<object, object> FromSurrogate { get; protected set; }
        public Func<object, object> ToSurrogate { get; protected set; }

        public static Surrogate Create<TSource, TSurrogate>(Func<TSource, TSurrogate> toSurrogate,
            Func<TSurrogate, TSource> fromSurrogate)
        {
            return new Surrogate<TSource, TSurrogate>(toSurrogate, fromSurrogate);
        }
    }
}