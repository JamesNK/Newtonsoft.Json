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

#pragma warning disable 618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json.Schema;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Schema
{
    [TestFixture]
    public class PerformanceTests : TestFixtureBase
    {
        [Test]
        public void ReaderPerformance()
        {
            string json = @"[
    {
        ""id"": 2,
        ""name"": ""An ice sculpture"",
        ""price"": 12.50,
        ""tags"": [""cold"", ""ice""],
        ""dimensions"": {
            ""length"": 7.0,
            ""width"": 12.0,
            ""height"": 9.5
        },
        ""warehouseLocation"": {
            ""latitude"": -78.75,
            ""longitude"": 20.4
        }
    },
    {
        ""id"": 3,
        ""name"": ""A blue mouse"",
        ""price"": 25.50,
        ""dimensions"": {
            ""length"": 3.1,
            ""width"": 1.0,
            ""height"": 1.0
        },
        ""warehouseLocation"": {
            ""latitude"": 54.4,
            ""longitude"": -32.7
        }
    }
]";

            JsonSchema schema = JsonSchema.Parse(@"{
    ""$schema"": ""http://json-schema.org/draft-04/schema#"",
    ""title"": ""Product set"",
    ""type"": ""array"",
    ""items"": {
        ""title"": ""Product"",
        ""type"": ""object"",
        ""properties"": {
            ""id"": {
                ""description"": ""The unique identifier for a product"",
                ""type"": ""number"",
                ""required"": true
            },
            ""name"": {
                ""type"": ""string"",
                ""required"": true
            },
            ""price"": {
                ""type"": ""number"",
                ""minimum"": 0,
                ""exclusiveMinimum"": true,
                ""required"": true
            },
            ""tags"": {
                ""type"": ""array"",
                ""items"": {
                    ""type"": ""string""
                },
                ""minItems"": 1,
                ""uniqueItems"": true
            },
            ""dimensions"": {
                ""type"": ""object"",
                ""properties"": {
                    ""length"": {""type"": ""number"",""required"": true},
                    ""width"": {""type"": ""number"",""required"": true},
                    ""height"": {""type"": ""number"",""required"": true}
                }
            },
            ""warehouseLocation"": {
                ""description"": ""A geographical coordinate"",
                ""type"": ""object"",
                ""properties"": {
                    ""latitude"": { ""type"": ""number"" },
                    ""longitude"": { ""type"": ""number"" }
                }
            }
        }
    }
}");

            using (var tester = new PerformanceTester("Reader"))
            {
                for (int i = 0; i < 5000; i++)
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader(json));
                    JsonValidatingReader validatingReader = new JsonValidatingReader(reader);
                    validatingReader.Schema = schema;

                    while (validatingReader.Read())
                    {
                    }
                }
            }
        }
    }

    public class PerformanceTester : IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Action<TimeSpan> _callback;

        public PerformanceTester(string description)
            : this(ts => Console.WriteLine(description + ": " + ts.TotalSeconds))
        {
        }

        public PerformanceTester(Action<TimeSpan> callback)
        {
            _callback = callback;
            _stopwatch.Start();
        }

        public static PerformanceTester Start(Action<TimeSpan> callback)
        {
            return new PerformanceTester(callback);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            if (_callback != null)
            {
                _callback(Result);
            }
        }

        public TimeSpan Result
        {
            get { return _stopwatch.Elapsed; }
        }
    }
}

#pragma warning restore 618