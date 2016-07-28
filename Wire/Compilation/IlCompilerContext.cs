using System;
using System.Reflection.Emit;

namespace Wire.Compilation
{
    public class IlCompilerContext
    {
        private int _stackDepth;

        public IlCompilerContext(ILGenerator il, Type selfType)
        {
            Il = il;
            SelfType = selfType;
        }

        public ILGenerator Il { get; }


        public int StackDepth
        {
            get { return _stackDepth; }
            set
            {
                _stackDepth = value;
                if (value < 0)
                {
                    throw new NotSupportedException("Stack depth can not be less than 0");
                }
            }
        }

        public Type SelfType { get; }

    }
}