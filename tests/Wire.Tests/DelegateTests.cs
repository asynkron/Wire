// -----------------------------------------------------------------------
//   <copyright file="DelegateTests.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Xunit;

namespace Wire.Tests
{
    public class DelegateTests
    {
        private static int StaticFunc(int a)
        {
            return a + 1;
        }

        [Fact]
        public void CanSerializeDelegate()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Action<Dummy> a = dummy => dummy.Prop = 1;
            serializer.Serialize(a, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Action<Dummy>>(stream);
            Assert.NotNull(res);

            var d = new Dummy {Prop = 0};
            res(d);
            Assert.Equal(1, d.Prop);
        }

        [Fact]
        public void CanSerializeMemberMethod()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Func<string> a = 123.ToString;
            serializer.Serialize(a, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Func<string>>(stream);
            Assert.NotNull(res);
            var actual = res();
            Assert.Equal("123", actual);
        }

        [Fact]
        public void CanSerializeObjectWithClosure()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            var hasClosure = new HasClosure();
            hasClosure.Create();

            serializer.Serialize(hasClosure, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<HasClosure>(stream);
            Assert.NotNull(res);
            var actual = res.Del();
            Assert.Equal(4, actual);
        }

        [Fact]
        public void CanSerializeStaticDelegate()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions());

            Func<int, int> fun = StaticFunc;

            serializer.Serialize(fun, stream);
            stream.Position = 0;
            var res = serializer.Deserialize<Func<int, int>>(stream);
            Assert.NotNull(res);
            var actual = res(4);

            Assert.Equal(5, actual);
        }

        public class Dummy
        {
            public int Prop { get; set; }
        }

        public class HasClosure
        {
            public Func<int> Del;

            public void Create()
            {
                var a = 3;
                Del = () => a + 1;
            }
        }
    }
}