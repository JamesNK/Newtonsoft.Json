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
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Converters
{
  public class XmlNodeConverterTest : TestFixtureBase
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

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
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

      jsonText = JsonConvert.SerializeXmlNode(attribute);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);


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
      XmlElement element = doc.CreateElement("xs:Choice");
      element.SetAttributeNode(attribute);

      element.AppendChild(instruction);
      element.AppendChild(cDataSection);

      jsonText = JsonConvert.SerializeXmlNode(element);

      Console.WriteLine(jsonText);
      Assert.AreEqual(@"{""xs:Choice"":{""@msdata:IsDataSet"":""true"",""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\"""",""#cdata-section"":""<Kiwi>true</Kiwi>""}}", jsonText);
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

      string jsonText = JsonConvert.SerializeXmlNode(doc);

      XmlDocument deserializedDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

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

      XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

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

      string jsonText = JsonConvert.SerializeXmlNode(doc);

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

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

      string jsonText = JsonConvert.SerializeXmlNode(doc);

      Console.WriteLine(jsonText);

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

      Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
    }

    [Test]
    public void OtherElementDataTypes()
    {
      string jsonText = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""Float"":2.5,""Integer"":99},{""@id"":""2"",""Boolean"":true,""date"":""\/Date(954374400000)\/""}]}}";

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

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

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonText);

      string expected = @"<root><r><-Date>34343</-Date><-Date>55</-Date></r></root>";

      Assert.AreEqual(expected, newDoc.InnerXml);

      string json = JsonConvert.SerializeXmlNode(newDoc);
      Assert.AreEqual(@"{""root"":{""r"":new Date(""34343"",""55"")}}", json);
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

      string arrayJsonText = JsonConvert.SerializeXmlNode(arrayDoc);
      Assert.AreEqual(@"{""root"":{""person"":{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com"",""role"":[""Admin""]}}}", arrayJsonText);

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

      arrayJsonText = JsonConvert.SerializeXmlNode(arrayDoc);
      Assert.AreEqual(@"{""root"":{""person"":{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com"",""role"":[""Admin1"",""Admin2""]}}}", arrayJsonText);

      arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""false"">Admin1</role>
			  </person>
			</root>";

      arrayDoc = new XmlDocument();
      arrayDoc.LoadXml(arrayXml);

      arrayJsonText = JsonConvert.SerializeXmlNode(arrayDoc);
      Assert.AreEqual(@"{""root"":{""person"":{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com"",""role"":""Admin1""}}}", arrayJsonText);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifing a DeserializeRootElementName.")]
    public void MultipleRootProperties()
    {
      string strJSON = @"{
""count"": 773840,""photos"": [
{
""photo_id"": 532693,
""photo_title"": ""Wheatfield in afternoon light"",
""photo_url"": ""http://www.panoramio.com/photo/532693"",
""photofileurl"": ""http://static2.bareka.com/photos/medium/532693.jpg"",
""longitude"": 11.280727,
""latitude"": 59.643198,
""width"": 500,
""height"": 333,
""upload_date"": ""22 January 2007"",
""owner_id"": 39160,
""owner_name"": ""Snemann"",
""owner_url"": ""http://www.panoramio.com/user/39160"",
},
{
""photo_id"": 505229,
""photo_title"": ""Etangs près de Dijon"",
""photo_url"": ""http://www.panoramio.com/photo/505229"",
""photofileurl"": ""http://static2.bareka.com/photos/medium/505229.jpg"",
""longitude"": 5.168552,
""latitude"": 47.312642,
""width"": 350,
""height"": 500,
""upload_date"": ""20 January 2007"",
""owner_id"": 78506,
""owner_name"": ""Philippe Stoop"",
""owner_url"": ""http://www.panoramio.com/user/78506""
}
]
}";

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(strJSON);
    }

    [Test]
    public void MultipleRootPropertiesAddRootElement()
    {
      string strJSON = @"{""count"": 773840,""photos"": 773840}";

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(strJSON, "myRoot");

      Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newDoc.InnerXml);
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

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);
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

      XmlDocument newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(json, "myRoot");

      Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);
    }
  }
}
#endif