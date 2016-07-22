using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nessos.FsPickler;

namespace Wire.PerformanceTests
{
    [TestClass]
    public class SpeedAndSizeTests
    {
        [Serializable]
        public class CyclicA
        {
            public CyclicB B { get; set; }
        }

        [Serializable]
        public class CyclicB
        {
            public CyclicA A { get; set; }
        }

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
            for (var i = 0; i < arr.Length; i++)
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
        public void TestDateTime()
        {
            Test(DateTime.Now);
        }

        [TestMethod]
        public void TestDateTimeArray()
        {
            var arr = new DateTime[200];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = DateTime.Now;
            }

            Test(arr);
        }

        [TestMethod]
        public void TestGuidArray()
        {
            var arr = new Guid[200];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = Guid.NewGuid();
            }

            Test(arr);
        }

        //fails as we are slower than FsPickler when serializing the integer array, we have slightly smaller payload though
        [TestMethod]
        public void TestIntArray()
        {
            var arr = new int[1000];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }

            Test(arr);
        }

        [TestMethod]
        public void TestStringShort()
        {
            
            Test("hello");
        }

        [TestMethod]
        public void TestStringLong()
        {
            var sb = new StringBuilder();
            sb.Append("Hello");
            for (var i = 0; i < 14; i++)
            {
                sb.Append(sb);
            }
            Test(sb.ToString());
        }


        //TODO: fails as FsPickler uses 2 bytes for length encoding instead of 4 as Wire does
        //our payload gets bigger
        [TestMethod]
        public void TestStringArray()
        {
            var arr = new string[1000];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = "hello";
            }

            Test(arr);
        }


        [TestMethod]
        public void TestByteArray()
        {
            var arr = new byte[1000];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = (byte)(i%255);
            }

            Test(arr);
        }

        [TestMethod]
        public void TestType()
        {
            Test(typeof(int));
        }



        ///fails big time, we are writing the entire qualified type name for each entry, fs pickler does not.
        [TestMethod]
        public void TestTypeArray()
        {
            var arr = new Type[100];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = typeof (int);
            }

            Test(arr);
        }

        [TestMethod]
        public void TestGuid()
        {
            Test(Guid.NewGuid());
        }

        //fails as we are slower than FsPickler when serializing the integer array, we have slightly smaller payload though
        [TestMethod]
        public void TestBoolArray()
        {
            var arr = new bool[1000];
            for (var i = 0; i < arr.Length; i++)
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

        [TestMethod,Ignore,Description("Works but is slower than FS Pickler")]
        public void TestTupleArray()
        {
            var arr = new Tuple<int,int>[100];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = Tuple.Create(i, 999 - i);
            }

            Test(arr);
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
        //fails as our payload is bigger, probably due to qualified typename, we are faster though
        public void TestCyclic()
        {
            var a = new CyclicA();
            var b = new CyclicB();
            a.B = b;
            b.A = a;

            Test(a);
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

        //TODO: F# lists are recursive cons lists, wire currently write those objects
        //we could optimize to convert to array and write it all in one go
        [TestMethod]
        public void TestFSharpList()
        {
            var list = ListModule.OfArray(new[] { 1, 2, 3, 4,5,6,7,8,9,0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
            Test(list);
        }

        private void Test(object value)
        {
            Serializer wireSerializer = new Serializer(new SerializerOptions(false,true,null, null));
            var pickler = FsPickler.CreateBinarySerializer();

            double wireTs;
            double picklerTs;
            long wireSize;
            long picklerSize;
            const int repeat = 10000;
            {
                var wireStream = new MemoryStream();
                wireSerializer.Serialize(value, wireStream);
                
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < repeat; i++)
                {
                    wireStream = new MemoryStream();
                    WireSerialize(value, wireSerializer, wireStream);
                }
                sw.Stop();
                wireTs = sw.Elapsed.TotalMilliseconds;
                wireSize = wireStream.ToArray().Length;
            }


            //using (MemoryStream picklerStram = new MemoryStream())
            {
                var picklerStram = new MemoryStream();
                pickler.Serialize(picklerStram,value);
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < repeat; i++)
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
            Console.WriteLine($"Wire is {picklerTs / wireTs :n2} times faster than FsPickler");
            Console.WriteLine();
            Console.WriteLine($"Wire payload size      {wireSize} bytes");
            Console.WriteLine($"FsPickler payload size {picklerSize} bytes");
            Console.WriteLine($"Wire is {picklerSize / (double)wireSize:n2} times smaller than FsPickler");

            //assert that we are in a 10% margin of FsPickler
            Assert.IsTrue(wireTs <= picklerTs * 1.1, "Wire was slower than FsPickler");
            Assert.IsTrue(wireSize <= picklerSize * 1.1, "Wire payload was larger than FsPickler");
        }

        private static void WireSerialize(object value, Serializer wireSerializer, MemoryStream wireStream)
        {
            wireSerializer.Serialize(value, wireStream);
        }
    }
}
