using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Json
{
    #region Types
    public class XmlJsonWriter : JsonWriter
    {
        private readonly XmlWriter _writer;
        private string _propertyName;

        public XmlJsonWriter(XmlWriter writer)
        {
            _writer = writer;
        }

        public override void WriteComment(string text)
        {
            base.WriteComment(text);
            _writer.WriteComment(text);
        }

        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);
            _propertyName = name;
        }

        public override void WriteNull()
        {
            base.WriteNull();

            WriteValueElement(JTokenType.Null);
            _writer.WriteEndElement();
        }

        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Date);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Date);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Guid);
            _writer.WriteValue(value.ToString());
            _writer.WriteEndElement();
        }

        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.TimeSpan);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Uri);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(string value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.String);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(int value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Integer);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(long value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Integer);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(short value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Integer);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(byte value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Integer);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(bool value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Boolean);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(char value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.String);
            _writer.WriteValue(value.ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Float);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(double value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Float);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        public override void WriteValue(float value)
        {
            base.WriteValue(value);

            WriteValueElement(JTokenType.Float);
            _writer.WriteValue(value);
            _writer.WriteEndElement();
        }

        private void WriteValueElement(JTokenType type)
        {
            if (_propertyName != null)
            {
                WriteValueElement(_propertyName, type);
                _propertyName = null;
            }
            else
            {
                WriteValueElement("Item", type);
            }
        }

        private void WriteValueElement(string elementName, JTokenType type)
        {
            _writer.WriteStartElement(elementName);
            _writer.WriteAttributeString("type", type.ToString());
        }

        public override void WriteStartArray()
        {
            bool isStart = (WriteState == WriteState.Start);

            base.WriteStartArray();

            if (isStart)
                WriteValueElement("Root", JTokenType.Array);
            else
                WriteValueElement(JTokenType.Array);
        }

        public override void WriteStartObject()
        {
            bool isStart = (WriteState == WriteState.Start);

            base.WriteStartObject();

            if (isStart)
                WriteValueElement("Root", JTokenType.Object);
            else
                WriteValueElement(JTokenType.Object);
        }

        public override void WriteStartConstructor(string name)
        {
            bool isStart = (WriteState == WriteState.Start);

            base.WriteStartConstructor(name);

            if (isStart)
                WriteValueElement("Root", JTokenType.Constructor);
            else
                WriteValueElement(JTokenType.Constructor);

            _writer.WriteAttributeString("name", name);
        }

        public override void WriteEndArray()
        {
            base.WriteEndArray();
            _writer.WriteEndElement();
        }

        public override void WriteEndObject()
        {
            base.WriteEndObject();
            _writer.WriteEndElement();
        }

        public override void WriteEndConstructor()
        {
            base.WriteEndConstructor();
            _writer.WriteEndElement();
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        protected override void WriteIndent()
        {
            _writer.WriteWhitespace(Environment.NewLine);

            // levels of indentation multiplied by the indent count
            int currentIndentCount = Top * 2;

            while (currentIndentCount > 0)
            {
                // write up to a max of 10 characters at once to avoid creating too many new strings
                int writeCount = Math.Min(currentIndentCount, 10);

                _writer.WriteWhitespace(new string(' ', writeCount));

                currentIndentCount -= writeCount;
            }
        }
    }
    #endregion

    public class CustomJsonWriter
    {
        public void Example()
        {
            #region Usage
            var user = new
            {
                Name = "James",
                Age = 30,
                Enabled = true,
                Roles = new[]
                    {
                        "Publisher",
                        "Administrator"
                    }
            };

            StringWriter sw = new StringWriter();

            using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
            using (XmlJsonWriter writer = new XmlJsonWriter(xmlWriter))
            {
                writer.Formatting = Formatting.Indented;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, user);
            }

            Console.WriteLine(sw.ToString());
            //<Root type="Object">
            //  <Name type="String">James</Name>
            //  <Age type="Integer">30</Age>
            //  <Enabled type="Boolean">true</Enabled>
            //  <Roles type="Array">
            //    <Item type="String">Publisher</Item>
            //    <Item type="String">Administrator</Item>
            //  </Roles>
            //</Root>
            #endregion

            sw = new StringWriter();

            using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
            using (XmlJsonWriter writer = new XmlJsonWriter(xmlWriter))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                writer.WritePropertyName("Null");
                writer.WriteNull();

                writer.WritePropertyName("String");
                writer.WriteValue("This is a string!");

                writer.WritePropertyName("Char");
                writer.WriteValue('!');

                writer.WritePropertyName("Integer");
                writer.WriteValue(123);

                writer.WritePropertyName("DateTime");
                writer.WriteValue(new DateTime(2001, 2, 22, 20, 59, 59, DateTimeKind.Utc));

                writer.WritePropertyName("DateTimeOffset");
                writer.WriteValue(new DateTimeOffset(2001, 2, 22, 20, 59, 59, TimeSpan.FromHours(12)));

                writer.WritePropertyName("Float");
                writer.WriteValue(1.1f);

                writer.WritePropertyName("Double");
                writer.WriteValue(3.14d);

                writer.WritePropertyName("Decimal");
                writer.WriteValue(19.95m);

                writer.WritePropertyName("Guid");
                writer.WriteValue(Guid.NewGuid());

                writer.WritePropertyName("Uri");
                writer.WriteValue(new Uri("http://james.newtonking.com"));

                writer.WritePropertyName("Array");
                writer.WriteStartArray();
                writer.WriteValue(1);
                writer.WriteValue(2);
                writer.WriteValue(3);
                writer.WriteEndArray();

                writer.WritePropertyName("Object");
                writer.WriteStartObject();
                writer.WritePropertyName("String");
                writer.WriteValue("This is a string!");
                writer.WritePropertyName("Null");
                writer.WriteNull();
                writer.WriteEndObject();

                writer.WritePropertyName("Constructor");
                writer.WriteStartConstructor("Date");
                writer.WriteValue(2000);
                writer.WriteValue(12);
                writer.WriteValue(30);
                writer.WriteEndConstructor();

                writer.WriteEndObject();

                writer.Flush();
            }

            Console.WriteLine(sw.ToString());

            //<Root type="Object">
            //  <Null type="Null" />
            //  <String type="String">This is a string!</String>
            //  <Char type="String">!</Char>
            //  <Integer type="Integer">123</Integer>
            //  <DateTime type="Date">2001-02-22T20:59:59Z</DateTime>
            //  <DateTimeOffset type="Date">2001-02-22T20:59:59+12:00</DateTimeOffset>
            //  <Float type="Float">1.1</Float>
            //  <Double type="Float">3.14</Double>
            //  <Decimal type="Float">19.95</Decimal>
            //  <Guid type="Guid">d66eab59-3715-4b35-9e06-fa61c1216eaa</Guid>
            //  <Uri type="Uri">http://james.newtonking.com</Uri>
            //  <Array type="Array">
            //    <Item type="Integer">1</Item>
            //    <Item type="Integer">2</Item>
            //    <Item type="Integer">3</Item>
            //  </Array>
            //  <Object type="Object">
            //    <String type="String">This is a string!</String>
            //    <Null type="Null" />
            //  </Object>
            //  <Constructor type="Constructor" name="Date">
            //    <Item type="Integer">2000</Item>
            //    <Item type="Integer">12</Item>
            //    <Item type="Integer">30</Item>
            //  </Constructor>
            //</Root>
        }
    }
}