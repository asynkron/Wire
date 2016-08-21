using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Bond;
using Bond.IO.Unsafe;
using Bond.Protocols;
using Newtonsoft.Json;
using ProtoBuf;
using Wire.PerfTest.Tests;
using Wire.SerializerFactories;
using Wire.ValueSerializers;

namespace Wire.PerfTest
{
   internal class Program
    {

        private static void Main(string[] args)
        {
            var t = new Thread(Run);
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();
            Console.ReadLine();
        }

       private static void Run()
       {
           var typicalPersonArrayTest = new TypicalPersonArrayTest();
           typicalPersonArrayTest.Run(1000);

           var typicalPersonTest = new TypicalPersonTest();
           typicalPersonTest.Run(100000);

           var typicalMessageArrayTest = new TypicalMessageArrayTest();
           typicalMessageArrayTest.Run(10000);

           var typicalMessageTest = new TypicalMessageTest();
           typicalMessageTest.Run(1000000);

           Console.ReadLine();
       }
    }
}