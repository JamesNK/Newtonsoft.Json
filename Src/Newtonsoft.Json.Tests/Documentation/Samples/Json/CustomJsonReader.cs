using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Json
{
    public class XmlJsonReader : JsonReader
    {
        private readonly XmlReader _reader;

        public XmlJsonReader(XmlReader reader)
        {
            _reader = reader;
        }

        public override bool Read()
        {
            if (!_reader.Read())
                return false;

            switch (_reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (CurrentState == State.ObjectStart || CurrentState == State.Object)
                    {
                        SetToken(JsonToken.PropertyName, _reader.LocalName);
                    }
                    else
                    {
                        SetToken(JsonToken.StartObject);
                    }
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    SetToken(JsonToken.String, _reader.Value);
                    break;
                case XmlNodeType.Comment:
                     SetToken(JsonToken.Comment, _reader.Value);
                   break;
                case XmlNodeType.Document:
                    break;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    break;
                case XmlNodeType.EndElement:
                    break;
                case XmlNodeType.EndEntity:
                    break;
                case XmlNodeType.XmlDeclaration:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        public override int? ReadAsInt32()
        {
            throw new NotImplementedException();
        }

        public override string ReadAsString()
        {
            throw new NotImplementedException();
        }

        public override byte[] ReadAsBytes()
        {
            throw new NotImplementedException();
        }

        public override decimal? ReadAsDecimal()
        {
            throw new NotImplementedException();
        }

        public override DateTime? ReadAsDateTime()
        {
            throw new NotImplementedException();
        }

        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            throw new NotImplementedException();
        }
    }

    public class CustomJsonReader
    {
        public void Example()
        {
            #region Usage
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("CPU");
                writer.WriteValue("Intel");
                writer.WritePropertyName("PSU");
                writer.WriteValue("500W");
                writer.WritePropertyName("Drives");
                writer.WriteStartArray();
                writer.WriteValue("DVD read/writer");
                writer.WriteComment("(broken)");
                writer.WriteValue("500 gigabyte hard drive");
                writer.WriteValue("200 gigabype hard drive");
                writer.WriteEnd();
                writer.WriteEndObject();
            }

            Console.WriteLine(sb.ToString());
            // {
            //   "CPU": "Intel",
            //   "PSU": "500W",
            //   "Drives": [
            //     "DVD read/writer"
            //     /*(broken)*/,
            //     "500 gigabyte hard drive",
            //     "200 gigabype hard drive"
            //   ]
            // }
            #endregion
        }
    }
}