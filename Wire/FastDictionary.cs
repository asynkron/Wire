using System;
using System.Collections.Generic;

namespace Wire
{
    public class FastTypeUShortDictionary
    {
        private int _length = 0;
        private Tuple<Type, ushort> _first;
        private Dictionary<Type, ushort> _all;

        public bool TryGetValue(Type key, out ushort value)
        {
            switch (_length)
            {
                case 0:
                    value = 0;
                    return false;
                case 1:
                    if (key == _first.Item1)
                    {
                        value = _first.Item2;
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
                    _first = Tuple.Create(type, value);
                    _length = 1;
                    break;
                case 1:
                    _all = new Dictionary<Type, ushort> {{_first.Item1, _first.Item2}, {type, value}};
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
