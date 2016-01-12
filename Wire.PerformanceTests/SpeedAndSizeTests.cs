using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessos.FsPickler;

namespace Wire.PerformanceTests
{
    [TestClass]
    public class SpeedAndSizeTests
    {
        [Serializable]
        public class Poco
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestPocoArray()
        {
            var arr = new Poco[200];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new Poco()
                {
                    Name = "Foo" + i,
                    Age = i
                };
            }

            Test(arr);
        }

        [TestMethod]
        public void TestDateTimeArray()
        {
            var arr = new DateTime[200];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = DateTime.Now;
            }

            Test(arr);
        }

        [TestMethod]
        public void TestGuidArray()
        {
            var arr = new Guid[200];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Guid.NewGuid();
            }

            Test(arr);
        }

        [TestMethod]
        public void TestIntArray()
        {
            var arr = new int[1000];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }

            Test(arr);
        }

        [TestMethod]
        public void TestShortString()
        {
            
            Test("hello");
        }

        [TestMethod]
        public void TestLongString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hello");
            for (int i = 0; i < 10; i++)
            {
                sb.Append(sb);
            }
            Test(sb.ToString());
        }


        [TestMethod]
        public void TestStringArray()
        {
            var arr = new string[1000];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = "hello" + i;
            }

            Test(arr);
        }

        [TestMethod]
        public void TestBoolArray()
        {
            var arr = new bool[1000];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i % 2 == 0;
            }

            Test(arr);
        }


        [TestMethod]
        public void TestTuple()
        {
            Test(Tuple.Create(123,456));
        }

        [TestMethod]
        public void TestList()
        {
            var list = new List<int>()
            {
                123,
                456,
                789,
                111,
                222
            };
            Test(list);
        }

        [TestMethod]
        public void TestInt()
        {
            Test(123456);
        }

        [TestMethod]
        public void TestPocoSmall()
        {
            Test(new Poco()
            {
                Age = 40,
                Name = "Roger"
            });
        }

        [TestMethod]
        public void TestIntDictionary()
        {
            var dict = new Dictionary<int,int>()
            {
                [123] = 123,
                [555] = 789,
                [666] = 999,
            };
            Test(dict);
        }

        private void Test(object value)
        {
            Serializer wireSerializer = new Serializer(new SerializerOptions(false,null,true,null));
            var pickler = FsPickler.CreateBinarySerializer();

            double wireTs;
            double picklerTs;
            long wireSize;
            long picklerSize;
            const int repeat = 10000;
            {
                MemoryStream wireStream = new MemoryStream();
                wireSerializer.Serialize(value, wireStream);
                
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < repeat; i++)
                {
                    wireStream = new MemoryStream();
                    wireSerializer.Serialize(value, wireStream);
                }
                sw.Stop();
                wireTs = sw.Elapsed.TotalMilliseconds;
                wireSize = wireStream.ToArray().Length;
            }


            //using (MemoryStream picklerStram = new MemoryStream())
            {
                MemoryStream picklerStram = new MemoryStream();
                pickler.Serialize(picklerStram,value);
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < repeat; i++)
                {
                    picklerStram = new MemoryStream();
                    pickler.Serialize(picklerStram, value);
                }
                sw.Stop();
                picklerTs = sw.Elapsed.TotalMilliseconds;
                picklerSize = picklerStram.ToArray().Length;
            }
            Console.WriteLine($"Serializing {value.GetType().Name} {repeat:n0} times");
            Console.WriteLine();
            Console.WriteLine($"Wire elapsed time      {wireTs:n0} MS");            
            Console.WriteLine($"FsPickler elapsed time {picklerTs:n0} MS");
            Console.WriteLine();
            Console.WriteLine($"Wire payload size      {wireSize} bytes");
            Console.WriteLine($"FsPickler payload size {picklerSize} bytes");

            //assert that we are in a 10% margin of FsPickler
            Assert.IsTrue(wireTs <= picklerTs * 1.1, "Wire was slower than FsPickler");
            Assert.IsTrue(wireSize <= picklerSize * 1.1, "Wire payload was larger than FsPickler");
        }
    }
}
