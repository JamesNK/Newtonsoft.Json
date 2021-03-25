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

//using System;
//using System.Diagnostics;
//using BenchmarkDotNet.Running;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Tests.Benchmarks;
//using System.Reflection;

//namespace Newtonsoft.Json.TestConsole
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            var attribute = (AssemblyFileVersionAttribute)typeof(JsonConvert).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute));
//            Console.WriteLine("Json.NET Version: " + attribute.Version);

//            new BenchmarkSwitcher(new [] { typeof(LowLevelBenchmarks) }).Run(new[] { "*" });
//        }
//    }
//}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class AClass
{
    private static List<string> DefaultListValue = new List<string>() { "123456789" };

    public List<string> List { get; set; } =DefaultListValue; 

}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        List<AClass> beforeList = new List<AClass>();

        var beforeA = new AClass();
        beforeList.Add(beforeA);

        var str = JsonConvert.SerializeObject(beforeList);

        List<AClass> afterList = JsonConvert.DeserializeObject<List<AClass>>(str);

        var afterA = afterList.FirstOrDefault();
        Console.ReadKey();
    }
}