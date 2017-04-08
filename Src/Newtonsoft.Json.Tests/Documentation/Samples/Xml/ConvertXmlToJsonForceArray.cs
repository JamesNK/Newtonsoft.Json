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
using System.Xml;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

#if !(NET20 || DNXCORE50 || PORTABLE || PORTABLE40)

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
    [TestFixture]
    public class ConvertXmlToJsonForceArray : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            string xml = @"<person id='1'>
              <name>Alan</name>
              <url>http://www.google.com</url>
              <role>Admin1</role>
            </person>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(doc);

            Console.WriteLine(json);
            // {
            //   "person": {
            //     "@id": "1",
            //     "name": "Alan",
            //     "url": "http://www.google.com",
            //     "role": "Admin1"
            //   }
            // }

            xml = @"<person xmlns:json='http://james.newtonking.com/projects/json' id='1'>
              <name>Alan</name>
              <url>http://www.google.com</url>
              <role json:Array='true'>Admin</role>
            </person>";

            doc = new XmlDocument();
            doc.LoadXml(xml);

            json = JsonConvert.SerializeXmlNode(doc);

            Console.WriteLine(json);
            // {
            //   "person": {
            //     "@id": "1",
            //     "name": "Alan",
            //     "url": "http://www.google.com",
            //     "role": [
            //       "Admin"
            //     ]
            //   }
            // }
            #endregion

            Assert.AreEqual(@"{""person"":{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com"",""role"":[""Admin""]}}", json);
        }
    }
}

#endif