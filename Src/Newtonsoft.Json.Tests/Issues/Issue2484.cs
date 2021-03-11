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

#if (NET45 || NET50)
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Collections;
using System;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2484
    {
        [Test]
        public void Test()
        {
            var json = "[]";
            var ex = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject(json, typeof(JObject)));
            Assert.AreEqual("Deserialized JSON type 'Newtonsoft.Json.Linq.JArray' is not compatible with expected type 'Newtonsoft.Json.Linq.JObject'. Path '', line 1, position 2.", ex.Message);
        }
    }
}
#endif