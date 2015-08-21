using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wire.ValueSerializers;

namespace Wire.Converters
{
    public class EnumerableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer Serializer, Type type)
        {
            return false;

            if (typeof (IEnumerable).IsAssignableFrom(type) && type.GetMethod("AddRange") != null)
                return true;

            return false;
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type, ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            throw new NotImplementedException();
        }
    }
}
