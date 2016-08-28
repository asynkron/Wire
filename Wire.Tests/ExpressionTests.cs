using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class ExpressionTests
    {
        public struct Dummy
        {
            public string TestField;
            public int TestProperty { get; set; }
            public string TestMethod(string input) => input;

            public Dummy(string testField) : this()
            {
                TestField = testField;
            }
        }

        public class DummyException : Exception
        {
            protected DummyException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

        [TestMethod]
        public void CanSerializeFieldInfo()
        {
            var fieldInfo = typeof(Dummy).GetField("TestField");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(fieldInfo, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<FieldInfo>(stream);
                Assert.AreEqual(fieldInfo, deserialized);
            }
        }

        [TestMethod]
        public void CanSerializePropertyInfo()
        {
            var propertyInfo = typeof(Dummy).GetProperty("TestProperty");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(propertyInfo, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<PropertyInfo>(stream);
                Assert.AreEqual(propertyInfo, deserialized);
            }
        }

        [TestMethod]
        public void CanSerializeMethodInfo()
        {
            var methodInfo = typeof(Dummy).GetMethod("TestMethod");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(methodInfo, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<MethodInfo>(stream);
                Assert.AreEqual(methodInfo, deserialized);
            }
        }

        [TestMethod]
        public void CanSerializeSymbolDocumentInfo()
        {
            var info = Expression.SymbolDocument("testFile");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(info, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<SymbolDocumentInfo>(stream);
                Assert.AreEqual(info.FileName, deserialized.FileName);
            }
        }

        [TestMethod]
        public void CanSerializeConstructorInfo()
        {
            var constructorInfo = typeof(Dummy).GetConstructor(new[] { typeof(string) });
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(constructorInfo, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<ConstructorInfo>(stream);
                Assert.AreEqual(constructorInfo, deserialized);
            }
        }

        [TestMethod]
        public void CanSerializeConstantExpression()
        {
            var expr = Expression.Constant(12);
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<ConstantExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Value, deserialized.Value);
                Assert.AreEqual(expr.Type, deserialized.Type);
            }
        }

        [TestMethod]
        public void CanSerializeUnaryExpression()
        {
            var expr = Expression.Decrement(Expression.Constant(1));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<UnaryExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Method, deserialized.Method);
                Assert.AreEqual(expr.Operand.ConstantValue(), deserialized.Operand.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeBinaryExpression()
        {
            var expr = Expression.Add(Expression.Constant(1), Expression.Constant(2));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<BinaryExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Method, deserialized.Method);
            }
        }

        [TestMethod]
        public void CanSerializeIndexExpression()
        {
            var value = new[] {1, 2, 3};
            var arrayExpr = Expression.Constant(value);
            var expr = Expression.ArrayAccess(arrayExpr, Expression.Constant(1));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<IndexExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Indexer, deserialized.Indexer);
                Assert.AreEqual(1, deserialized.Arguments.Count);
                Assert.AreEqual(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
                var actual = (int[])deserialized.Object.ConstantValue();
                Assert.AreEqual(value[0], actual[0]);
                Assert.AreEqual(value[1], actual[1]);
                Assert.AreEqual(value[2], actual[2]);
            }
        }

        [TestMethod]
        public void CanSerializeMemberAssignment()
        {
            var property = typeof(Dummy).GetProperty("TestProperty");
            var expr = Expression.Bind(property.SetMethod, Expression.Constant(9));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<MemberAssignment>(stream);
                Assert.AreEqual(expr.BindingType, deserialized.BindingType);
                Assert.AreEqual(expr.Member, deserialized.Member);
                Assert.AreEqual(expr.Expression.ConstantValue(), deserialized.Expression.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeConditionalExpression()
        {
            var expr = Expression.Condition(Expression.Constant(true), Expression.Constant(1), Expression.Constant(2));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<ConditionalExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Test.ConstantValue(), deserialized.Test.ConstantValue());
                Assert.AreEqual(expr.IfTrue.ConstantValue(), deserialized.IfTrue.ConstantValue());
                Assert.AreEqual(expr.IfFalse.ConstantValue(), deserialized.IfFalse.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeBlockExpression()
        {
            var expr = Expression.Block(new[] { Expression.Constant(1), Expression.Constant(2), Expression.Constant(3) });
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<BlockExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Expressions.Count, deserialized.Expressions.Count);
                Assert.AreEqual(expr.Expressions[0].ConstantValue(), deserialized.Expressions[0].ConstantValue());
                Assert.AreEqual(expr.Expressions[1].ConstantValue(), deserialized.Expressions[1].ConstantValue());
                Assert.AreEqual(expr.Result.ConstantValue(), deserialized.Result.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeLabelTarget()
        {
            var label = Expression.Label(typeof(int), "testLabel");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(label, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<LabelTarget>(stream);
                Assert.AreEqual(label.Name, deserialized.Name);
                Assert.AreEqual(label.Type, deserialized.Type);
            }
        }

        [TestMethod]
        public void CanSerializeLabelExpression()
        {
            var label = Expression.Label(typeof(int), "testLabel");
            var expr = Expression.Label(label, Expression.Constant(2));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<LabelExpression>(stream);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Target.Name, deserialized.Target.Name);
                Assert.AreEqual(expr.DefaultValue.ConstantValue(), deserialized.DefaultValue.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeMethodCallExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("TestMethod");
            var expr = Expression.Call(Expression.Constant(new Dummy()), methodInfo, Expression.Constant("test string"));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<MethodCallExpression>(stream);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Method, deserialized.Method);
                Assert.AreEqual(expr.Object.ConstantValue(), deserialized.Object.ConstantValue());
                Assert.AreEqual(1, deserialized.Arguments.Count);
                Assert.AreEqual(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeDefaultExpression()
        {
            var expr = Expression.Default(typeof(int));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<DefaultExpression>(stream);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
            }
        }

        [TestMethod]
        public void CanSerializeParameterExpression()
        {
            var expr = Expression.Parameter(typeof(int), "p1");
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<ParameterExpression>(stream);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Name, deserialized.Name);
            }
        }

        [TestMethod]
        public void CanSerializeCatchBlock()
        {
            var expr = Expression.Catch(typeof(DummyException), Expression.Constant(2));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<CatchBlock>(stream);
                Assert.AreEqual(expr.Test, deserialized.Test);
                Assert.AreEqual(expr.Body.ConstantValue(), deserialized.Body.ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeGotoExpression()
        {
            var label = Expression.Label(typeof(void), "testLabel");
            var expr = Expression.Continue(label);
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<GotoExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Kind, deserialized.Kind);
                Assert.AreEqual(expr.Target.Name, deserialized.Target.Name);
            }
        }

        [TestMethod]
        public void CanSerializeNewExpression()
        {
            var ctor = typeof(Dummy).GetConstructor(new[] { typeof(string) });
            var expr = Expression.New(ctor, Expression.Constant("test param"));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<NewExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Constructor, deserialized.Constructor);
                Assert.AreEqual(expr.Arguments.Count, deserialized.Arguments.Count);
                Assert.AreEqual(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeDebugInfoExpression()
        {
            var info = Expression.SymbolDocument("testFile");
            var expr = Expression.DebugInfo(info, 1, 2, 3, 4);
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<DebugInfoExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Document.FileName, deserialized.Document.FileName);
                Assert.AreEqual(expr.EndColumn, deserialized.EndColumn);
                Assert.AreEqual(expr.StartColumn, deserialized.StartColumn);
                Assert.AreEqual(expr.EndLine, deserialized.EndLine);
                Assert.AreEqual(expr.StartLine, deserialized.StartLine);
            }
        }

        [TestMethod]
        public void CanSerializeLambdaExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("TestMethod");
            var param = Expression.Parameter(typeof (Dummy), "dummy");
            var expr = Expression.Lambda(Expression.Call(param, methodInfo, Expression.Constant("s")), param);
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<LambdaExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Name, deserialized.Name);
                Assert.AreEqual(expr.TailCall, deserialized.TailCall);
                Assert.AreEqual(expr.ReturnType, deserialized.ReturnType);
                Assert.AreEqual(expr.Parameters.Count, deserialized.Parameters.Count);
                Assert.AreEqual(expr.Parameters[0].Name, deserialized.Parameters[0].Name);
            }
        }

        [TestMethod]
        public void CanSerializeInvocationExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("TestMethod");
            var param = Expression.Parameter(typeof(Dummy), "dummy");
            var lambda = Expression.Lambda(Expression.Call(param, methodInfo, Expression.Constant("s")), param);
            var expr = Expression.Invoke(lambda, Expression.Constant(new Dummy()));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<InvocationExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Arguments.Count, deserialized.Arguments.Count);
                Assert.AreEqual(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeElementInit()
        {
            var listAddMethod = typeof (List<int>).GetMethod("Add");
            var expr = Expression.ElementInit(listAddMethod, Expression.Constant(1));
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<ElementInit>(stream);
                Assert.AreEqual(expr.AddMethod, deserialized.AddMethod);
                Assert.AreEqual(1, deserialized.Arguments.Count);
                Assert.AreEqual(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
            }
        }

        [TestMethod]
        public void CanSerializeLoopExpression()
        {
            var breakLabel = Expression.Label(typeof (void), "break");
            var continueLabel = Expression.Label(typeof(void), "cont");
            var expr = Expression.Loop(Expression.Constant(2), breakLabel, continueLabel);
            var serializer = new Wire.Serializer();

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(expr, stream);
                stream.Position = 0;
                var deserialized = serializer.Deserialize<LoopExpression>(stream);
                Assert.AreEqual(expr.NodeType, deserialized.NodeType);
                Assert.AreEqual(expr.Type, deserialized.Type);
                Assert.AreEqual(expr.Body.ConstantValue(), deserialized.Body.ConstantValue());
                Assert.AreEqual(expr.BreakLabel.Name, deserialized.BreakLabel.Name);
                Assert.AreEqual(expr.ContinueLabel.Name, deserialized.ContinueLabel.Name);
            }
        }
    }

    internal static class ExpressionExtensions
    {
        public static object ConstantValue(this Expression expr) => ((ConstantExpression)expr).Value;
    }
}