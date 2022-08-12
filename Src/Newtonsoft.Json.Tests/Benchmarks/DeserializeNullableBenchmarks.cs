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

#if HAVE_BENCHMARKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Converters;
using BenchmarkDotNet.Attributes.Jobs;

namespace Newtonsoft.Json.Tests.Benchmarks
{
        
    public class DeserializeNullableBenchmarks
    {
        private static readonly string JsonText;
        private static readonly JsonSerializerSettings SerializerSettings;


        static DeserializeNullableBenchmarks()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            for (var i = 0; i <= 1000; i++)
            {
                sb.Append("{\"a\":1,\"b\":\"c\",\"c\":true},");
            }
            sb.Append(']');

            JsonText = sb.ToString();


            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.Converters.Add(new StringEnumConverter());
        }
        

        [Benchmark]
        public List<NullablePropertiesTestClass> DeserializeTypeWithNullables()
        {
            return JsonConvert.DeserializeObject<List<NullablePropertiesTestClass>>(JsonText, SerializerSettings);
        }

        public class NullablePropertiesTestClass
        {
            public int? a { get; set; }
            
            public TestEnum? b { get; set; }

            public bool? c { get; set; }

            public enum TestEnum
            {
                a,
                b,
                c
            }
        }
    }
}

#endif