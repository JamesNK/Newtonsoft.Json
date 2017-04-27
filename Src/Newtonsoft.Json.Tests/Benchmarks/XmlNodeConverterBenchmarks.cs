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
using System.Xml;
using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Benchmarks
{
    public class XmlNodeConverterBenchmarks
    {
        [Benchmark]
        public void ConvertXmlNode()
        {
            XmlDocument doc = new XmlDocument();
            using (FileStream file = System.IO.File.OpenRead(TestFixtureBase.ResolvePath("large_sample.xml")))
            {
                doc.Load(file);
            }

            JsonConvert.SerializeXmlNode(doc);
        }

        [Benchmark]
        public void ConvertXNode()
        {
            XDocument doc;
            using (FileStream file = System.IO.File.OpenRead(TestFixtureBase.ResolvePath("large_sample.xml")))
            {
                doc = XDocument.Load(file);
            }

            JsonConvert.SerializeXNode(doc);
        }
    }
}

#endif