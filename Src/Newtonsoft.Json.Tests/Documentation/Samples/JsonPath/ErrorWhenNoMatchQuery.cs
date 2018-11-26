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
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System;
using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.JsonPath
{
    [TestFixture]
    public class ErrorWhenNoMatchQuery : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            JArray items = JArray.Parse(@"[
              {
                'Name': 'John Doe',
              },
              {
                'Name': 'Jane Doe',
              }
            ]");

            // A true value for errorWhenNoMatch will result in an error if the queried value is missing 
            string result;
            try
            {
                result = (string)items.SelectToken(@"$.[3]['Name']", errorWhenNoMatch: true);
            }
            catch (JsonException)
            {
                result = "Unable to find result in JSON.";
            }
            #endregion

            Assert.AreEqual("Unable to find result in JSON.", result);
        }
    }
}