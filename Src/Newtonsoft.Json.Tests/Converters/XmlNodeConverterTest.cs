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

#if !SILVERLIGHT
using System;
using Newtonsoft.Json.Tests.Serialization;
using NUnit.Framework;
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
  public class XmlNodeConverterTest : TestFixtureBase
  {
    private string SerializeXmlNode(XmlNode node)
    {
      string json = JsonConvert.SerializeXmlNode(node, Formatting.Indented);
      XmlNodeReader reader = new XmlNodeReader(node);

#if !NET20
      XObject xNode;
      if (node is XmlDocument)
      {
        xNode = XDocument.Load(reader);
      }
      else if (node is XmlAttribute)
      {
        XmlAttribute attribute = (XmlAttribute) node;
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
        converter.DeserializeRootElementName = deserializeRootElementName;

      XmlNode node = (XmlNode)converter.ReadJson(reader, typeof (XmlDocument), null, new JsonSerializer());

#if !NET20
     string xmlText = node.OuterXml;

      reader = new JsonTextReader(new StringReader(json));
      reader.Read();
      XDocument d = (XDocument) converter.ReadJson(reader, typeof (XDocument), null, new JsonSerializer());

      string linqXmlText = d.ToString(SaveOptions.DisableFormatting);
      if (d.Declaration != null)
        linqXmlText = d.Declaration + linqXmlText;

      Assert.AreEqual(xmlText, linqXmlText);
#endif

      return node;
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

      Console.WriteLine(jsonText);
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

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\""""}", jsonText);


      // XmlProcessingInstruction
      XmlCDataSection cDataSection = doc.CreateCDataSection("<Kiwi>true</Kiwi>");

      jsonText = JsonConvert.SerializeXmlNode(cDataSection);

      Console.WriteLine(jsonText);
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

      Assert.AreEqual(@"{
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
    public void DocumentFragmentSerialize()
    {
      XmlDocument doc = new XmlDocument();

      XmlDocumentFragment fragement = doc.CreateDocumentFragment();

      fragement.InnerXml = "<Item>widget</Item><Item>widget</Item>";

      string jsonText = JsonConvert.SerializeXmlNode(fragement);

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

      Assert.AreEqual(expected, jsonText);

      XmlDocument deserializedDoc = (XmlDocument)DeserializeXmlNode(jsonText);

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

      XmlDocument doc = (XmlDocument)DeserializeXmlNode(jsonText);

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

      Console.WriteLine(jsonText);

      XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

      Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
    }

    [Test]
    public void OtherElementDataTypes()
    {
      string jsonText = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""Float"":2.5,""Integer"":99},{""@id"":""2"",""Boolean"":true,""date"":""\/Date(954374400000)\/""}]}}";

      XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

      string expected = @"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><Float>2.5</Float><Integer>99</Integer></person><person id=""2""><Boolean>true</Boolean><date>2000-03-30T00:00:00Z</date></person></root>";

      Assert.AreEqual(expected, newDoc.InnerXml);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "XmlNodeConverter can only convert JSON that begins with an object.")]
    public void NoRootObject()
    {
      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"[1]");
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.")]
    public void RootObjectMultipleProperties()
    {
      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"{Prop1:1,Prop2:2}");
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

      Assert.AreEqual(expected, json);
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
      Assert.AreEqual(expected, arrayJsonText);

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
      Assert.AreEqual(expected, arrayJsonText);

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
      Assert.AreEqual(expected, arrayJsonText);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.")]
    public void MultipleRootPropertiesXmlDocument()
    {
      string json = @"{""count"": 773840,""photos"": null}";

      JsonConvert.DeserializeXmlNode(json);
    }

#if !NET20
    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.")]
    public void MultipleRootPropertiesXDocument()
    {
      string json = @"{""count"": 773840,""photos"": null}";

      JsonConvert.DeserializeXNode(json);
    }
#endif

    [Test]
    public void MultipleRootPropertiesAddRootElement()
    {
      string json = @"{""count"": 773840,""photos"": 773840}";

      XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newDoc.InnerXml);

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
    ]
  ]
}";

      XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);

#if !NET20
      XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
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

      XmlDocument newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);

#if !NET20
      XDocument newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
#endif
    }

    [Test]
    public void Encoding()
    {
      XmlDocument doc = new XmlDocument();

      doc.LoadXml(@"<name>O""Connor</name>"); // i use "" so it will be easier to see the  problem

      string json = SerializeXmlNode(doc);
      Assert.AreEqual(@"{
  ""name"": ""O\""Connor""
}", json);
    }

    [Test]
    public void SerializeComment()
    {
      string xml = @"<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">Text</a><!-- Hi! -->
</span>";
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(xml);

      string jsonText = SerializeXmlNode(doc);

      string expected = @"{
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""#text"": ""Text""
    }/* Hi! */
  }
}";

      Assert.AreEqual(expected, jsonText);

      XmlDocument newDoc = (XmlDocument)DeserializeXmlNode(jsonText);
      Assert.AreEqual(@"<span class=""vevent""><a class=""url"" href=""http://www.web2con.com/"">Text</a><!-- Hi! --></span>", newDoc.InnerXml);
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

      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"<?xml version=""1.0"" standalone=""no""?>
<root>
<person id=""1"">
<name>Alan</name>
<url>http://www.google.com</url>
</person>
<person id=""2"">
<name>Louis</name>
<url>http://www.yahoo.com</url>
</person>
</root>".Replace(Environment.NewLine, string.Empty), doc.InnerXml);
    }

    [Test]
    public void SerializeDeserializeSpecialProperties()
    {
      PreserveReferencesHandlingTests.CircularDictionary circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
      circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
      circularDictionary.Add("self", circularDictionary);

      string json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
        new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

      Assert.AreEqual(@"{
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

      Assert.AreEqual(expected, xml);

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

      Assert.AreEqual(expectedXmlJson, xmlJson);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "XmlNodeConverter cannot convert JSON with an empty property name to XML.")]
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

      DeserializeXmlNode(json);
    }
  }
}
#endif