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
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public class XmlNodeConverterTest
  {
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

      StringWriter sw = new StringWriter();

      using (JsonWriter jsonWriter = new JsonWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

        JsonSerializer jsonSerializer = new JsonSerializer();
        jsonSerializer.Converters.Add(new XmlNodeConverter());

        jsonSerializer.Serialize(jsonWriter, doc);
      }

      string jsonText = sw.ToString();
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

      Assert.AreEqual(expected, jsonText);

      Console.WriteLine("DocumentSerializeIndented");
      Console.WriteLine(jsonText);
      Console.WriteLine();
    }

    [Test]
    public void SerializeNodeTypes()
    {
      XmlDocument doc = new XmlDocument();
      string jsonText;

      Console.WriteLine("SerializeNodeTypes");

      // XmlAttribute
      XmlAttribute attribute = doc.CreateAttribute("msdata:IsDataSet");
      attribute.Value = "true";

      jsonText = JavaScriptConvert.SerializeXmlNode(attribute);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);


      // XmlProcessingInstruction
      XmlProcessingInstruction instruction = doc.CreateProcessingInstruction("xml-stylesheet", @"href=""classic.xsl"" type=""text/xml""");

      jsonText = JavaScriptConvert.SerializeXmlNode(instruction);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\""""}", jsonText);


      // XmlProcessingInstruction
      XmlCDataSection cDataSection = doc.CreateCDataSection("<Kiwi>true</Kiwi>");

      jsonText = JavaScriptConvert.SerializeXmlNode(cDataSection);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""#cdata-section"":""<Kiwi>true</Kiwi>""}", jsonText);


      // XmlElement
      XmlElement element = doc.CreateElement("xs:Choice");
      element.SetAttributeNode(attribute);

      element.AppendChild(instruction);
      element.AppendChild(cDataSection);

      jsonText = JavaScriptConvert.SerializeXmlNode(element);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""xs:Choice"":{""@msdata:IsDataSet"":""true"",""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\"""",""#cdata-section"":""<Kiwi>true</Kiwi>""}}", jsonText);
    }

    [Test]
    public void DocumentFragmentSerialize()
    {
      XmlDocument doc = new XmlDocument();

      XmlDocumentFragment fragement = doc.CreateDocumentFragment();

      fragement.InnerXml = "<Item>widget</Item><Item>widget</Item>";

      string jsonText = JavaScriptConvert.SerializeXmlNode(fragement);

      string expected = @"{""Item"":[""widget"",""widget""]}";

      Assert.AreEqual(expected, jsonText);

      Console.WriteLine("DocumentFragmentSerialize");
      Console.WriteLine(jsonText);
      Console.WriteLine();
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

      string jsonText = JavaScriptConvert.SerializeXmlNode(doc);

      XmlDocument deserializedDoc = (XmlDocument)JavaScriptConvert.DeerializeXmlNode(jsonText);

      Assert.AreEqual(doc.InnerXml, deserializedDoc.InnerXml);

      Console.WriteLine("NamespaceSerializeDeserialize");
      Console.WriteLine(jsonText);
      Console.WriteLine(deserializedDoc.InnerXml);
      Console.WriteLine();
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
      ""@href"": ""http://www.web2con.com/"",
      ""span"": {
        ""@class"": ""summary"",
        ""#text"": ""Web 2.0 Conference"",
        ""#cdata-section"": ""my escaped text""
      }
    }
  }
}";

      XmlDocument doc = (XmlDocument)JavaScriptConvert.DeerializeXmlNode(jsonText);

      string expected = @"<?xml version=""1.0"" standalone=""no""?>
<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">
    <span class=""summary"">Web 2.0 Conference<![CDATA[my escaped text]]></span>
  </a>
</span>";

      string formattedXml = GetIndentedInnerXml(doc);

      Console.WriteLine("DocumentDeserialize");
      Console.WriteLine(formattedXml);
      Console.WriteLine();

      Assert.AreEqual(expected, formattedXml);
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

      string jsonText = JavaScriptConvert.SerializeXmlNode(doc);

      XmlDocument newDoc = (XmlDocument)JavaScriptConvert.DeerializeXmlNode(jsonText);

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

      string jsonText = JavaScriptConvert.SerializeXmlNode(doc);

      Console.WriteLine(jsonText);

      XmlDocument newDoc = (XmlDocument)JavaScriptConvert.DeerializeXmlNode(jsonText);

      Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
    }

    [Test]
    public void OtherElementDataTypes()
    {
      string jsonText = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""Float"":2.5,""Integer"":99},{""@id"":""2"",""Boolean"":true,""date"":new Date(954374400000)}]}}";

      XmlDocument newDoc = (XmlDocument)JavaScriptConvert.DeerializeXmlNode(jsonText);

      string expected = @"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><Float>2.5</Float><Integer>99</Integer></person><person id=""2""><Boolean>true</Boolean><date>2000-03-30T00:00:00.0000000+12:00</date></person></root>";

      Assert.AreEqual(expected, newDoc.InnerXml);
    }
  }
}