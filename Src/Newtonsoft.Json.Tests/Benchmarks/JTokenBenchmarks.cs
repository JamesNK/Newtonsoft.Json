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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Benchmarks
{
    public class JTokenBenchmarks
    {
        private static readonly JObject JObjectSample = JObject.Parse(BenchmarkConstants.JsonText);
        private static readonly string JsonTextSample;
        private static readonly string NestedJsonText;

        static JTokenBenchmarks()
        {
            JObject o = new JObject();
            for (int i = 0; i < 50; i++)
            {
                o[i.ToString()] = i;
            }
            JsonTextSample = o.ToString();

            NestedJsonText = (new string('[', 100000)) + "1" + (new string(']', 100000));
        }

        [Benchmark]
        public void TokenWriteTo()
        {
            StringWriter sw = new StringWriter();
            JObjectSample.WriteTo(new JsonTextWriter(sw));
        }

        [Benchmark]
        public Task TokenWriteToAsync()
        {
            StringWriter sw = new StringWriter();
            return JObjectSample.WriteToAsync(new JsonTextWriter(sw));
        }

        [Benchmark]
        public JObject JObjectParse()
        {
            return JObject.Parse(JsonTextSample);
        }

        [Benchmark]
        public JArray JArrayNestedParse()
        {
            return JArray.Parse(NestedJsonText);
        }

        [Benchmark]
        public JArray JArrayNestedBuild()
        {
            JArray current = new JArray();
            JArray root = current;
            for (int j = 0; j < 100000; j++)
            {
                JArray temp = new JArray();
                current.Add(temp);
                current = temp;
            }
            current.Add(1);

            return root;
        }
    }
}

#endif