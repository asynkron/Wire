using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wire.Compilation;
using Wire.Extensions;

namespace Wire.Tests
{
    public class Poco
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public Guid GuidProp { get; set; }
        public DateTime DateProp { get; set; }
    }

    public class Dummy
    {
        public bool BoolField;

        public void SetBool()
        {
            BoolField = true;
        }

        public static void SetStatic(Dummy d)
        {
            d.BoolField = true;
        }
    }

    public class FakeTupleString
    {
        public string Item1 { get; }

        public FakeTupleString(string item1)
        {
            Item1 = item1;
        }
    }

    [TestClass]
    public class IlCompilerTests
    {
        private static readonly FieldInfo BoolField = typeof(Dummy).GetField(nameof(Dummy.BoolField));
        private static readonly MethodInfo SetBool = typeof(Dummy).GetMethod(nameof(Dummy.SetBool));
        private static readonly MethodInfo SetStatic = typeof(Dummy).GetMethod(nameof(Dummy.SetStatic));

        [TestMethod]
        public void CanCallStaticMethodUsingParameter()
        {
            var c = new IlCompiler<Action<Dummy>>();
            var param = c.Parameter<Dummy>("dummy");
            c.EmitStaticCall(SetStatic, param);
            var a = c.Compile();
            var dummy = new Dummy();
            a(dummy);
            Assert.AreEqual(true, dummy.BoolField);
        }

        [TestMethod]
        public void CanCallInstanceMethodOnParameter()
        {
            var c = new IlCompiler<Action<Dummy>>();
            var param = c.Parameter<Dummy>("dummy");
            c.EmitCall(SetBool, param);            
            var a = c.Compile();
            var dummy = new Dummy();
            a(dummy);
            Assert.AreEqual(true, dummy.BoolField);
        }


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
            var boxedBool = c.Convert(True,typeof(object));
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


        [TestMethod]
        public void ReadSimulationFakeTupleString()
        {
            var value = new FakeTupleString("Hello");
            var type = value.GetType();
            var serializer = new Serializer(new SerializerOptions(knownTypes: new List<Type>() { type }));
            var session = new DeserializerSession(serializer);
            var stream = new MemoryStream();

            serializer.Serialize(value, stream);
            var bytes = stream.ToArray();
            stream.Position = 3; //skip forward to payload
            var fields = type.GetFieldInfosForType();

            var readAllFields = GetDelegate(type, fields, serializer);

            var x = (FakeTupleString)readAllFields(stream, session);
            Assert.AreEqual(value.Item1, x.Item1);
        }


        [TestMethod]
        public void ReadSimulationOptionString()
        {
            var value = FSharpOption<string>.Some("abc");
            var type = value.GetType();
            var serializer = new Serializer(new SerializerOptions(knownTypes: new List<Type>() { type }));
            var session = new DeserializerSession(serializer);
            var stream = new MemoryStream();

            serializer.Serialize(value, stream);
            stream.Position = 3; //skip forward to payload
            var fields = ReflectionEx.GetFieldInfosForType(type);

            var readAllFields = GetDelegate(type, fields, serializer);

            var x = (FSharpOption<string>)readAllFields(stream, session);
            Assert.AreEqual(value.Value, x.Value);
        }

        [TestMethod]
        public void ReadSimulationTupleString()
        {
            var value = Tuple.Create("Hello");
            var type = value.GetType();
            var serializer = new Serializer(new SerializerOptions(knownTypes: new List<Type>() { type }));
            var session = new DeserializerSession(serializer);
            var stream = new MemoryStream();

            serializer.Serialize(value, stream);
            var bytes = stream.ToArray();
            stream.Position = 3; //skip forward to payload
            var fields = type.GetFieldInfosForType();

            var readAllFields = GetDelegate(type, fields, serializer);

            var x = (Tuple<string>)readAllFields(stream, session);
            Assert.AreEqual(value.Item1, x.Item1);
        }

        [TestMethod]
        public void ReadSimulation()
        {
            var serializer = new Serializer(new SerializerOptions(knownTypes:new List<Type>() {typeof(Poco)}));
            var session = new DeserializerSession(serializer);
            var stream = new MemoryStream();
            var poco = new Poco()
            {
                StringProp = "hello",
                GuidProp = Guid.NewGuid(),
                IntProp = 123,
                DateProp = DateTime.Now,
            };
            serializer.Serialize(poco,stream);
            stream.Position = 3; //skip forward to payload

            var type = typeof(Poco);
            var fields = type.GetFieldInfosForType();

            var readAllFields = GetDelegate(type, fields, serializer);

            var x = (Poco)readAllFields(stream, session);
            Assert.AreEqual(poco.DateProp, x.DateProp);
            Assert.AreEqual(poco.GuidProp, x.GuidProp);
            Assert.AreEqual(poco.IntProp, x.IntProp);
            Assert.AreEqual(poco.StringProp, x.StringProp);
        }

        private static ObjectReader GetDelegate(Type type, FieldInfo[] fields, Serializer serializer)
        {
            var c = new IlCompiler<ObjectReader>();
            var stream = c.Parameter<Stream>("stream");
            var session = c.Parameter<DeserializerSession>("session");
            var newExpression = c.NewObject(type);
            var target = c.Variable<object>("target");
            var assignNewObjectToTarget = c.WriteVar(target, newExpression);

            c.Emit(assignNewObjectToTarget);

            var size = c.Constant(16);
            var buffer = c.Variable<byte[]>(DefaultCodeGenerator.PreallocatedByteBuffer);
            var bufferValue = c.Call(typeof(DeserializerSession).GetMethod("GetBuffer"), session, size);
            var assignBuffer = c.WriteVar(buffer, bufferValue);
            c.Emit(assignBuffer);
            
            var typedTarget = c.CastOrUnbox(target, type);
            foreach (var field in fields)
            {
                var s = serializer.GetSerializerByType(field.FieldType);
                var read = s.EmitReadValue(c, stream, session, field);
                
                var assignReadToField = c.WriteField(field, typedTarget, read);
                c.Emit(assignReadToField);
            }
            c.Emit(target);

            var readAllFields = c.Compile();
            return readAllFields;
        }
    }
}