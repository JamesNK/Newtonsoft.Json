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
    public class LargeJArrayBenchmarks
    {
        private JArray _largeJArraySample;

        [GlobalSetup]
        public void SetupData()
        {
            _largeJArraySample = new JArray();
            for (int i = 0; i < 100000; i++)
            {
                _largeJArraySample.Add(i);
            }
        }

        [Benchmark]
        public string JTokenPathFirstItem()
        {
            JToken first = _largeJArraySample.First;

            return first.Path;
        }

        [Benchmark]
        public string JTokenPathLastItem()
        {
            JToken last = _largeJArraySample.Last;

            return last.Path;
        }

        [Benchmark]
        public void AddPerformance()
        {
            _largeJArraySample.Add(1);
            _largeJArraySample.RemoveAt(_largeJArraySample.Count - 1);
        }
    }
}

#endif