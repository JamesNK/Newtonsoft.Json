﻿#region License
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
using System.Linq;
using System.Collections.Generic;
using System;
using System.Globalization;
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
    public class Issue2185
    {
        [Test]
        public void Test()
        {
            AssertDeserializeDateTimeInRFC3339("2019-08-13T11:28:30.8630322Z");
            AssertDeserializeDateTimeInRFC3339("2019-08-13T11:28:30.4506139547Z");
            AssertDeserializeDateTimeInRFC3339("2019-08-13T11:28:30.4506139432Z");
            AssertDeserializeDateTimeInRFC3339("2019-08-13T11:28:30.4506139346782349461Z");
        }

        private void AssertDeserializeDateTimeInRFC3339(string dateTime)
        {
            var list = JsonConvert.DeserializeObject<List<object>>($"[ '{dateTime}' ]");

            Assert.AreEqual(typeof(DateTime), list[0].GetType());

            var parsedDateTime = (DateTime)list[0];
            var frameworkDateTime = DateTime.Parse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            Assert.AreEqual(frameworkDateTime, parsedDateTime);
        }
    }
}
