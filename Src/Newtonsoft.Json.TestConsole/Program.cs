using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            string version = FileVersionInfo.GetVersionInfo(typeof (JsonConvert).Assembly.Location).FileVersion;
            Console.WriteLine("Json.NET Version: " + version);
            Console.ReadKey();

            Console.WriteLine("Doing stuff...");

            //PerformanceTests t = new PerformanceTests();
            //t.DeserializeLargeJson();

            //DeserializeLargeJson();
            //WriteLargeJson();
            DeserializeJson();

            Console.WriteLine();
            Console.WriteLine("Finished");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public static void DeserializeJson()
        {
            PerformanceTests t = new PerformanceTests();
            t.Iterations = 50000;
            t.Deserialize();
        }

        public static void DeserializeLargeJson()
        {
            PerformanceTests t = new PerformanceTests();
            t.DeserializeLargeJson();
        }

        public static void WriteLargeJson()
        {
            var json = System.IO.File.ReadAllText("large.json");

            IList<PerformanceTests.RootObject> o = JsonConvert.DeserializeObject<IList<PerformanceTests.RootObject>>(json);

            Console.WriteLine("Press any key to start serialize");
            Console.ReadKey();
            Console.WriteLine("Serializing...");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 100; i++)
            {
                using (StreamWriter file = System.IO.File.CreateText("largewrite.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, o);
                }
            }

            sw.Stop();

            Console.WriteLine("Finished. Total seconds: " + sw.Elapsed.TotalSeconds);
        }
    }
}
