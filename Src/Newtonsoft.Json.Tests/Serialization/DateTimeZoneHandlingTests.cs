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
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class DateTimeZoneHandlingTests : TestFixtureBase
    {
        [Test]
        public void DeserializeObject()
        {
            string json = @"
  {
    ""Value"": ""2017-12-05T21:59:00""
  }";

            DateTimeWrapper c1 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

            DateTimeWrapper c2 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });

            DateTimeWrapper c3 = JsonConvert.DeserializeObject<DateTimeWrapper>(json, new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });

            DateTimeWrapper c4 = JsonConvert.DeserializeObject<DateTimeWrapper>(json);

            Assert.AreEqual(DateTimeKind.Utc, c1.Value.Kind);
            Assert.AreEqual(DateTimeKind.Local, c2.Value.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, c3.Value.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, c4.Value.Kind);
        }

        [Test]
        public void DeserializeFromJObject()
        {
            string json = @"
  {
    ""Value"": ""2017-12-05T21:59:00""
  }";

            JObject jo = JObject.Parse(json);

            DateTimeWrapper c1 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            }));

            DateTimeWrapper c2 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            }));

            DateTimeWrapper c3 = jo.ToObject<DateTimeWrapper>(JsonSerializer.Create(new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            }));

            DateTimeWrapper c4 = jo.ToObject<DateTimeWrapper>();

            Assert.AreEqual(DateTimeKind.Utc, c1.Value.Kind);
            Assert.AreEqual(DateTimeKind.Local, c2.Value.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, c3.Value.Kind);
            Assert.AreEqual(DateTimeKind.Unspecified, c4.Value.Kind);
        }
    }
}