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
using System.Text;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
    [TestFixture]
    public class JValueCast : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            JValue v1 = new JValue("1");
            int i = (int)v1;

            Console.WriteLine(i);
            // 1

            JValue v2 = new JValue(true);
            bool b = (bool)v2;

            Console.WriteLine(b);
            // true

            JValue v3 = new JValue("19.95");
            decimal d = (decimal)v3;

            Console.WriteLine(d);
            // 19.95

            JValue v4 = new JValue(new DateTime(2013, 1, 21));
            string s = (string)v4;

            Console.WriteLine(s);
            // 01/21/2013 00:00:00

            JValue v5 = new JValue("http://www.bing.com");
            Uri u = (Uri)v5;

            Console.WriteLine(u);
            // http://www.bing.com/

            JValue v6 = JValue.CreateNull();
            u = (Uri)v6;

            Console.WriteLine((u != null) ? u.ToString() : "{null}");
            // {null}

            DateTime? dt = (DateTime?)v6;

            Console.WriteLine((dt != null) ? dt.ToString() : "{null}");
            // {null}
            #endregion

            Assert.AreEqual("01/21/2013 00:00:00", s);
        }
    }
}