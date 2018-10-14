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

#if !(DNXCORE50 || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
#if !NET20
using System.Xml.Linq;
#endif
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
    public class Issue1734
    {
#if !(PORTABLE || PORTABLE40)
        [Test]
        public void Test_XmlNode()
        {
            XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(JsonWithoutNamespace, "", true);

            StringAssert.AreEqual(@"<Test_Service>
  <fname>mark</fname>
  <lname>joye</lname>
  <CarCompany>saab</CarCompany>
  <CarNumber>9741</CarNumber>
  <IsInsured>true</IsInsured>
  <safty>ABS</safty>
  <safty>AirBags</safty>
  <safty>childdoorlock</safty>
  <CarDescription>test Car</CarDescription>
  <collections json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <XYZ>1</XYZ>
    <PQR>11</PQR>
    <contactdetails>
      <contname>DOM</contname>
      <contnumber>8787</contnumber>
    </contactdetails>
    <contactdetails>
      <contname>COM</contname>
      <contnumber>4564</contnumber>
      <addtionaldetails json:Array=""true"">
        <description>54657667</description>
      </addtionaldetails>
    </contactdetails>
    <contactdetails>
      <contname>gf</contname>
      <contnumber>123</contnumber>
      <addtionaldetails json:Array=""true"">
        <description>123</description>
      </addtionaldetails>
    </contactdetails>
  </collections>
</Test_Service>", IndentXml(xmlDoc.OuterXml));

            xmlDoc = JsonConvert.DeserializeXmlNode(JsonWithNamespace, "", true);

            StringAssert.AreEqual(@"<ns3:Test_Service xmlns:ns3=""http://www.CCKS.org/XRT/Form"">
  <ns3:fname>mark</ns3:fname>
  <ns3:lname>joye</ns3:lname>
  <ns3:CarCompany>saab</ns3:CarCompany>
  <ns3:CarNumber>9741</ns3:CarNumber>
  <ns3:IsInsured>true</ns3:IsInsured>
  <ns3:safty>ABS</ns3:safty>
  <ns3:safty>AirBags</ns3:safty>
  <ns3:safty>childdoorlock</ns3:safty>
  <ns3:CarDescription>test Car</ns3:CarDescription>
  <ns3:collections json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <ns3:XYZ>1</ns3:XYZ>
    <ns3:PQR>11</ns3:PQR>
    <ns3:contactdetails>
      <ns3:contname>DOM</ns3:contname>
      <ns3:contnumber>8787</ns3:contnumber>
    </ns3:contactdetails>
    <ns3:contactdetails>
      <ns3:contname>COM</ns3:contname>
      <ns3:contnumber>4564</ns3:contnumber>
      <ns3:addtionaldetails json:Array=""true"">
        <ns3:description>54657667</ns3:description>
      </ns3:addtionaldetails>
    </ns3:contactdetails>
    <ns3:contactdetails>
      <ns3:contname>gf</ns3:contname>
      <ns3:contnumber>123</ns3:contnumber>
      <ns3:addtionaldetails json:Array=""true"">
        <ns3:description>123</ns3:description>
      </ns3:addtionaldetails>
    </ns3:contactdetails>
  </ns3:collections>
</ns3:Test_Service>", IndentXml(xmlDoc.OuterXml));
        }

        private string IndentXml(string xml)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml));

            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });

            while (reader.Read())
            {
                writer.WriteNode(reader, false);
            }

            writer.Flush();

            return sw.ToString();
        }
#endif

#if !NET20
        [Test]
        public void Test_XNode()
        {
            XDocument xmlDoc = JsonConvert.DeserializeXNode(JsonWithoutNamespace, "", true);

            string xml = xmlDoc.ToString();
            StringAssert.AreEqual(@"<Test_Service>
  <fname>mark</fname>
  <lname>joye</lname>
  <CarCompany>saab</CarCompany>
  <CarNumber>9741</CarNumber>
  <IsInsured>true</IsInsured>
  <safty>ABS</safty>
  <safty>AirBags</safty>
  <safty>childdoorlock</safty>
  <CarDescription>test Car</CarDescription>
  <collections json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <XYZ>1</XYZ>
    <PQR>11</PQR>
    <contactdetails>
      <contname>DOM</contname>
      <contnumber>8787</contnumber>
    </contactdetails>
    <contactdetails>
      <contname>COM</contname>
      <contnumber>4564</contnumber>
      <addtionaldetails json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
        <description>54657667</description>
      </addtionaldetails>
    </contactdetails>
    <contactdetails>
      <contname>gf</contname>
      <contnumber>123</contnumber>
      <addtionaldetails json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
        <description>123</description>
      </addtionaldetails>
    </contactdetails>
  </collections>
</Test_Service>", xml);

            xmlDoc = JsonConvert.DeserializeXNode(JsonWithNamespace, "", true);

            xml = xmlDoc.ToString();
            StringAssert.AreEqual(@"<ns3:Test_Service xmlns:ns3=""http://www.CCKS.org/XRT/Form"">
  <ns3:fname>mark</ns3:fname>
  <ns3:lname>joye</ns3:lname>
  <ns3:CarCompany>saab</ns3:CarCompany>
  <ns3:CarNumber>9741</ns3:CarNumber>
  <ns3:IsInsured>true</ns3:IsInsured>
  <ns3:safty>ABS</ns3:safty>
  <ns3:safty>AirBags</ns3:safty>
  <ns3:safty>childdoorlock</ns3:safty>
  <ns3:CarDescription>test Car</ns3:CarDescription>
  <ns3:collections json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <ns3:XYZ>1</ns3:XYZ>
    <ns3:PQR>11</ns3:PQR>
    <ns3:contactdetails>
      <ns3:contname>DOM</ns3:contname>
      <ns3:contnumber>8787</ns3:contnumber>
    </ns3:contactdetails>
    <ns3:contactdetails>
      <ns3:contname>COM</ns3:contname>
      <ns3:contnumber>4564</ns3:contnumber>
      <ns3:addtionaldetails json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
        <ns3:description>54657667</ns3:description>
      </ns3:addtionaldetails>
    </ns3:contactdetails>
    <ns3:contactdetails>
      <ns3:contname>gf</ns3:contname>
      <ns3:contnumber>123</ns3:contnumber>
      <ns3:addtionaldetails json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
        <ns3:description>123</ns3:description>
      </ns3:addtionaldetails>
    </ns3:contactdetails>
  </ns3:collections>
</ns3:Test_Service>", xml);
        }
#endif

        private const string JsonWithoutNamespace = @"{
  ""Test_Service"": {
    ""fname"": ""mark"",
    ""lname"": ""joye"",
    ""CarCompany"": ""saab"",
    ""CarNumber"": ""9741"",
    ""IsInsured"": ""true"",
    ""safty"": [
      ""ABS"",
      ""AirBags"",
      ""childdoorlock""
    ],
    ""CarDescription"": ""test Car"",
    ""collections"": [
      {
        ""XYZ"": ""1"",
        ""PQR"": ""11"",
        ""contactdetails"": [
          {
            ""contname"": ""DOM"",
            ""contnumber"": ""8787""
          },
          {
            ""contname"": ""COM"",
            ""contnumber"": ""4564"",
            ""addtionaldetails"": [
              {
                ""description"": ""54657667""
              }
            ]
          },
          {
            ""contname"": ""gf"",
            ""contnumber"": ""123"",
            ""addtionaldetails"": [
              {
                ""description"": ""123""
              }
            ]
          }
        ]
      }
    ]
  }
}";

        private const string JsonWithNamespace = @"{
  ""ns3:Test_Service"": {
    ""@xmlns:ns3"": ""http://www.CCKS.org/XRT/Form"",
    ""ns3:fname"": ""mark"",
    ""ns3:lname"": ""joye"",
    ""ns3:CarCompany"": ""saab"",
    ""ns3:CarNumber"": ""9741"",
    ""ns3:IsInsured"": ""true"",
    ""ns3:safty"": [
      ""ABS"",
      ""AirBags"",
      ""childdoorlock""
    ],
    ""ns3:CarDescription"": ""test Car"",
    ""ns3:collections"": [
      {
        ""ns3:XYZ"": ""1"",
        ""ns3:PQR"": ""11"",
        ""ns3:contactdetails"": [
          {
            ""ns3:contname"": ""DOM"",
            ""ns3:contnumber"": ""8787""
          },
          {
            ""ns3:contname"": ""COM"",
            ""ns3:contnumber"": ""4564"",
            ""ns3:addtionaldetails"": [
              {
                ""ns3:description"": ""54657667""
              }
            ]
          },
          {
            ""ns3:contname"": ""gf"",
            ""ns3:contnumber"": ""123"",
            ""ns3:addtionaldetails"": [
              {
                ""ns3:description"": ""123""
              }
            ]
          }
        ]
      }
    ]
  }
}";
    }
}
#endif