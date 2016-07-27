using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wire.Compilation;

namespace Wire.Tests
{
    public class Dummy
    {
        public bool BoolField;
    }

    [TestClass]
    public class IlCompilerTests
    {
        [TestMethod]
        public void CanCreateEmptyMethod()
        {
            var c = new IlCompiler<Action>();
            var a = c.Compile();
            a();
        }

        [TestMethod]
        public void CanCreateObject()
        {
            var c = new IlCompiler<Action>();
            var obj = c.NewObject(typeof(Dummy));
            c.Emit(obj);
            var a = c.Compile();
            a();
        }

        [TestMethod]
        public void CanStoreBoolInField()
        {
            var c = new IlCompiler<Action>();
            var obj = c.NewObject(typeof(Dummy));
            var True = c.Constant(true);
            var write = c.WriteField(typeof(Dummy).GetField("BoolField"), obj, True);
            c.Emit(write);
            var a = c.Compile();
            a();
        }

        [TestMethod]
        public void CanCreateObjectAndStoreInVar()
        {
            var c = new IlCompiler<Action>();
            var variable = c.Variable<Dummy>("dummy");
            var obj = c.NewObject(typeof(Dummy));
            var assign = c.WriteVar(variable, obj);
            c.Emit(assign);
            var a = c.Compile();
            a();
        }
    }
}