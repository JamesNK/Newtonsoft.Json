using System;
using System.Collections.Generic;
using System.IO;
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

            //PerformanceTests t = new PerformanceTests();
            //t.DeserializeLargeJson();

            WriteLargeJson();

            Console.WriteLine();
            Console.WriteLine("Finished");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public static void WriteLargeJson()
        {
            var json = System.IO.File.ReadAllText("large.json");

            IList<PerformanceTests.RootObject> o = JsonConvert.DeserializeObject<IList<PerformanceTests.RootObject>>(json);

            Console.WriteLine("Press any key to start serialize");
            Console.ReadKey();

            for (int i = 0; i < 100; i++)
            {
                using (StreamWriter file = System.IO.File.CreateText("largewrite.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, o);
                }
            }
        }
    }
}
