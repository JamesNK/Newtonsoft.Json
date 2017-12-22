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

#if !(DNXCORE50 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD2_0

namespace Newtonsoft.Json.Tests.Documentation.Samples.Xml
{
    [TestFixture]
    public class ConvertXmlToJson : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            #region Usage
            string xml = @"<?xml version='1.0' standalone='no'?>
            <root>
              <person id='1'>
              <name>Alan</name>
              <url>http://www.google.com</url>
              </person>
              <person id='2'>
              <name>Louis</name>
              <url>http://www.yahoo.com</url>
              </person>
            </root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(doc);

            Console.WriteLine(json);
            // {
            //   "?xml": {
            //     "@version": "1.0",
            //     "@standalone": "no"
            //   },
            //   "root": {
            //     "person": [
            //       {
            //         "@id": "1",
            //         "name": "Alan",
            //         "url": "http://www.google.com"
            //       },
            //       {
            //         "@id": "2",
            //         "name": "Louis",
            //         "url": "http://www.yahoo.com"
            //       }
            //     ]
            //   }
            // }
            #endregion

            Assert.AreEqual(@"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com""},{""@id"":""2"",""name"":""Louis"",""url"":""http://www.yahoo.com""}]}}", json);
        }
    }
}

#endif