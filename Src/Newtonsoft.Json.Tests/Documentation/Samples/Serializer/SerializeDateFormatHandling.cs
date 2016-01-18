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
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class SerializeDateFormatHandling : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            DateTime mayanEndOfTheWorld = new DateTime(2012, 12, 21);

            string jsonIsoDate = JsonConvert.SerializeObject(mayanEndOfTheWorld);

            Console.WriteLine(jsonIsoDate);
            // "2012-12-21T00:00:00"

            string jsonMsDate = JsonConvert.SerializeObject(mayanEndOfTheWorld, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            });

            Console.WriteLine(jsonMsDate);
            // "\/Date(1356044400000+0100)\/"
            #endregion

            Assert.IsNotNull(jsonMsDate);
        }
    }
}