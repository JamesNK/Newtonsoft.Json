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

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1877
    {
        [Test]
        public void Test()
        {
            var f2 = new Fubar2();
            f2.Version = new Version("3.0");
            (f2 as Fubar).Version = new Version("4.0");

            var s = JsonConvert.SerializeObject(f2, new JsonSerializerSettings
            {
                Converters = { new VersionConverter() }
            });
            Assert.AreEqual(@"{""Version"":""4.0""}", s);

            var f3 = JsonConvert.DeserializeObject<Fubar2>(s, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Converters = { new VersionConverter() }
            });

            Assert.AreEqual(2, f3.Version.Major);
            Assert.AreEqual(4, (f3 as Fubar).Version.Major);
        }

        class Fubar
        {
            public Version Version { get; set; } = new Version("1.0");

            // ...
        }

        private class Fubar2 : Fubar
        {
            [JsonIgnore]
            public new Version Version { get; set; } = new Version("2.0");

            // ...
        }
    }
}
