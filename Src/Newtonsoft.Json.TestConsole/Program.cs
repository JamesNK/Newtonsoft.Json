#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests;
using Newtonsoft.Json.Tests.Converters;

namespace Newtonsoft.Json.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Json.NET Test Console");

            string version = FileVersionInfo.GetVersionInfo(typeof(JsonConvert).Assembly.Location).FileVersion;
            Console.WriteLine("Json.NET Version: " + version);
            Console.ReadKey();

            Console.WriteLine("Doing stuff...");

            //PerformanceTests t = new PerformanceTests();
            //t.DeserializeLargeJson();

            //PerformanceTests t = new PerformanceTests();
            TokenWriteToAsync();
            //t.Iterations = 50000;
            //t.BenchmarkDeserializeMethod<TestClass>(PerformanceTests.SerializeMethod.JsonNet, PerformanceTests.JsonText);

            //Console.WriteLine("Wait to do stuff again");
            //Console.ReadKey();

            //t.BenchmarkDeserializeMethod<TestClass>(PerformanceTests.SerializeMethod.JsonNet, PerformanceTests.JsonText);

            //ReadLargeJson();
            //WriteLargeJson();
            //DeserializeJson();
            //ReadLargeJson();
            //ReadLargeJsonJavaScriptSerializer();

            Console.WriteLine();
            Console.WriteLine("Finished");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public static void LargeArrayJTokenPathPerformance()
        {
            JArray a = new JArray();
            for (int i = 0; i < 100000; i++)
            {
                a.Add(i);
            }

            JToken last = a.Last;

            int interations = 1000;

            Console.WriteLine("Ready!!!");
            Console.ReadKey();

            string p = null;
            for (int i = 0; i < interations; i++)
            {
                p = last.Path;
            }
        }

        public static void TokenWriteToAsync()
        {
            PerformanceTests t = new PerformanceTests();
            t.Iterations = 50000;
            t.TokenWriteToAsync().Wait();
        }

        public static void SerializeJsonAsync()
        {
            PerformanceTests t = new PerformanceTests();
            t.Iterations = 50000;
            t.SerializeAsync().Wait();
        }

        public static void DeserializeJsonAsync()
        {
            PerformanceTests t = new PerformanceTests();
            t.Iterations = 50000;
            t.DeserializeAsync().Wait();
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

            for (int i = 0; i < 10; i++)
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

        public static void ReadLargeJson()
        {
            using (var jsonFile = System.IO.File.OpenText("large.json"))
            using (JsonTextReader jsonTextReader = new JsonTextReader(jsonFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Deserialize<IList<PerformanceTests.RootObject>>(jsonTextReader);
            }

            Console.WriteLine("Press any key to start deserialization");
            Console.ReadKey();
            Console.WriteLine("Deserializing...");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 5; i++)
            {
                using (var jsonFile = System.IO.File.OpenText("large.json"))
                using (JsonTextReader jsonTextReader = new JsonTextReader(jsonFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Deserialize<IList<PerformanceTests.RootObject>>(jsonTextReader);
                }
            }

            sw.Stop();

            Console.WriteLine("Finished. Total seconds: " + sw.Elapsed.TotalSeconds);
        }

        public static void ReadLargeJsonJavaScriptSerializer()
        {
            string json = System.IO.File.ReadAllText("large.json");

            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = int.MaxValue;
            s.Deserialize<IList<PerformanceTests.RootObject>>(json);

            Console.WriteLine("Press any key to start deserialization");
            Console.ReadKey();
            Console.WriteLine("Deserializing...");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 5; i++)
            {
                json = System.IO.File.ReadAllText("large.json");

                s = new JavaScriptSerializer();
                s.MaxJsonLength = int.MaxValue;
                s.Deserialize<IList<PerformanceTests.RootObject>>(json);
            }

            sw.Stop();

            Console.WriteLine("Finished. Total seconds: " + sw.Elapsed.TotalSeconds);
        }
    }
}