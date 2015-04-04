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

#if !(NETFX_CORE || PORTABLE || PORTABLE40 || DNXCORE50)
using NUnit.Framework;
using System.Web.UI;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq.ComponentModel
{
    [TestFixture]
    public class BindingTests : TestFixtureBase
    {
        [Test]
        public void DataBinderEval()
        {
            JObject o = new JObject(
                new JProperty("First", "String!"),
                new JProperty("Second", 12345.6789m),
                new JProperty("Third", new JArray(
                    1,
                    2,
                    3,
                    4,
                    5,
                    new JObject(
                        new JProperty("Fourth", "String!"),
                        new JProperty("Fifth", new JObject(
                            new JProperty("Sixth", "String!")))))));

            object value;

            value = (string)DataBinder.Eval(o, "First.Value");
            Assert.AreEqual(value, (string)o["First"]);

            value = DataBinder.Eval(o, "Second.Value");
            Assert.AreEqual(value, (decimal)o["Second"]);

            value = DataBinder.Eval(o, "Third");
            Assert.AreEqual(value, o["Third"]);

            value = DataBinder.Eval(o, "Third[0].Value");
            Assert.AreEqual((int)value, (int)o["Third"][0]);

            value = DataBinder.Eval(o, "Third[5].Fourth.Value");
            Assert.AreEqual(value, (string)o["Third"][5]["Fourth"]);

            value = DataBinder.Eval(o, "Third[5].Fifth.Sixth.Value");
            Assert.AreEqual(value, (string)o["Third"][5]["Fifth"]["Sixth"]);
        }
    }
}

#endif