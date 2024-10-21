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
using System.Globalization;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Benchmarks
{
    public class DeserializeBenchmarks
    {
        private static readonly string LargeJsonText;
        private static readonly string FloatArrayJson;
        private static readonly string DateArrayJson;
        private static readonly StringReference[] DateTimeStringReferences;
        private static readonly DateTimeOffset[] ParsedDateTimesArray;
        private static readonly JsonSerializer Serializer = new();

        static DeserializeBenchmarks()
        {
            LargeJsonText = System.IO.File.ReadAllText(TestFixtureBase.ResolvePath("large.json"));

            const int count = 5000;
            FloatArrayJson = new JArray(Enumerable.Range(0, count).Select(i => i * 1.1m)).ToString(Formatting.None);
            var dates = new DateTime[count];
            DateTimeStringReferences = new StringReference[count];
            ParsedDateTimesArray = new DateTimeOffset[count];
            DateTime time = new(1969, 7, 20, 2, 56, 15, DateTimeKind.Utc);
            for (int i = 0; i < count; i++)
            {
                DateTime dateTime = time.AddDays(i);
                dates[i] = dateTime;
                string dateTimeString = dateTime.ToString("O", CultureInfo.InvariantCulture);
                DateTimeStringReferences[i] = new StringReference(dateTimeString.ToCharArray(), 0, dateTimeString.Length);
            }
            DateArrayJson = new JArray(dates).ToString(Formatting.None);
        }

        [Benchmark]
        public IList<RootObject> DeserializeLargeJsonText()
        {
            return JsonConvert.DeserializeObject<IList<RootObject>>(LargeJsonText);
        }

        [Benchmark]
        public IList<RootObject> DeserializeLargeJsonFile()
        {
            using StringReader jsonFile = new(LargeJsonText);
            using (JsonTextReader jsonTextReader = new JsonTextReader(jsonFile))
            {
                return Serializer.Deserialize<IList<RootObject>>(jsonTextReader);
            }
        }

        [Benchmark]
        public IList<double> DeserializeDoubleList()
        {
            return JsonConvert.DeserializeObject<IList<double>>(FloatArrayJson);
        }

        [Benchmark]
        public IList<decimal> DeserializeDecimalList()
        {
            return JsonConvert.DeserializeObject<IList<decimal>>(FloatArrayJson);
        }

        [Benchmark]
        public IList<DateTime> DeserializeDateTimeList()
        {
            return JsonConvert.DeserializeObject<IList<DateTime>>(DateArrayJson);
        }

        [Benchmark]
        public bool TryParseDateTimeOffsetIso()
        {
            bool success = true;
            for (var index = 0; index < DateTimeStringReferences.Length; index++)
            {
                success &= DateTimeUtils.TryParseDateTimeOffsetIso(DateTimeStringReferences[index], out DateTimeOffset result);
                ParsedDateTimesArray[index] = result;
            }
            return success;
        }
    }
}

#endif