using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Tests;

namespace Newtonsoft.Json.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Json.NET Test Console");
            Console.ReadKey();
            Console.WriteLine("Doing stuff...");

            PerformanceTests t = new PerformanceTests();

            //t.BenchmarkSerializeMethod(PerformanceTests.SerializeMethod.JsonNet, new { hello = "world" });
            t.DeserializeLargeJson();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
