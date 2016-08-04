using System;
using System.Collections.Generic;

namespace Wire
{
    public class FastTypeUShortDictionary
    {
        private int _length;
        private Type _firstType;
        private ushort _firstValue;
        private Dictionary<Type, ushort> _all;

        public bool TryGetValue(Type key, out ushort value)
        {
            switch (_length)
            {
                case 0:
                    value = 0;
                    return false;
                case 1:
                    if (key == _firstType)
                    {
                        value = _firstValue;
                        return true;
                    }
                    value = 0;
                    return false;
                default:
                    return _all.TryGetValue(key, out value);
            }
        }

        public void Add(Type type, ushort value)
        {
            switch (_length)
            {
                case 0:
                    _firstType = type;
                    _firstValue = value;
                    _length = 1;
                    break;
                case 1:
                    _all = new Dictionary<Type, ushort> {{_firstType, _firstValue}, {type, value}};
                    _length = 2;
                    break;
                default:
                    _all.Add(type,value);
                    _length ++;
                    break;
            }
        }
    }
}
