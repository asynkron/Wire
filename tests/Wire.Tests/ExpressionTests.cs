// -----------------------------------------------------------------------
//   <copyright file="ExpressionTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;

namespace Wire.Tests
{
    public class ExpressionTests
    {
        [Fact]
        public void CanSerializeBinaryExpression()
        {
            var expr = Expression.Add(Expression.Constant(1), Expression.Constant(2));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<BinaryExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Method, deserialized.Method);
        }

        [Fact]
        public void CanSerializeBlockExpression()
        {
            var expr = Expression.Block(new[] {Expression.Constant(1), Expression.Constant(2), Expression.Constant(3)});
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<BlockExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Expressions.Count, deserialized.Expressions.Count);
            Assert.Equal(expr.Expressions[0].ConstantValue(), deserialized.Expressions[0].ConstantValue());
            Assert.Equal(expr.Expressions[1].ConstantValue(), deserialized.Expressions[1].ConstantValue());
            Assert.Equal(expr.Result.ConstantValue(), deserialized.Result.ConstantValue());
        }


        [Fact]
        public void CanSerializeCatchBlock()
        {
            var expr = Expression.Catch(typeof(DummyException), Expression.Constant(2));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<CatchBlock>(stream);
            Assert.Equal(expr.Test, deserialized.Test);
            Assert.Equal(expr.Body.ConstantValue(), deserialized.Body.ConstantValue());
        }


        [Fact]
        public void CanSerializeConditionalExpression()
        {
            var expr = Expression.Condition(Expression.Constant(true), Expression.Constant(1), Expression.Constant(2));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<ConditionalExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Test.ConstantValue(), deserialized.Test.ConstantValue());
            Assert.Equal(expr.IfTrue.ConstantValue(), deserialized.IfTrue.ConstantValue());
            Assert.Equal(expr.IfFalse.ConstantValue(), deserialized.IfFalse.ConstantValue());
        }

        [Fact]
        public void CanSerializeConstantExpression()
        {
            var expr = Expression.Constant(12);
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<ConstantExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Value, deserialized.Value);
            Assert.Equal(expr.Type, deserialized.Type);
        }

        [Fact]
        public void CanSerializeConstructorInfo()
        {
            var constructorInfo = typeof(Dummy).GetConstructor(new[] {typeof(string)});
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(constructorInfo, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<ConstructorInfo>(stream);
            Assert.Equal(constructorInfo, deserialized);
        }

        [Fact]
        public void CanSerializeDebugInfoExpression()
        {
            var info = Expression.SymbolDocument("testFile");
            var expr = Expression.DebugInfo(info, 1, 2, 3, 4);
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<DebugInfoExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Document.FileName, deserialized.Document.FileName);
            Assert.Equal(expr.EndColumn, deserialized.EndColumn);
            Assert.Equal(expr.StartColumn, deserialized.StartColumn);
            Assert.Equal(expr.EndLine, deserialized.EndLine);
            Assert.Equal(expr.StartLine, deserialized.StartLine);
        }

        [Fact]
        public void CanSerializeDefaultExpression()
        {
            var expr = Expression.Default(typeof(int));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<DefaultExpression>(stream);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
        }

        [Fact]
        public void CanSerializeElementInit()
        {
            var listAddMethod = typeof(List<int>).GetMethod("Add");
            var expr = Expression.ElementInit(listAddMethod, Expression.Constant(1));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<ElementInit>(stream);
            Assert.Equal(expr.AddMethod, deserialized.AddMethod);
            Assert.Equal(1, deserialized.Arguments.Count);
            Assert.Equal(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
        }

        [Fact]
        public void CanSerializeFieldInfo()
        {
            var fieldInfo = typeof(Dummy).GetField("TestField");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(fieldInfo, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<FieldInfo>(stream);
            Assert.Equal(fieldInfo, deserialized);
        }

        [Fact]
        public void CanSerializeGotoExpression()
        {
            var label = Expression.Label(typeof(void), "testLabel");
            var expr = Expression.Continue(label);
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<GotoExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Kind, deserialized.Kind);
            Assert.Equal(expr.Target.Name, deserialized.Target.Name);
        }

        [Fact]
        public void CanSerializeIndexExpression()
        {
            var value = new[] {1, 2, 3};
            var arrayExpr = Expression.Constant(value);
            var expr = Expression.ArrayAccess(arrayExpr, Expression.Constant(1));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<IndexExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Indexer, deserialized.Indexer);
            Assert.Equal(1, deserialized.Arguments.Count);
            Assert.Equal(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
            var actual = (int[]) deserialized.Object.ConstantValue();
            Assert.Equal(value[0], actual[0]);
            Assert.Equal(value[1], actual[1]);
            Assert.Equal(value[2], actual[2]);
        }

        [Fact]
        public void CanSerializeInvocationExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("Fact");
            var param = Expression.Parameter(typeof(Dummy), "dummy");
            var lambda = Expression.Lambda(Expression.Call(param, methodInfo, Expression.Constant("s")), param);
            var expr = Expression.Invoke(lambda, Expression.Constant(new Dummy()));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<InvocationExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Arguments.Count, deserialized.Arguments.Count);
            Assert.Equal(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
        }

        [Fact]
        public void CanSerializeLabelExpression()
        {
            var label = Expression.Label(typeof(int), "testLabel");
            var expr = Expression.Label(label, Expression.Constant(2));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<LabelExpression>(stream);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Target.Name, deserialized.Target.Name);
            Assert.Equal(expr.DefaultValue.ConstantValue(), deserialized.DefaultValue.ConstantValue());
        }

        [Fact]
        public void CanSerializeLabelTarget()
        {
            var label = Expression.Label(typeof(int), "testLabel");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(label, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<LabelTarget>(stream);
            Assert.Equal(label.Name, deserialized.Name);
            Assert.Equal(label.Type, deserialized.Type);
        }

        [Fact]
        public void CanSerializeLambdaExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("Fact");
            var param = Expression.Parameter(typeof(Dummy), "dummy");
            var expr = Expression.Lambda(Expression.Call(param, methodInfo, Expression.Constant("s")), param);
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<LambdaExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Name, deserialized.Name);
            Assert.Equal(expr.TailCall, deserialized.TailCall);
            Assert.Equal(expr.ReturnType, deserialized.ReturnType);
            Assert.Equal(expr.Parameters.Count, deserialized.Parameters.Count);
            Assert.Equal(expr.Parameters[0].Name, deserialized.Parameters[0].Name);
        }

        [Fact]
        public void CanSerializeLoopExpression()
        {
            var breakLabel = Expression.Label(typeof(void), "break");
            var continueLabel = Expression.Label(typeof(void), "cont");
            var expr = Expression.Loop(Expression.Constant(2), breakLabel, continueLabel);
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<LoopExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Body.ConstantValue(), deserialized.Body.ConstantValue());
            Assert.Equal(expr.BreakLabel.Name, deserialized.BreakLabel.Name);
            Assert.Equal(expr.ContinueLabel.Name, deserialized.ContinueLabel.Name);
        }

        [Fact]
        public void CanSerializeMemberAssignment()
        {
            var property = typeof(Dummy).GetProperty("TestProperty");
            var expr = Expression.Bind(property.SetMethod, Expression.Constant(9));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<MemberAssignment>(stream);
            Assert.Equal(expr.BindingType, deserialized.BindingType);
            Assert.Equal(expr.Member, deserialized.Member);
            Assert.Equal(expr.Expression.ConstantValue(), deserialized.Expression.ConstantValue());
        }

        [Fact]
        public void CanSerializeMethodCallExpression()
        {
            var methodInfo = typeof(Dummy).GetMethod("Fact");
            var expr = Expression.Call(Expression.Constant(new Dummy()), methodInfo,
                Expression.Constant("test string"));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<MethodCallExpression>(stream);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Method, deserialized.Method);
            Assert.Equal(expr.Object.ConstantValue(), deserialized.Object.ConstantValue());
            Assert.Equal(1, deserialized.Arguments.Count);
            Assert.Equal(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
        }

        [Fact]
        public void CanSerializeMethodInfo()
        {
            var methodInfo = typeof(Dummy).GetMethod("Fact");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(methodInfo, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<MethodInfo>(stream);
            Assert.Equal(methodInfo, deserialized);
        }

        [Fact]
        public void CanSerializeNewExpression()
        {
            var ctor = typeof(Dummy).GetConstructor(new[] {typeof(string)});
            var expr = Expression.New(ctor, Expression.Constant("test param"));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<NewExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Constructor, deserialized.Constructor);
            Assert.Equal(expr.Arguments.Count, deserialized.Arguments.Count);
            Assert.Equal(expr.Arguments[0].ConstantValue(), deserialized.Arguments[0].ConstantValue());
        }

        [Fact]
        public void CanSerializeParameterExpression()
        {
            var expr = Expression.Parameter(typeof(int), "p1");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<ParameterExpression>(stream);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Name, deserialized.Name);
        }

        [Fact]
        public void CanSerializePropertyInfo()
        {
            var propertyInfo = typeof(Dummy).GetProperty("TestProperty");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(propertyInfo, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<PropertyInfo>(stream);
            Assert.Equal(propertyInfo, deserialized);
        }

        [Fact]
        public void CanSerializeSymbolDocumentInfo()
        {
            var info = Expression.SymbolDocument("testFile");
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(info, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<SymbolDocumentInfo>(stream);
            Assert.Equal(info.FileName, deserialized.FileName);
        }

        [Fact]
        public void CanSerializeUnaryExpression()
        {
            var expr = Expression.Decrement(Expression.Constant(1));
            var serializer = new Serializer();

            using var stream = new MemoryStream();
            serializer.Serialize(expr, stream);
            stream.Position = 0;
            var deserialized = serializer.Deserialize<UnaryExpression>(stream);
            Assert.Equal(expr.NodeType, deserialized.NodeType);
            Assert.Equal(expr.Type, deserialized.Type);
            Assert.Equal(expr.Method, deserialized.Method);
            Assert.Equal(expr.Operand.ConstantValue(), deserialized.Operand.ConstantValue());
        }

        public struct Dummy
        {
            public string TestField;
            public int TestProperty { get; set; }

            public string Fact(string input)
            {
                return input;
            }

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
    }

    internal static class ExpressionExtensions
    {
        public static object ConstantValue(this Expression expr)
        {
            return ((ConstantExpression) expr).Value;
        }
    }
}