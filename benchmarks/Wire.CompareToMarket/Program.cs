// -----------------------------------------------------------------------
//   <copyright file="Program.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading;
using Wire.PerfTest.Tests;

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
            var typicalMessageTest = new TypicalMessageTest();
            typicalMessageTest.Run(10000000);
            
            var largeStructTest = new LargeStructTest();
            largeStructTest.Run(10000000);

            var guidArrayTest = new GuidArrayTest();
            guidArrayTest.Run(30000);

            var guidTest = new GuidTest();
            guidTest.Run(1000000);
            var typicalPersonArrayTest = new TypicalPersonArrayTest();
            typicalPersonArrayTest.Run(1000);

            var typicalPersonTest = new TypicalPersonTest();
            typicalPersonTest.Run(100000);

            var typicalMessageArrayTest = new TypicalMessageArrayTest();
            typicalMessageArrayTest.Run(10000);



            Console.ReadLine();
        }
    }
}