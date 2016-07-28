using System;
using System.Reflection;
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
        private static readonly FieldInfo BoolField = typeof(Dummy).GetField(nameof(Dummy.BoolField));

        [TestMethod]
        public void CanModifyParameter()
        {
            var c = new IlCompiler<Action<Dummy>>();
            var param = c.Parameter<Dummy>("dummy");
            var write = c.WriteField(BoolField, param, c.Constant(true));
            c.Emit(write);
            var a = c.Compile();
            var dummy = new Dummy();
            a(dummy);
            Assert.AreEqual(true,dummy.BoolField);
        }

        [TestMethod]
        public void CanCreateEmptyMethodWithArguments()
        {
            var c = new IlCompiler<Action<bool>>();
            var a = c.Compile();
            a(true);
        }

        [TestMethod]
        public void CanCreateEmptyMethodWithReturnType()
        {
            var c = new IlCompiler<Func<bool>>();
            var b = c.Constant(true);
            c.Emit(b);
            var a = c.Compile();
            var res = a();
            Assert.AreEqual(true,res);
        }

        [TestMethod]
        public void CanReturnConstantString()
        {
            var c = new IlCompiler<Func<string>>();
            var b = c.Constant("hello");
            c.Emit(b);
            var a = c.Compile();
            var res = a();
            Assert.AreEqual("hello", res);
        }

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
            var c = new IlCompiler<Func<Dummy>>();
            var obj = c.NewObject(typeof(Dummy));
            c.Emit(obj);
            var a = c.Compile();
            a();
        }

        [TestMethod]
        public void CanStoreBoolInField()
        {
            var c = new IlCompiler<Action>();
            var True = c.Constant(true);
            var obj = c.NewObject(typeof(Dummy));
            var write = c.WriteField(BoolField, obj, True);
            c.Emit(write);
            var a = c.Compile();
            a();
        }

        [TestMethod]
        public void CanCastToAndFromObject()
        {
            var c = new IlCompiler<Action>();
            
            var True = c.Constant(true);
            var boxedBool = c.CastOrBox(True,typeof(object));
            var unboxedBool = c.CastOrUnbox(boxedBool, typeof(bool));

            var obj = c.NewObject(typeof(Dummy));
            var write = c.WriteField(BoolField, obj, unboxedBool);
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