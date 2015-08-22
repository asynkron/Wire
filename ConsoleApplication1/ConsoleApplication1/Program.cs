using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new byte[] {1, 2, 3, 4};
            var b = new byte[] { 1, 2, 3, 4 };
            Console.WriteLine(a == b);
        }
    }
}
