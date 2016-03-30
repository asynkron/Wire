using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wire
{
    public static class TypeExtensions
    {
        public static object GetEmptyObject(this Type type)
        {
#if false
            return FormatterServices.GetUninitializedObject(type);
#else
            return Activator.CreateInstance(type); //TODO fix
#endif
        }
    }
}
