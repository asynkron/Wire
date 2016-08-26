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
            //var largeStructTest = new LargeStructTest();
            //largeStructTest.Run(1000000);

            //var guidArrayTest = new GuidArrayTest();
            //guidArrayTest.Run(t);

            var guidTest = new GuidTest();
            guidTest.Run(2000000);
            //var typicalPersonArrayTest = new TypicalPersonArrayTest();
            //typicalPersonArrayTest.Run(1000);

            //var typicalPersonTest = new TypicalPersonTest();
            //typicalPersonTest.Run(100000);

            //var typicalMessageArrayTest = new TypicalMessageArrayTest();
            //typicalMessageArrayTest.Run(10000);

            var typicalMessageTest = new TypicalMessageTest();
            typicalMessageTest.Run(1000000);

            Console.ReadLine();
        }
    }
}