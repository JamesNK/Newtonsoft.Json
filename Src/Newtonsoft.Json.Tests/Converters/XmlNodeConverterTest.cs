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
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
#if !NET20
using System.Xml.Linq;

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class XmlNodeConverterTest : TestFixtureBase
    {
#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        private string SerializeXmlNode(XmlNode node)
        {
            string json = JsonConvert.SerializeXmlNode(node, Formatting.Indented);

#if !(NET20)
#if !NETSTANDARD1_3
            XmlReader reader = new XmlNodeReader(node);
#else
            StringReader sr = new StringReader(node.OuterXml);
            XmlReader reader = XmlReader.Create(sr);
#endif

            XObject xNode;
            if (node is XmlDocument)
            {
                xNode = XDocument.Load(reader);
            }
            else if (node is XmlAttribute)
            {
                XmlAttribute attribute = (XmlAttribute)node;
                xNode = new XAttribute(XName.Get(attribute.LocalName, attribute.NamespaceURI), attribute.Value);
            }
            else
            {
                reader.MoveToContent();
                xNode = XNode.ReadFrom(reader);
            }

            string linqJson = JsonConvert.SerializeXNode(xNode, Formatting.Indented);

            Assert.AreEqual(json, linqJson);
#endif

            return json;
        }

        private XmlNode DeserializeXmlNode(string json)
        {
            return DeserializeXmlNode(json, null);
        }

        private XmlNode DeserializeXmlNode(string json, string deserializeRootElementName)
        {
            JsonTextReader reader;

            reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            XmlNodeConverter converter = new XmlNodeConverter();
            if (deserializeRootElementName != null)
            {
                converter.DeserializeRootElementName = deserializeRootElementName;
            }

            XmlNode node = (XmlNode)converter.ReadJson(reader, typeof(XmlDocument), null, new JsonSerializer());

#if !NET20
            string xmlText = node.OuterXml;

            reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            XDocument d = (XDocument)converter.ReadJson(reader, typeof(XDocument), null, new JsonSerializer());

            string linqXmlText = d.ToString(SaveOptions.DisableFormatting);
            if (d.Declaration != null)
            {
                linqXmlText = d.Declaration + linqXmlText;
            }

            Assert.AreEqual(xmlText, linqXmlText);
#endif

            return node;
        }
#endif

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

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void DeserializeXmlNode_DefaultDate()
        {
            XmlDocument xmlNode = JsonConvert.DeserializeXmlNode("{Time: \"0001-01-01T00:00:00\"}");

            Assert.AreEqual("<Time>0001-01-01T00:00:00</Time>", xmlNode.OuterXml);
        }

        [Test]
        public void XmlNode_Roundtrip_PropertyNameWithColon()
        {
            const string initialJson = @"{""Be:fore:After!"":""Value!""}";

            XmlDocument xmlNode = JsonConvert.DeserializeXmlNode(initialJson, null, false, true);

            Assert.AreEqual("<Be_x003A_fore_x003A_After_x0021_>Value!</Be_x003A_fore_x003A_After_x0021_>", xmlNode.OuterXml);

            string json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XmlNode_Roundtrip_PropertyNameWithEscapedValue()
        {
            const string initialJson = @"{""BeforeAfter!"":""Value!""}";

            XmlDocument xmlNode = JsonConvert.DeserializeXmlNode(initialJson);

            Assert.AreEqual("<BeforeAfter_x0021_>Value!</BeforeAfter_x0021_>", xmlNode.OuterXml);

            string json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XmlNode_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            XmlDocument xmlNode = JsonConvert.DeserializeXmlNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x003F_xml>
    <_x0040_version>1.0</_x0040_version>
    <_x0040_standalone>no</_x0040_standalone>
  </_x003F_xml>
  <_x003F_xml-stylesheet>href=""classic.xsl"" type=""text/xml""</_x003F_xml-stylesheet>
  <span>
    <_x0040_class>vevent</_x0040_class>
    <a>
      <_x0040_class>url</_x0040_class>
      <_x0040_href>http://www.web2con.com/</_x0040_href>
      <span>
        <_x0040_class>summary</_x0040_class>
        <_x0023_text>Web 2.0 Conference</_x0023_text>
        <_x0023_cdata-section>my escaped text</_x0023_cdata-section>
      </span>
      <span>
        <_x0040_class>location</_x0040_class>
        <_x0023_text>Argent Hotel, San Francisco, CA</_x0023_text>
      </span>
      <abbr>
        <_x0040_class>dtstart</_x0040_class>
        <_x0040_title>2005-10-05</_x0040_title>
        <_x0023_text>October 5</_x0023_text>
      </abbr>
      <abbr>
        <_x0040_class>dtend</_x0040_class>
        <_x0040_title>2005-10-08</_x0040_title>
        <_x0023_text>7</_x0023_text>
      </abbr>
    </a>
  </span>
</root>", IndentXml(xmlNode.OuterXml));

            string json = JsonConvert.SerializeXmlNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XmlNode_UnescapeTextContent()
        {
            XmlDocument xmlNode = new XmlDocument();
            xmlNode.LoadXml("<root>A &gt; B</root>");

            string json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(@"{""root"":""A > B""}", json);
        }
#endif

#if !NET20
        [Test]
        public void DeserializeXNode_DefaultDate()
        {
            var xmlNode = JsonConvert.DeserializeXNode("{Time: \"0001-01-01T00:00:00\"}");

            Assert.AreEqual("<Time>0001-01-01T00:00:00</Time>", xmlNode.ToString());
        }
#endif

        [Test]
        public void WriteJsonNull()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            XmlNodeConverter converter = new XmlNodeConverter();
            converter.WriteJson(jsonWriter, null, null);

            StringAssert.AreEqual(@"null", sw.ToString());
        }

#if !NET20
        [Test]
        public void XNode_UnescapeTextContent()
        {
            XElement xmlNode = XElement.Parse("<root>A &gt; B</root>");

            string json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(@"{""root"":""A > B""}", json);
        }

        [Test]
        public void XNode_Roundtrip_PropertyNameWithColon()
        {
            const string initialJson = @"{""Be:fore:After!"":""Value!""}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson, null, false, true);

            Assert.AreEqual("<Be_x003A_fore_x003A_After_x0021_>Value!</Be_x003A_fore_x003A_After_x0021_>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XNode_Roundtrip_PropertyNameWithEscapedValue()
        {
            const string initialJson = @"{""BeforeAfter!"":""Value!""}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson);

            Assert.AreEqual("<BeforeAfter_x0021_>Value!</BeforeAfter_x0021_>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XNode_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x003F_xml>
    <_x0040_version>1.0</_x0040_version>
    <_x0040_standalone>no</_x0040_standalone>
  </_x003F_xml>
  <_x003F_xml-stylesheet>href=""classic.xsl"" type=""text/xml""</_x003F_xml-stylesheet>
  <span>
    <_x0040_class>vevent</_x0040_class>
    <a>
      <_x0040_class>url</_x0040_class>
      <_x0040_href>http://www.web2con.com/</_x0040_href>
      <span>
        <_x0040_class>summary</_x0040_class>
        <_x0023_text>Web 2.0 Conference</_x0023_text>
        <_x0023_cdata-section>my escaped text</_x0023_cdata-section>
      </span>
      <span>
        <_x0040_class>location</_x0040_class>
        <_x0023_text>Argent Hotel, San Francisco, CA</_x0023_text>
      </span>
      <abbr>
        <_x0040_class>dtstart</_x0040_class>
        <_x0040_title>2005-10-05</_x0040_title>
        <_x0023_text>October 5</_x0023_text>
      </abbr>
      <abbr>
        <_x0040_class>dtend</_x0040_class>
        <_x0040_title>2005-10-08</_x0040_title>
        <_x0023_text>7</_x0023_text>
      </abbr>
    </a>
  </span>
</root>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void XNode_MetadataArray_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x0024_id>1</_x0024_id>
  <_x0024_values>1</_x0024_values>
  <_x0024_values>2</_x0024_values>
  <_x0024_values>3</_x0024_values>
  <_x0024_values>4</_x0024_values>
  <_x0024_values>5</_x0024_values>
</root>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }

        [Test]
        public void SerializeDollarProperty()
        {
            string json1 = @"{""$"":""test""}";

            var doc = JsonConvert.DeserializeXNode(json1);

            Assert.AreEqual(@"<_x0024_>test</_x0024_>", doc.ToString());

            var json2 = JsonConvert.SerializeXNode(doc);
            
            Assert.AreEqual(json1, json2);
        }

        [Test]
        public void SerializeNonKnownDollarProperty()
        {
            string json1 = @"{""$JELLY"":""test""}";

            var doc = JsonConvert.DeserializeXNode(json1);

            Console.WriteLine(doc.ToString());

            Assert.AreEqual(@"<_x0024_JELLY>test</_x0024_JELLY>", doc.ToString());

            var json2 = JsonConvert.SerializeXNode(doc);

            Assert.AreEqual(json1, json2);
        }

        public class MyModel
        {
            public string MyProperty { get; set; }
        }

        [Test]
        public void ConvertNullString()
        {
            JObject json = new JObject();
            json["Prop1"] = (string)null;
            json["Prop2"] = new MyModel().MyProperty;

            var xmlNodeConverter = new XmlNodeConverter { DeserializeRootElementName = "object" };
            var jsonSerializerSettings = new JsonSerializerSettings { Converters = new JsonConverter[] { xmlNodeConverter } };
            var jsonSerializer = JsonSerializer.CreateDefault(jsonSerializerSettings);
            XDocument d = json.ToObject<XDocument>(jsonSerializer);

            StringAssert.Equals(@"<object>
  <Prop1 />
  <Prop2 />
</object>", d.ToString());
        }

        public class Foo
        {
            public XElement Bar { get; set; }
        }

        [Test]
        public void SerializeAndDeserializeXElement()
        {
            Foo foo = new Foo { Bar = null };
            string json = JsonConvert.SerializeObject(foo);

            Assert.AreEqual(@"{""Bar"":null}", json);
            Foo foo2 = JsonConvert.DeserializeObject<Foo>(json);

            Assert.IsNull(foo2.Bar);
        }

        [Test]
        public void MultipleNamespacesXDocument()
        {
            string xml = @"<result xp_0:end=""2014-08-15 13:12:11.9184"" xp_0:start=""2014-08-15 13:11:49.3140"" xp_0:time_diff=""22604.3836"" xmlns:xp_0=""Test1"" p2:end=""2014-08-15 13:13:49.5522"" p2:start=""2014-08-15 13:13:49.0268"" p2:time_diff=""525.4646"" xmlns:p2=""Test2"" />";

            XDocument d = XDocument.Parse(xml);

            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""result"": {
    ""@xp_0:end"": ""2014-08-15 13:12:11.9184"",
    ""@xp_0:start"": ""2014-08-15 13:11:49.3140"",
    ""@xp_0:time_diff"": ""22604.3836"",
    ""@xmlns:xp_0"": ""Test1"",
    ""@p2:end"": ""2014-08-15 13:13:49.5522"",
    ""@p2:start"": ""2014-08-15 13:13:49.0268"",
    ""@p2:time_diff"": ""525.4646"",
    ""@xmlns:p2"": ""Test2""
  }
}", json);

            XDocument doc = JsonConvert.DeserializeObject<XDocument>(json);

            StringAssert.AreEqual(xml, doc.ToString());
        }
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void MultipleNamespacesXmlDocument()
        {
            string xml = @"<result xp_0:end=""2014-08-15 13:12:11.9184"" xp_0:start=""2014-08-15 13:11:49.3140"" xp_0:time_diff=""22604.3836"" xmlns:xp_0=""Test1"" p2:end=""2014-08-15 13:13:49.5522"" p2:start=""2014-08-15 13:13:49.0268"" p2:time_diff=""525.4646"" xmlns:p2=""Test2"" />";

            XmlDocument d = new XmlDocument();
            d.LoadXml(xml);

            string json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""result"": {
    ""@xp_0:end"": ""2014-08-15 13:12:11.9184"",
    ""@xp_0:start"": ""2014-08-15 13:11:49.3140"",
    ""@xp_0:time_diff"": ""22604.3836"",
    ""@xmlns:xp_0"": ""Test1"",
    ""@p2:end"": ""2014-08-15 13:13:49.5522"",
    ""@p2:start"": ""2014-08-15 13:13:49.0268"",
    ""@p2:time_diff"": ""525.4646"",
    ""@xmlns:p2"": ""Test2""
  }
}", json);

            XmlDocument doc = JsonConvert.DeserializeObject<XmlDocument>(json);

            StringAssert.AreEqual(xml, doc.OuterXml);
        }

        [Test]
        public void SerializeXmlElement()
        {
            string xml = @"<payload>
    <Country>6</Country>
    <FinancialTransactionApprovalRequestUID>79</FinancialTransactionApprovalRequestUID>
    <TransactionStatus>Approved</TransactionStatus>
    <StatusChangeComment></StatusChangeComment>
    <RequestedBy>Someone</RequestedBy>
</payload>";

            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(xml);

            var result = xmlDocument.FirstChild.ChildNodes.Cast<XmlNode>().ToArray();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented); // <--- fails here with the cast message

            StringAssert.AreEqual(@"[
  {
    ""Country"": ""6""
  },
  {
    ""FinancialTransactionApprovalRequestUID"": ""79""
  },
  {
    ""TransactionStatus"": ""Approved""
  },
  {
    ""StatusChangeComment"": """"
  },
  {
    ""RequestedBy"": ""Someone""
  }
]", json);
        }
#endif

#if !NET20
        [Test]
        public void SerializeXElement()
        {
            string xml = @"<payload>
    <Country>6</Country>
    <FinancialTransactionApprovalRequestUID>79</FinancialTransactionApprovalRequestUID>
    <TransactionStatus>Approved</TransactionStatus>
    <StatusChangeComment></StatusChangeComment>
    <RequestedBy>Someone</RequestedBy>
</payload>";

            var xmlDocument = XDocument.Parse(xml);

            var result = xmlDocument.Root.Nodes().ToArray();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented); // <--- fails here with the cast message

            StringAssert.AreEqual(@"[
  {
    ""Country"": ""6""
  },
  {
    ""FinancialTransactionApprovalRequestUID"": ""79""
  },
  {
    ""TransactionStatus"": ""Approved""
  },
  {
    ""StatusChangeComment"": """"
  },
  {
    ""RequestedBy"": ""Someone""
  }
]", json);
        }

        public class DecimalContainer
        {
            public decimal Number { get; set; }
        }

        [Test]
        public void FloatParseHandlingDecimal()
        {
            decimal d = (decimal)Math.PI + 1000000000m;
            var x = new DecimalContainer { Number = d };

            var json = JsonConvert.SerializeObject(x, Formatting.Indented);

            XDocument doc1 = JsonConvert.DeserializeObject<XDocument>(json, new JsonSerializerSettings
            {
                Converters = { new XmlNodeConverter() },
                FloatParseHandling = FloatParseHandling.Decimal
            });

            var xml = doc1.ToString();
            Assert.AreEqual("<Number>1000000003.14159265358979</Number>", xml);

            string json2 = JsonConvert.SerializeObject(doc1, Formatting.Indented);

            DecimalContainer x2 = JsonConvert.DeserializeObject<DecimalContainer>(json2);

            Assert.AreEqual(x.Number, x2.Number);
        }

        public class DateTimeOffsetContainer
        {
            public DateTimeOffset Date { get; set; }
        }

        [Test]
        public void DateTimeParseHandlingOffset()
        {
            DateTimeOffset d = new DateTimeOffset(2012, 12, 12, 12, 44, 1, TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(34)));
            var x = new DateTimeOffsetContainer { Date = d };

            var json = JsonConvert.SerializeObject(x, Formatting.Indented);

            XDocument doc1 = JsonConvert.DeserializeObject<XDocument>(json, new JsonSerializerSettings
            {
                Converters = { new XmlNodeConverter() },
                DateParseHandling = DateParseHandling.DateTimeOffset
            });

            var xml = doc1.ToString();
            Assert.AreEqual("<Date>2012-12-12T12:44:01+12:34</Date>", xml);

            string json2 = JsonConvert.SerializeObject(doc1, Formatting.Indented);

            DateTimeOffsetContainer x2 = JsonConvert.DeserializeObject<DateTimeOffsetContainer>(json2);

            Assert.AreEqual(x.Date, x2.Date);
        }

        [Test]
        public void GroupElementsOfTheSameName()
        {
            string xml = "<root><p>Text1<span>Span1</span> <span>Span2</span> Text2</p></root>";

            string json = JsonConvert.SerializeXNode(XElement.Parse(xml));

            Assert.AreEqual(@"{""root"":{""p"":{""#text"":[""Text1"","" Text2""],""span"":[""Span1"",""Span2""]}}}", json);

            XDocument doc = JsonConvert.DeserializeXNode(json);

            StringAssert.AreEqual(@"<root>
  <p>Text1 Text2<span>Span1</span><span>Span2</span></p>
</root>", doc.ToString());
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void SerializeEmptyDocument()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root />");

            string json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented, true);
            Assert.AreEqual("null", json);

            doc = new XmlDocument();
            doc.LoadXml("<root></root>");

            json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented, true);
            Assert.AreEqual(@"""""", json);

            XDocument doc1 = XDocument.Parse("<root />");

            json = JsonConvert.SerializeXNode(doc1, Formatting.Indented, true);
            Assert.AreEqual("null", json);

            doc1 = XDocument.Parse("<root></root>");

            json = JsonConvert.SerializeXNode(doc1, Formatting.Indented, true);
            Assert.AreEqual(@"""""", json);
        }
#endif

        [Test]
        public void SerializeAndDeserializeXmlWithNamespaceInChildrenAndNoValueInChildren()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'/>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeXNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XElement>(json1);

            var equals = XElement.DeepEquals(xmlBack, xml);
            Assert.IsTrue(equals);
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void DeserializeUndeclaredNamespacePrefix()
        {
            XmlDocument doc = JsonConvert.DeserializeXmlNode("{ A: { '@xsi:nil': true } }");

            Assert.AreEqual(@"<A nil=""true"" />", doc.OuterXml);

            XDocument xdoc = JsonConvert.DeserializeXNode("{ A: { '@xsi:nil': true } }");

            Assert.AreEqual(doc.OuterXml, xdoc.ToString());
        }
#endif
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void DeserializeMultipleRootElements()
        {
            string json = @"{
    ""Id"": 1,
     ""Email"": ""james@example.com"",
     ""Active"": true,
     ""CreatedDate"": ""2013-01-20T00:00:00Z"",
     ""Roles"": [
       ""User"",
       ""Admin""
     ],
    ""Team"": {
        ""Id"": 2,
        ""Name"": ""Software Developers"",
        ""Description"": ""Creators of fine software products and services.""
    }
}";
            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'Email', line 3, position 13.");
        }

        [Test]
        public void DocumentSerializeIndented()
        {
            string xml = @"<?xml version=""1.0"" standalone=""no""?>
<?xml-stylesheet href=""classic.xsl"" type=""text/xml""?>
<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">
    <span class=""summary"">Web 2.0 Conference<![CDATA[my escaped text]]></span>
    <abbr class=""dtstart"" title=""2005-10-05"">October 5</abbr>
    <abbr class=""dtend"" title=""2005-10-08"">7</abbr>
    <span class=""location"">Argent Hotel, San Francisco, CA</span>
  </a>
</span>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);
            string expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            StringAssert.AreEqual(expected, jsonText);
        }

        [Test]
        public void SerializeNodeTypes()
        {
            XmlDocument doc = new XmlDocument();
            string jsonText;

            string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<xs:schema xs:id=""SomeID"" 
	xmlns="""" 
	xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
	xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xs:element name=""MyDataSet"" msdata:IsDataSet=""true"">
	</xs:element>
</xs:schema>";

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            // XmlAttribute
            XmlAttribute attribute = document.DocumentElement.ChildNodes[0].Attributes["IsDataSet", "urn:schemas-microsoft-com:xml-msdata"];
            attribute.Value = "true";

            jsonText = JsonConvert.SerializeXmlNode(attribute);

            Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);

#if !NET20
            XDocument d = XDocument.Parse(xml);
            XAttribute a = d.Root.Element("{http://www.w3.org/2001/XMLSchema}element").Attribute("{urn:schemas-microsoft-com:xml-msdata}IsDataSet");

            jsonText = JsonConvert.SerializeXNode(a);

            Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);
#endif

            // XmlProcessingInstruction
            XmlProcessingInstruction instruction = doc.CreateProcessingInstruction("xml-stylesheet", @"href=""classic.xsl"" type=""text/xml""");

            jsonText = JsonConvert.SerializeXmlNode(instruction);

            Assert.AreEqual(@"{""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\""""}", jsonText);

            // XmlProcessingInstruction
            XmlCDataSection cDataSection = doc.CreateCDataSection("<Kiwi>true</Kiwi>");

            jsonText = JsonConvert.SerializeXmlNode(cDataSection);

            Assert.AreEqual(@"{""#cdata-section"":""<Kiwi>true</Kiwi>""}", jsonText);

            // XmlElement
            XmlElement element = doc.CreateElement("xs", "Choice", "http://www.w3.org/2001/XMLSchema");
            element.SetAttributeNode(doc.CreateAttribute("msdata", "IsDataSet", "urn:schemas-microsoft-com:xml-msdata"));

            XmlAttribute aa = doc.CreateAttribute(@"xmlns", "xs", "http://www.w3.org/2000/xmlns/");
            aa.Value = "http://www.w3.org/2001/XMLSchema";
            element.SetAttributeNode(aa);

            aa = doc.CreateAttribute(@"xmlns", "msdata", "http://www.w3.org/2000/xmlns/");
            aa.Value = "urn:schemas-microsoft-com:xml-msdata";
            element.SetAttributeNode(aa);

            element.AppendChild(instruction);
            element.AppendChild(cDataSection);

            doc.AppendChild(element);

            jsonText = JsonConvert.SerializeXmlNode(element, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""xs:Choice"": {
    ""@msdata:IsDataSet"": """",
    ""@xmlns:xs"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}", jsonText);
        }

        [Test]
        public void SerializeNodeTypes_Encoding()
        {
            XmlNode node = DeserializeXmlNode(@"{
  ""xs!:Choice!"": {
    ""@msdata:IsDataSet!"": """",
    ""@xmlns:xs!"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}");

            Assert.AreEqual(@"<xs_x0021_:Choice_x0021_ msdata:IsDataSet_x0021_="""" xmlns:xs_x0021_=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata""><?xml-stylesheet href=""classic.xsl"" type=""text/xml""?><![CDATA[<Kiwi>true</Kiwi>]]></xs_x0021_:Choice_x0021_>", node.InnerXml);

            string json = SerializeXmlNode(node);

            StringAssert.AreEqual(@"{
  ""xs!:Choice!"": {
    ""@msdata:IsDataSet!"": """",
    ""@xmlns:xs!"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}", json);
        }

        [Test]
        public void DocumentFragmentSerialize()
        {
            XmlDocument doc = new XmlDocument();

            XmlDocumentFragment fragement = doc.CreateDocumentFragment();

            fragement.InnerXml = "<Item>widget</Item><Item>widget</Item>";

            string jsonText = JsonConvert.SerializeXmlNode(fragement);

            string expected = @"{""Item"":[""widget"",""widget""]}";

            Assert.AreEqual(expected, jsonText);
        }

#if !NETSTANDARD1_3
        [Test]
        public void XmlDocumentTypeSerialize()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE STOCKQUOTE PUBLIC ""-//W3C//DTD StockQuote 1.5//EN"" ""http://www.idontexistnopenopewhatnope123.org/dtd/stockquote_1.5.dtd""><STOCKQUOTE ROWCOUNT=""2""><RESULT><ROW><ASK>0</ASK><BID>0</BID><CHANGE>-16.310</CHANGE><COMPANYNAME>Dow Jones</COMPANYNAME><DATETIME>2014-04-17 15:50:37</DATETIME><DIVIDEND>0</DIVIDEND><EPS>0</EPS><EXCHANGE></EXCHANGE><HIGH>16460.490</HIGH><LASTDATETIME>2014-04-17 15:50:37</LASTDATETIME><LASTPRICE>16408.540</LASTPRICE><LOW>16368.140</LOW><OPEN>16424.140</OPEN><PCHANGE>-0.099</PCHANGE><PE>0</PE><PREVIOUSCLOSE>16424.850</PREVIOUSCLOSE><SHARES>0</SHARES><TICKER>DJII</TICKER><TRADES>0</TRADES><VOLUME>136188700</VOLUME><YEARHIGH>11309.000</YEARHIGH><YEARLOW>9302.280</YEARLOW><YIELD>0</YIELD></ROW><ROW><ASK>0</ASK><BID>0</BID><CHANGE>9.290</CHANGE><COMPANYNAME>NASDAQ</COMPANYNAME><DATETIME>2014-04-17 15:40:01</DATETIME><DIVIDEND>0</DIVIDEND><EPS>0</EPS><EXCHANGE></EXCHANGE><HIGH>4110.460</HIGH><LASTDATETIME>2014-04-17 15:40:01</LASTDATETIME><LASTPRICE>4095.520</LASTPRICE><LOW>4064.700</LOW><OPEN>4080.300</OPEN><PCHANGE>0.227</PCHANGE><PE>0</PE><PREVIOUSCLOSE>4086.230</PREVIOUSCLOSE><SHARES>0</SHARES><TICKER>COMP</TICKER><TRADES>0</TRADES><VOLUME>1784210100</VOLUME><YEARHIGH>4371.710</YEARHIGH><YEARLOW>3154.960</YEARLOW><YIELD>0</YIELD></ROW></RESULT><STATUS>Couldn't find ticker: SPIC?</STATUS><STATUSCODE>2</STATUSCODE></STOCKQUOTE>";

            string expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""!DOCTYPE"": {
    ""@name"": ""STOCKQUOTE"",
    ""@public"": ""-//W3C//DTD StockQuote 1.5//EN"",
    ""@system"": ""http://www.idontexistnopenopewhatnope123.org/dtd/stockquote_1.5.dtd""
  },
  ""STOCKQUOTE"": {
    ""@ROWCOUNT"": ""2"",
    ""RESULT"": {
      ""ROW"": [
        {
          ""ASK"": ""0"",
          ""BID"": ""0"",
          ""CHANGE"": ""-16.310"",
          ""COMPANYNAME"": ""Dow Jones"",
          ""DATETIME"": ""2014-04-17 15:50:37"",
          ""DIVIDEND"": ""0"",
          ""EPS"": ""0"",
          ""EXCHANGE"": """",
          ""HIGH"": ""16460.490"",
          ""LASTDATETIME"": ""2014-04-17 15:50:37"",
          ""LASTPRICE"": ""16408.540"",
          ""LOW"": ""16368.140"",
          ""OPEN"": ""16424.140"",
          ""PCHANGE"": ""-0.099"",
          ""PE"": ""0"",
          ""PREVIOUSCLOSE"": ""16424.850"",
          ""SHARES"": ""0"",
          ""TICKER"": ""DJII"",
          ""TRADES"": ""0"",
          ""VOLUME"": ""136188700"",
          ""YEARHIGH"": ""11309.000"",
          ""YEARLOW"": ""9302.280"",
          ""YIELD"": ""0""
        },
        {
          ""ASK"": ""0"",
          ""BID"": ""0"",
          ""CHANGE"": ""9.290"",
          ""COMPANYNAME"": ""NASDAQ"",
          ""DATETIME"": ""2014-04-17 15:40:01"",
          ""DIVIDEND"": ""0"",
          ""EPS"": ""0"",
          ""EXCHANGE"": """",
          ""HIGH"": ""4110.460"",
          ""LASTDATETIME"": ""2014-04-17 15:40:01"",
          ""LASTPRICE"": ""4095.520"",
          ""LOW"": ""4064.700"",
          ""OPEN"": ""4080.300"",
          ""PCHANGE"": ""0.227"",
          ""PE"": ""0"",
          ""PREVIOUSCLOSE"": ""4086.230"",
          ""SHARES"": ""0"",
          ""TICKER"": ""COMP"",
          ""TRADES"": ""0"",
          ""VOLUME"": ""1784210100"",
          ""YEARHIGH"": ""4371.710"",
          ""YEARLOW"": ""3154.960"",
          ""YIELD"": ""0""
        }
      ]
    },
    ""STATUS"": ""Couldn't find ticker: SPIC?"",
    ""STATUSCODE"": ""2""
  }
}";

            XmlDocument doc1 = new XmlDocument();
            doc1.XmlResolver = null;
            doc1.LoadXml(xml);

            string json1 = JsonConvert.SerializeXmlNode(doc1, Formatting.Indented);

            StringAssert.AreEqual(expected, json1);

            XmlDocument doc11 = JsonConvert.DeserializeXmlNode(json1);

            StringAssert.AreEqual(xml, ToStringWithDeclaration(doc11));

#if !NET20
            XDocument doc2 = XDocument.Parse(xml);

            string json2 = JsonConvert.SerializeXNode(doc2, Formatting.Indented);

            StringAssert.AreEqual(expected, json2);

            XDocument doc22 = JsonConvert.DeserializeXNode(json2);

            StringAssert.AreEqual(xml, ToStringWithDeclaration(doc22));
#endif
        }
#endif

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }
        }

#if !NET20
        public static string ToStringWithDeclaration(XDocument doc, bool indent = false)
        {
            StringBuilder builder = new StringBuilder();
            using (var writer = XmlWriter.Create(new Utf8StringWriter(builder), new XmlWriterSettings { Indent = indent }))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }
#endif

        public static string ToStringWithDeclaration(XmlDocument doc, bool indent = false)
        {
            StringBuilder builder = new StringBuilder();
            using (var writer = XmlWriter.Create(new Utf8StringWriter(builder), new XmlWriterSettings { Indent = indent }))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        [Test]
        public void NamespaceSerializeDeserialize()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<xs:schema xs:id=""SomeID"" 
	xmlns="""" 
	xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
	xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xs:element name=""MyDataSet"" msdata:IsDataSet=""true"">
		<xs:complexType>
			<xs:choice maxOccurs=""unbounded"">
				<xs:element name=""customers"" >
					<xs:complexType >
						<xs:sequence>
							<xs:element name=""CustomerID"" type=""xs:integer"" 
										 minOccurs=""0"" />
							<xs:element name=""CompanyName"" type=""xs:string"" 
										 minOccurs=""0"" />
							<xs:element name=""Phone"" type=""xs:string"" />
						</xs:sequence>
					</xs:complexType>
				</xs:element>
			</xs:choice>
		</xs:complexType>
	</xs:element>
</xs:schema>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);

            string expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""xs:schema"": {
    ""@xs:id"": ""SomeID"",
    ""@xmlns"": """",
    ""@xmlns:xs"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""xs:element"": {
      ""@name"": ""MyDataSet"",
      ""@msdata:IsDataSet"": ""true"",
      ""xs:complexType"": {
        ""xs:choice"": {
          ""@maxOccurs"": ""unbounded"",
          ""xs:element"": {
            ""@name"": ""customers"",
            ""xs:complexType"": {
              ""xs:sequence"": {
                ""xs:element"": [
                  {
                    ""@name"": ""CustomerID"",
                    ""@type"": ""xs:integer"",
                    ""@minOccurs"": ""0""
                  },
                  {
                    ""@name"": ""CompanyName"",
                    ""@type"": ""xs:string"",
                    ""@minOccurs"": ""0""
                  },
                  {
                    ""@name"": ""Phone"",
                    ""@type"": ""xs:string""
                  }
                ]
              }
            }
          }
        }
      }
    }
  }
}";

            StringAssert.AreEqual(expected, jsonText);

            XmlDocument deserializedDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, deserializedDoc.InnerXml);
        }

        [Test]
        public void FailOnIncomplete()
        {
            string json = @"{'Row' : ";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeXmlNode(json, "ROOT"),
                "Unexpected end when reading JSON. Path 'Row', line 1, position 9.");
        }

        [Test]
        public void DocumentDeserialize()
        {
            string jsonText = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""span"": {
        ""@class"": ""summary"",
        ""#text"": ""Web 2.0 Conference"",
        ""#cdata-section"": ""my escaped text""
      },
      ""@href"": ""http://www.web2con.com/""
    }
  }
}";

            XmlDocument doc = (XmlDocument)DeserializeXmlNode(jsonText);

            string expected = @"<?xml version=""1.0"" standalone=""no""?>
<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">
    <span class=""summary"">Web 2.0 Conference<![CDATA[my escaped text]]></span>
  </a>
</span>";

            string formattedXml = GetIndentedInnerXml(doc);

            StringAssert.AreEqual(expected, formattedXml);
        }

        private string GetIndentedInnerXml(XmlNode node)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            StringWriter sw = new StringWriter();

            using (XmlWriter writer = XmlWriter.Create(sw, settings))
            {
                node.WriteTo(writer);
            }

            return sw.ToString();
        }

        public class Foo2
        {
            public XmlElement Bar { get; set; }
        }

        [Test]
        public void SerializeAndDeserializeXmlElement()
        {
            Foo2 foo = new Foo2 { Bar = null };
            string json = JsonConvert.SerializeObject(foo);

            Assert.AreEqual(@"{""Bar"":null}", json);
            Foo2 foo2 = JsonConvert.DeserializeObject<Foo2>(json);

            Assert.IsNull(foo2.Bar);
        }

        [Test]
        public void SingleTextNode()
        {
            string xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
	  			<name>Alan</name>
		  		<url>http://www.google.com</url>
			  </person>
			  <person id=""2"">
			  	<name>Louis</name>
				  <url>http://www.yahoo.com</url>
			  </person>
			</root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Test]
        public void EmptyNode()
        {
            string xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
				<name>Alan</name>
				<url />
			  </person>
			  <person id=""2"">
				<name>Louis</name>
				<url>http://www.yahoo.com</url>
			  </person>
			</root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""root"": {
    ""person"": [
      {
        ""@id"": ""1"",
        ""name"": ""Alan"",
        ""url"": null
      },
      {
        ""@id"": ""2"",
        ""name"": ""Louis"",
        ""url"": ""http://www.yahoo.com""
      }
    ]
  }
}", jsonText);

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Test]
        public void OtherElementDataTypes()
        {
            string jsonText = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""Float"":2.5,""Integer"":99},{""Boolean"":true,""@id"":""2"",""date"":""\/Date(954374400000)\/""}]}}";

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            string expected = @"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><Float>2.5</Float><Integer>99</Integer></person><person id=""2""><Boolean>true</Boolean><date>2000-03-30T00:00:00Z</date></person></root>";

            Assert.AreEqual(expected, newDoc.InnerXml);
        }

        [Test]
        public void NoRootObject()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"[1]"); }, "XmlNodeConverter can only convert JSON that begins with an object. Path '', line 1, position 1.");
        }

        [Test]
        public void RootObjectMultipleProperties()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => { XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"{Prop1:1,Prop2:2}"); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'Prop2', line 1, position 15.");
        }

        [Test]
        public void JavaScriptConstructor()
        {
            string jsonText = @"{root:{r:new Date(34343, 55)}}";

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            string expected = @"<root><r><Date>34343</Date><Date>55</Date></r></root>";

            Assert.AreEqual(expected, newDoc.InnerXml);

            string json = SerializeXmlNode(newDoc);
            expected = @"{
  ""root"": {
    ""r"": {
      ""Date"": [
        ""34343"",
        ""55""
      ]
    }
  }
}";

            StringAssert.AreEqual(expected, json);
        }

        [Test]
        public void ForceJsonArray()
        {
            string arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"">Admin</role>
			  </person>
			</root>";

            XmlDocument arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            string arrayJsonText = SerializeXmlNode(arrayDoc);
            string expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"">Admin1</role>
				  <role json:Array=""true"">Admin2</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin1"",
        ""Admin2""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""false"">Admin1</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": ""Admin1""
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root>
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">Admin</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);
        }

        [Test]
        public void MultipleRootPropertiesXmlDocument()
        {
            string json = @"{""count"": 773840,""photos"": null}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'photos', line 1, position 26.");
        }
#endif

#if !NET20
        [Test]
        public void MultipleRootPropertiesXDocument()
        {
            string json = @"{""count"": 773840,""photos"": null}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'photos', line 1, position 26.");
        }
#endif

        [Test]
        public void MultipleRootPropertiesAddRootElement()
        {
            string json = @"{""count"": 773840,""photos"": 773840}";

#if !PORTABLE
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newDoc.InnerXml);
#endif

#if !NET20
            XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
#endif
        }

        [Test]
        public void NestedArrays()
        {
            string json = @"{
  ""available_sizes"": [
    [
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ]
  ]
}";

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            string xml = IndentXml(newDoc.InnerXml);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newDoc.InnerXml));
#endif

#if !NET20
            XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newXDoc.ToString(SaveOptions.DisableFormatting)));
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
            string newJson = JsonConvert.SerializeXmlNode(newDoc, Formatting.Indented);
            Console.WriteLine(newJson);
#endif
        }

        [Test]
        public void RoundTripNestedArrays()
        {
            string json = @"{
  ""available_sizes"": [
    [
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ]
  ]
}";

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot", true);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes json:Array=""true"">assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newDoc.InnerXml));
#endif

#if !NET20
            XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot", true);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes json:Array=""true"">assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newXDoc.ToString(SaveOptions.DisableFormatting)));
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
            string newJson = JsonConvert.SerializeXmlNode(newDoc, Formatting.Indented, true);
            StringAssert.AreEqual(json, newJson);
#endif
        }

        [Test]
        public void MultipleNestedArraysToXml()
        {
            string json = @"{
  ""available_sizes"": [
    [
      [113, 150],
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      [189, 250],
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      [341, 450],
      ""assets/images/resized/0001/1070/11070v1-max-450x450.jpg""
    ]
  ]
}";

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
            XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);
#endif

#if !NET20
            XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
#endif
        }

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void Encoding()
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(@"<name>O""Connor</name>"); // i use "" so it will be easier to see the  problem

            string json = SerializeXmlNode(doc);
            StringAssert.AreEqual(@"{
  ""name"": ""O\""Connor""
}", json);
        }
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void SerializeComment()
        {
            string xml = @"<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/""><!-- Hi --><span>Text</span></a><!-- Hi! -->
</span>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);

            string expected = @"{
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/""/* Hi */,
      ""span"": ""Text""
    }/* Hi! */
  }
}";

            StringAssert.AreEqual(expected, jsonText);

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);
            Assert.AreEqual(@"<span class=""vevent""><a class=""url"" href=""http://www.web2con.com/""><!-- Hi --><span>Text</span></a><!-- Hi! --></span>", newDoc.InnerXml);
        }

        [Test]
        public void SerializeExample()
        {
            string xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
				<name>Alan</name>
				<url>http://www.google.com</url>
			  </person>
			  <person id=""2"">
				<name>Louis</name>
				<url>http://www.yahoo.com</url>
			  </person>
			</root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string jsonText = SerializeXmlNode(doc);
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

            // format
            jsonText = JObject.Parse(jsonText).ToString();

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""root"": {
    ""person"": [
      {
        ""@id"": ""1"",
        ""name"": ""Alan"",
        ""url"": ""http://www.google.com""
      },
      {
        ""@id"": ""2"",
        ""name"": ""Louis"",
        ""url"": ""http://www.yahoo.com""
      }
    ]
  }
}", jsonText);

            XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Test]
        public void DeserializeExample()
        {
            string json = @"{
        ""?xml"": {
          ""@version"": ""1.0"",
          ""@standalone"": ""no""
        },
        ""root"": {
          ""person"": [
            {
              ""@id"": ""1"",
              ""name"": ""Alan"",
              ""url"": ""http://www.google.com""
            },
            {
              ""@id"": ""2"",
              ""name"": ""Louis"",
              ""url"": ""http://www.yahoo.com""
            }
          ]
        }
      }";

            XmlDocument doc = (XmlDocument)DeserializeXmlNode(json);
            // <?xml version="1.0" standalone="no"?>
            // <root>
            //   <person id="1">
            //   <name>Alan</name>
            //   <url>http://www.google.com</url>
            //   </person>
            //   <person id="2">
            //   <name>Louis</name>
            //   <url>http://www.yahoo.com</url>
            //   </person>
            // </root>

            StringAssert.AreEqual(@"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><name>Alan</name><url>http://www.google.com</url></person><person id=""2""><name>Louis</name><url>http://www.yahoo.com</url></person></root>", doc.InnerXml);
        }

        [Test]
        public void EscapingNames()
        {
            string json = @"{
              ""root!"": {
                ""person!"": [
                  {
                    ""@id!"": ""1"",
                    ""name!"": ""Alan"",
                    ""url!"": ""http://www.google.com""
                  },
                  {
                    ""@id!"": ""2"",
                    ""name!"": ""Louis"",
                    ""url!"": ""http://www.yahoo.com""
                  }
                ]
              }
            }";

            XmlDocument doc = (XmlDocument)DeserializeXmlNode(json);

            Assert.AreEqual(@"<root_x0021_><person_x0021_ id_x0021_=""1""><name_x0021_>Alan</name_x0021_><url_x0021_>http://www.google.com</url_x0021_></person_x0021_><person_x0021_ id_x0021_=""2""><name_x0021_>Louis</name_x0021_><url_x0021_>http://www.yahoo.com</url_x0021_></person_x0021_></root_x0021_>", doc.InnerXml);

            string json2 = SerializeXmlNode(doc);

            StringAssert.AreEqual(@"{
  ""root!"": {
    ""person!"": [
      {
        ""@id!"": ""1"",
        ""name!"": ""Alan"",
        ""url!"": ""http://www.google.com""
      },
      {
        ""@id!"": ""2"",
        ""name!"": ""Louis"",
        ""url!"": ""http://www.yahoo.com""
      }
    ]
  }
}", json2);
        }

        [Test]
        public void SerializeDeserializeMetadataProperties()
        {
            PreserveReferencesHandlingTests.CircularDictionary circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
            circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}", json);

            XmlNode node = DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);
            string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <other json:id=""2"">
    <blah />
  </other>
  <self json:ref=""1"" />
</root>";

            StringAssert.AreEqual(expected, xml);

            string xmlJson = SerializeXmlNode(node);
            string expectedXmlJson = @"{
  ""root"": {
    ""$id"": ""1"",
    ""other"": {
      ""$id"": ""2"",
      ""blah"": null
    },
    ""self"": {
      ""$ref"": ""1""
    }
  }
}";

            StringAssert.AreEqual(expectedXmlJson, xmlJson);
        }

        [Test]
        public void SerializeDeserializeMetadataArray()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            string newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(json, newJson);
        }

        [Test]
        public void SerializeDeserializeMetadataArrayNoId()
        {
            string json = @"{
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            string newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            Console.WriteLine(newJson);

            StringAssert.AreEqual(json, newJson);
        }

        [Test]
        public void SerializeDeserializeMetadataArrayWithIdLast()
        {
            string json = @"{
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ],
  ""$id"": ""1""
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            string newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}", newJson);
        }

        [Test]
        public void SerializeMetadataPropertyWithBadValue()
        {
            string json = @"{
  ""$id"": []
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json, "root"); },
                "Unexpected JsonToken: StartArray. Path '$id', line 2, position 10.");
        }

        [Test]
        public void SerializeDeserializeMetadataWithNullValue()
        {
            string json = @"{
  ""$id"": null
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id="""" />", xml);

            string newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""$id"": """"
}", newJson);
        }

        [Test]
        public void SerializeDeserializeMetadataArrayNull()
        {
            string json = @"{
  ""$id"": ""1"",
  ""$values"": null
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            string xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"" />
</root>", xml);

            string newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(json, newJson);
        }

        [Test]
        public void EmptyPropertyName()
        {
            string json = @"{
  ""8452309520V2"": {
    """": {
      ""CLIENT"": {
        ""ID_EXPIRATION_1"": {
          ""VALUE"": ""12/12/2000"",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Identification Exp. Date 1""
        },
        ""ID_ISSUEDATE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Identification Issue Date 1""
        }
      }
    },
    ""457463534534"": {
      ""ACCOUNT"": {
        ""FUNDING_SOURCE"": {
          ""VALUE"": ""FS0"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Source of Funds""
        }
      }
    }
  }
}{
  ""34534634535345"": {
    """": {
      ""CLIENT"": {
        ""ID_NUMBER_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""S"",
          ""MSG"": ""Missing Picture ID""
        },
        ""ID_EXPIRATION_1"": {
          ""VALUE"": ""12/12/2000"",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Picture ID""
        },
        ""WALK_IN"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Walk in""
        },
        ""PERSONAL_MEETING"": {
          ""VALUE"": ""PM1"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Met Client in Person""
        },
        ""ID_ISSUEDATE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Picture ID""
        },
        ""PHOTO_ID"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Picture ID""
        },
        ""ID_TYPE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Picture ID""
        }
      }
    },
    ""45635624523"": {
      ""ACCOUNT"": {
        ""FUNDING_SOURCE"": {
          ""VALUE"": ""FS1"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Source of Funds""
        }
      }
    }
  }
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { DeserializeXmlNode(json); },
                "XmlNodeConverter cannot convert JSON with an empty property name to XML. Path '8452309520V2.', line 3, position 9.");
        }

        [Test]
        public void SingleItemArrayPropertySerialization()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28, 0, 0, 0, DateTimeKind.Utc);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small" };

            string output = JsonConvert.SerializeObject(product, new IsoDateTimeConverter());

            XmlDocument xmlProduct = JsonConvert.DeserializeXmlNode(output, "product", true);

            StringAssert.AreEqual(@"<product>
  <Name>Apple</Name>
  <ExpiryDate>2008-12-28T00:00:00Z</ExpiryDate>
  <Price>3.99</Price>
  <Sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">Small</Sizes>
</product>", IndentXml(xmlProduct.InnerXml));

            string output2 = JsonConvert.SerializeXmlNode(xmlProduct.DocumentElement, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""product"": {
    ""Name"": ""Apple"",
    ""ExpiryDate"": ""2008-12-28T00:00:00Z"",
    ""Price"": ""3.99"",
    ""Sizes"": [
      ""Small""
    ]
  }
}", output2);
        }

        public class TestComplexArrayClass
        {
            public string Name { get; set; }
            public IList<Product> Products { get; set; }
        }

        [Test]
        public void ComplexSingleItemArrayPropertySerialization()
        {
            TestComplexArrayClass o = new TestComplexArrayClass
            {
                Name = "Hi",
                Products = new List<Product>
                {
                    new Product { Name = "First" }
                }
            };

            string output = JsonConvert.SerializeObject(o, new IsoDateTimeConverter());

            XmlDocument xmlProduct = JsonConvert.DeserializeXmlNode(output, "test", true);

            StringAssert.AreEqual(@"<test>
  <Name>Hi</Name>
  <Products json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <Name>First</Name>
    <ExpiryDate>2000-01-01T00:00:00Z</ExpiryDate>
    <Price>0</Price>
    <Sizes />
  </Products>
</test>", IndentXml(xmlProduct.InnerXml));

            string output2 = JsonConvert.SerializeXmlNode(xmlProduct.DocumentElement, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""Name"": ""Hi"",
  ""Products"": [
    {
      ""Name"": ""First"",
      ""ExpiryDate"": ""2000-01-01T00:00:00Z"",
      ""Price"": ""0"",
      ""Sizes"": null
    }
  ]
}", output2);
        }

        [Test]
        public void OmitRootObject()
        {
            string xml = @"<test>
  <Name>Hi</Name>
  <Name>Hi</Name>
  <Products json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <Name>First</Name>
    <ExpiryDate>2000-01-01T00:00:00Z</ExpiryDate>
    <Price>0</Price>
    <Sizes />
  </Products>
</test>";

            XmlDocument d = new XmlDocument();
            d.LoadXml(xml);

            string output = JsonConvert.SerializeXmlNode(d, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""Name"": [
    ""Hi"",
    ""Hi""
  ],
  ""Products"": [
    {
      ""Name"": ""First"",
      ""ExpiryDate"": ""2000-01-01T00:00:00Z"",
      ""Price"": ""0"",
      ""Sizes"": null
    }
  ]
}", output);
        }

        [Test]
        public void EmtpyElementWithArrayAttributeShouldWriteAttributes()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<root xmlns:json=""http://james.newtonking.com/projects/json"">
<A>
<B name=""sample"" json:Array=""true""/>
<C></C>
<C></C>
</A>
</root>";

            XmlDocument d = new XmlDocument();
            d.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""root"": {
    ""A"": {
      ""B"": [
        {
          ""@name"": ""sample""
        }
      ],
      ""C"": [
        """",
        """"
      ]
    }
  }
}", json);

            XmlDocument d2 = JsonConvert.DeserializeXmlNode(json);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <A>
    <B name=""sample"" />
    <C></C>
    <C></C>
  </A>
</root>", ToStringWithDeclaration(d2, true));
        }

        [Test]
        public void EmtpyElementWithArrayAttributeShouldWriteElement()
        {
            string xml = @"<root>
<Reports d1p1:Array=""true"" xmlns:d1p1=""http://james.newtonking.com/projects/json"" />
</root>";

            XmlDocument d = new XmlDocument();
            d.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""root"": {
    ""Reports"": [
      {}
    ]
  }
}", json);
        }

        [Test]
        public void DeserializeNonInt64IntegerValues()
        {
            var dict = new Dictionary<string, object> { { "Int16", (short)1 }, { "Float", 2f }, { "Int32", 3 } };
            var obj = JObject.FromObject(dict);
            var serializer = JsonSerializer.Create(new JsonSerializerSettings { Converters = { new XmlNodeConverter() { DeserializeRootElementName = "root" } } });
            using (var reader = obj.CreateReader())
            {
                var value = (XmlDocument)serializer.Deserialize(reader, typeof(XmlDocument));

                Assert.AreEqual(@"<root><Int16>1</Int16><Float>2</Float><Int32>3</Int32></root>", value.InnerXml);
            }
        }

        [Test]
        public void DeserializingBooleanValues()
        {
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(@"{root:{""@booleanType"":true}}"));
            MemoryStream xml = new MemoryStream();

            JsonBodyToSoapXml(ms, xml);

            string xmlString = System.Text.Encoding.UTF8.GetString(xml.ToArray());

            Assert.AreEqual(@"﻿<?xml version=""1.0"" encoding=""utf-8""?><root booleanType=""true"" />", xmlString);
        }

#if !(NETSTANDARD1_0 || NETSTANDARD1_3)
        [Test]
        public void IgnoreCultureForTypedAttributes()
        {
            var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

                // in russian culture value 12.27 will be written as 12,27

                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var json = new StringBuilder(@"{
                    ""metrics"": {
                        ""type"": ""CPULOAD"",
                        ""@value"": 12.27
                    }
                }");

                using (var stringReader = new StringReader(json.ToString()))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));
                    StringAssert.AreEqual(@"<metrics value=""12.27""><type>CPULOAD</type></metrics>", document.OuterXml);
                }
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
#endif

        [Test]
        public void NullAttributeValue()
        {
            var node = JsonConvert.DeserializeXmlNode(@"{
                    ""metrics"": {
                        ""type"": ""CPULOAD"",
                        ""@value"": null
                    }
                }");

            StringAssert.AreEqual(@"<metrics value=""""><type>CPULOAD</type></metrics>", node.OuterXml);
        }

        [Test]
        public void NonStandardAttributeValues()
        {
            JObject o = new JObject
            {
                new JProperty("root", new JObject
                {
                    new JProperty("@uri", new JValue(new Uri("http://localhost/"))),
                    new JProperty("@time_span", new JValue(TimeSpan.FromMinutes(1))),
                    new JProperty("@bytes", new JValue(System.Text.Encoding.UTF8.GetBytes("Hello world")))
                })
            };

            using (var jsonReader = o.CreateReader())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));

                StringAssert.AreEqual(@"<root uri=""http://localhost/"" time_span=""00:01:00"" bytes=""SGVsbG8gd29ybGQ="" />", document.OuterXml);
            }
        }

        [Test]
        public void NonStandardElementsValues()
        {
            JObject o = new JObject
            {
                new JProperty("root", new JObject
                {
                    new JProperty("uri", new JValue(new Uri("http://localhost/"))),
                    new JProperty("time_span", new JValue(TimeSpan.FromMinutes(1))),
                    new JProperty("bytes", new JValue(System.Text.Encoding.UTF8.GetBytes("Hello world")))
                })
            };

            using (var jsonReader = o.CreateReader())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));

                StringAssert.AreEqual(@"<root><uri>http://localhost/</uri><time_span>00:01:00</time_span><bytes>SGVsbG8gd29ybGQ=</bytes></root>", document.OuterXml);
            }
        }

        private static void JsonBodyToSoapXml(Stream json, Stream xml)
        {
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.Converters.Add(new Newtonsoft.Json.Converters.XmlNodeConverter());
            Newtonsoft.Json.JsonSerializer serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
            using (Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StreamReader(json)))
            {
                XmlDocument doc = (XmlDocument)serializer.Deserialize(reader, typeof(XmlDocument));
                if (reader.Read() && reader.TokenType != JsonToken.Comment)
                {
                    throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
                }
                using (XmlWriter writer = XmlWriter.Create(xml))
                {
                    doc.Save(writer);
                }
            }
        }
#endif

#if !NET20
        [Test]
        public void DeserializeXNodeDefaultNamespace()
        {
            string xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            string json = JsonConvert.SerializeXNode(XDocument.Parse(xaml), Formatting.Indented);

            string expectedJson = @"{
  ""Grid"": {
    ""@xmlns"": ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"",
    ""@xmlns:x"": ""http://schemas.microsoft.com/winfx/2006/xaml"",
    ""@xmlns:toolkit"": ""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"",
    ""@Style"": ""{StaticResource trimFormGrid}"",
    ""@x:Name"": ""TrimObjectForm"",
    ""Grid.ColumnDefinitions"": {
      ""ColumnDefinition"": [
        {
          ""@Width"": ""63*""
        },
        {
          ""@Width"": ""320*""
        }
      ]
    },
    ""Grid.RowDefinitions"": {
      ""@xmlns"": """",
      ""RowDefinition"": [
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ]
    },
    ""TextBox"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordTypedTitle"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordExternalReference"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Author, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAuthor"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Container, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordContainer"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordIsEnclosed"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAssignee"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ],
    ""toolkit:DatePicker"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateCreated"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""2""
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateDue"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""3""
      }
    ],
    ""TextBlock"": [
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Title (Free Text Part)"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""External ID"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Created"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""2"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Due"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""3"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Author"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Container"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Enclosed?"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Assignee"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ]
  }
}";

            StringAssert.AreEqual(expectedJson, json);

            XNode node = JsonConvert.DeserializeXNode(json);

            string xaml2 = node.ToString();

            string expectedXaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            StringAssert.AreEqual(expectedXaml, xaml2);
        }
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void DeserializeXmlNodeDefaultNamespace()
        {
            string xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            XmlDocument document = new XmlDocument();
            document.LoadXml(xaml);

            string json = JsonConvert.SerializeXmlNode(document, Formatting.Indented);

            string expectedJson = @"{
  ""Grid"": {
    ""@xmlns"": ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"",
    ""@xmlns:x"": ""http://schemas.microsoft.com/winfx/2006/xaml"",
    ""@xmlns:toolkit"": ""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"",
    ""@Style"": ""{StaticResource trimFormGrid}"",
    ""@x:Name"": ""TrimObjectForm"",
    ""Grid.ColumnDefinitions"": {
      ""ColumnDefinition"": [
        {
          ""@Width"": ""63*""
        },
        {
          ""@Width"": ""320*""
        }
      ]
    },
    ""Grid.RowDefinitions"": {
      ""@xmlns"": """",
      ""RowDefinition"": [
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ]
    },
    ""TextBox"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordTypedTitle"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordExternalReference"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Author, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAuthor"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Container, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordContainer"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordIsEnclosed"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAssignee"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ],
    ""toolkit:DatePicker"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateCreated"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""2""
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateDue"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""3""
      }
    ],
    ""TextBlock"": [
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Title (Free Text Part)"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""External ID"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Created"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""2"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Due"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""3"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Author"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Container"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Enclosed?"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Assignee"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ]
  }
}";

            StringAssert.AreEqual(expectedJson, json);

            XmlNode node = JsonConvert.DeserializeXmlNode(json);

            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            });
            node.WriteTo(writer);
            writer.Flush();

            string xaml2 = sw.ToString();

            string expectedXaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            StringAssert.AreEqual(expectedXaml, xaml2);
        }

        [Test]
        public void DeserializeAttributePropertyNotAtStart()
        {
            string json = @"{""item"": {""@action"": ""update"", ""@itemid"": ""1"", ""elements"": [{""@action"": ""none"", ""@id"": ""2""},{""@action"": ""none"", ""@id"": ""3""}],""@description"": ""temp""}}";

            XmlDocument xmldoc = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual(@"<item action=""update"" itemid=""1"" description=""temp""><elements action=""none"" id=""2"" /><elements action=""none"" id=""3"" /></item>", xmldoc.InnerXml);
        }
#endif

        [Test]
        public void SerializingXmlNamespaceScope()
        {
            var xmlString = @"<root xmlns=""http://www.example.com/ns"">
  <a/>
  <bns:b xmlns:bns=""http://www.example.com/ns""/>
  <c/>
</root>";

#if !NET20
            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeObject(xml);

            Assert.AreEqual(@"{""root"":{""@xmlns"":""http://www.example.com/ns"",""a"":null,""bns:b"":{""@xmlns:bns"":""http://www.example.com/ns""},""c"":null}}", json1);
#endif
#if !(PORTABLE)
            var xml1 = new XmlDocument();
            xml1.LoadXml(xmlString);

            var json2 = JsonConvert.SerializeObject(xml1);

            Assert.AreEqual(@"{""root"":{""@xmlns"":""http://www.example.com/ns"",""a"":null,""bns:b"":{""@xmlns:bns"":""http://www.example.com/ns""},""c"":null}}", json2);
#endif
        }

#if !NET20
        public class NullableXml
        {
            public string Name;
            public XElement notNull;
            public XElement isNull;
        }

        [Test]
        public void SerializeAndDeserializeNullableXml()
        {
            var xml = new NullableXml { Name = "test", notNull = XElement.Parse("<root>test</root>") };
            var json = JsonConvert.SerializeObject(xml);

            var w2 = JsonConvert.DeserializeObject<NullableXml>(json);
            Assert.AreEqual(xml.Name, w2.Name);
            Assert.AreEqual(xml.isNull, w2.isNull);
            Assert.AreEqual(xml.notNull.ToString(), w2.notNull.ToString());
        }
#endif

#if !NET20
        [Test]
        public void SerializeAndDeserializeXElementWithNamespaceInChildrenRootDontHaveNameSpace()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'>Asd</b>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeXNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XElement>(json1);

            var equals = XElement.DeepEquals(xmlBack, xml);
            Assert.IsTrue(equals);
        }
#endif

#if !PORTABLE || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void SerializeAndDeserializeXmlElementWithNamespaceInChildrenRootDontHaveNameSpace()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'>Asd</b>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var json1 = JsonConvert.SerializeXmlNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XmlDocument>(json1);

            Assert.AreEqual(@"<root><b xmlns=""http://www.example.com/ns"">Asd</b><c>AAA</c><test>adad</test></root>", xmlBack.OuterXml);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void DeserializeBigInteger()
        {
            var json = "{\"DocumentId\":13779965364495889899 }";

            XmlDocument node = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual("<DocumentId>13779965364495889899</DocumentId>", node.OuterXml);

            string json2 = JsonConvert.SerializeXmlNode(node);

            Assert.AreEqual(@"{""DocumentId"":""13779965364495889899""}", json2);
        }
#endif

        [Test]
        public void DeserializeXmlIncompatibleCharsInPropertyName()
        {
            var json = "{\"%name\":\"value\"}";

            XmlDocument node = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual("<_x0025_name>value</_x0025_name>", node.OuterXml);

            string json2 = JsonConvert.SerializeXmlNode(node);

            Assert.AreEqual(json, json2);
        }

        [Test]
        public void RootPropertyError()
        {
            string json = @"{
  ""$id"": ""1"",
  ""AOSLocaleName"": ""en-US"",
  ""AXLanguage"": ""EN-AU"",
  ""Company"": ""AURE"",
  ""CompanyTimeZone"": 8,
  ""CurrencyInfo"": {
    ""$id"": ""2"",
    ""CurrencyCode"": ""AUD"",
    ""Description"": ""Australian Dollar"",
    ""ExchangeRate"": 100.0,
    ""ISOCurrencyCode"": ""AUD"",
    ""Prefix"": """",
    ""Suffix"": """"
  },
  ""IsSysAdmin"": true,
  ""UserId"": ""lamar.miller"",
  ""UserPreferredCalendar"": 0,
  ""UserPreferredTimeZone"": 8
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeXmlNode(json),
                "JSON root object has property '$id' that will be converted to an attribute. A root object cannot have any attribute properties. Consider specifying a DeserializeRootElementName. Path '$id', line 2, position 12.");
        }

        [Test]
        public void SerializeEmptyNodeAndOmitRoot()
        {
            string xmlString = @"<myemptynode />";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);

            string json = JsonConvert.SerializeXmlNode(xml, Formatting.Indented, true);

            Assert.AreEqual("null", json);
        }
#endif

#if !NET20
        [Test]
        public void Serialize_XDocument_NoRoot()
        {
            XDocument d = new XDocument();

            string json = JsonConvert.SerializeXNode(d);

            Assert.AreEqual(@"{}", json);
        }

        [Test]
        public void Deserialize_XDocument_NoRoot()
        {
            XDocument d = JsonConvert.DeserializeXNode(@"{}");

            Assert.AreEqual(null, d.Root);
            Assert.AreEqual(null, d.Declaration);
        }

        [Test]
        public void Serialize_XDocument_NoRootWithDeclaration()
        {
            XDocument d = new XDocument();
            d.Declaration = new XDeclaration("Version!", "Encoding!", "Standalone!");

            string json = JsonConvert.SerializeXNode(d);

            Assert.AreEqual(@"{""?xml"":{""@version"":""Version!"",""@encoding"":""Encoding!"",""@standalone"":""Standalone!""}}", json);
        }

        [Test]
        public void Deserialize_XDocument_NoRootWithDeclaration()
        {
            XDocument d = JsonConvert.DeserializeXNode(@"{""?xml"":{""@version"":""Version!"",""@encoding"":""Encoding!"",""@standalone"":""Standalone!""}}");

            Assert.AreEqual(null, d.Root);
            Assert.AreEqual("Version!", d.Declaration.Version);
            Assert.AreEqual("Encoding!", d.Declaration.Encoding);
            Assert.AreEqual("Standalone!", d.Declaration.Standalone);
        }

        [Test]
        public void DateTimeToXml_Unspecified()
        {
            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Unspecified: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void DateTimeToXml_Utc()
        {
            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00Z""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00Z", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Utc: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void DateTimeToXml_Local()
        {
            DateTime dt = DateTime.Parse("2014-01-23T00:00:00+01:00");

            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00+01:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual(dt.ToString("yyyy-MM-ddTHH:mm:sszzzzzzz", CultureInfo.InvariantCulture), dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Local: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void DateTimeToXml_Unspecified_Precision()
        {
            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00.1234567", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Unspecified: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void DateTimeToXml_Utc_Precision()
        {
            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567Z""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00.1234567Z", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Utc: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void DateTimeToXml_Local_Precision()
        {
            DateTime dt = DateTime.Parse("2014-01-23T00:00:00.1234567+01:00");

            string json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567+01:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual(dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture), dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Local: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Test]
        public void SerializeEmptyNodeAndOmitRoot_XElement()
        {
            string xmlString = @"<myemptynode />";

            var xml = XElement.Parse(xmlString);

            string json = JsonConvert.SerializeXNode(xml, Formatting.Indented, true);

            Assert.AreEqual("null", json);
        }

        [Test]
        public void SerializeElementExplicitAttributeNamespace()
        {
            var original = XElement.Parse("<MyElement xmlns=\"http://example.com\" />");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XElement>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Test]
        public void SerializeElementImplicitAttributeNamespace()
        {
            var original = new XElement("{http://example.com}MyElement");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XElement>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Test]
        public void SerializeDocumentExplicitAttributeNamespace()
        {
            var original = XDocument.Parse("<MyElement xmlns=\"http://example.com\" />");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XDocument>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Test]
        public void SerializeDocumentImplicitAttributeNamespace()
        {
            var original = new XDocument(new XElement("{http://example.com}MyElement"));
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XDocument>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        public class Model
        {
            public XElement Document { get; set; }
        }

        [Test]
        public void DeserializeDateInElementText()
        {
            Model model = new Model();
            model.Document = new XElement("Value", new XAttribute("foo", "bar"))
            {
                Value = "2001-01-01T11:11:11"
            };

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>(new[] { new XmlNodeConverter() })
            });

            var json = new StringBuilder(1024);

            using (var stringWriter = new StringWriter(json, CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.Formatting = Formatting.None;
                serializer.Serialize(jsonWriter, model);

                Assert.AreEqual(@"{""Document"":{""Value"":{""@foo"":""bar"",""#text"":""2001-01-01T11:11:11""}}}", json.ToString());
            }

            using (var stringReader = new StringReader(json.ToString()))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                var document = (XDocument)serializer.Deserialize(jsonReader, typeof(XDocument));

                StringAssert.AreEqual(@"<Document>
  <Value foo=""bar"">2001-01-01T11:11:11</Value>
</Document>", document.ToString());
            }
        }
#endif
    }
}
#endif