using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Text;

namespace Newtonsoft.Json.Serialization
{
    internal class TraceJsonWriter : JsonWriter
    {
        private readonly JsonWriter _innerWriter;
        private readonly JsonTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceJsonWriter(JsonWriter innerWriter)
        {
            _innerWriter = innerWriter;

            _sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
            _sw.Write("Serialized JSON: " + Environment.NewLine);

            _textWriter = new JsonTextWriter(_sw);
            _textWriter.Formatting = Formatting.Indented;
            _textWriter.Culture = innerWriter.Culture;
            _textWriter.DateFormatHandling = innerWriter.DateFormatHandling;
            _textWriter.DateFormatString = innerWriter.DateFormatString;
            _textWriter.DateTimeZoneHandling = innerWriter.DateTimeZoneHandling;
            _textWriter.FloatFormatHandling = innerWriter.FloatFormatHandling;
        }

        public string GetSerializedJsonMessage()
        {
            return _sw.ToString();
        }

        public override void WriteValue(decimal value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(bool value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(byte value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(char value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(byte[] value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            if (value == null)
            {
                base.WriteUndefined();
            }
            else
            {
                base.WriteValue(value);
            }
        }

        public override void WriteValue(DateTime value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

#if !NET20
        public override void WriteValue(DateTimeOffset value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }
#endif

        public override void WriteValue(double value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteUndefined()
        {
            _textWriter.WriteUndefined();
            _innerWriter.WriteUndefined();
            base.WriteUndefined();
        }

        public override void WriteNull()
        {
            _textWriter.WriteNull();
            _innerWriter.WriteNull();
            base.WriteUndefined();
        }

        public override void WriteValue(float value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(Guid value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(object value)
        {
#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_1
            if (value is BigInteger)
            {
                _textWriter.WriteValue(value);
                _innerWriter.WriteValue(value);
                InternalWriteValue(JsonToken.Integer);
            }
            else
#endif
            {
                _textWriter.WriteValue(value);
                _innerWriter.WriteValue(value);
                if (value == null)
                {
                    base.WriteUndefined();
                }
                else
                {
                    base.WriteValue(value);
                }
            }
        }

        public override void WriteValue(sbyte value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(short value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(string value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(uint value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(ulong value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteValue(Uri value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            if (value == null)
            {
                base.WriteUndefined();
            }
            else
            {
                base.WriteValue(value);
            }
        }

        public override void WriteValue(ushort value)
        {
            _textWriter.WriteValue(value);
            _innerWriter.WriteValue(value);
            base.WriteValue(value);
        }

        public override void WriteWhitespace(string ws)
        {
            _textWriter.WriteWhitespace(ws);
            _innerWriter.WriteWhitespace(ws);
            base.WriteWhitespace(ws);
        }

        public override void WriteComment(string text)
        {
            _textWriter.WriteComment(text);
            _innerWriter.WriteComment(text);
            base.WriteComment(text);
        }

        public override void WriteStartArray()
        {
            _textWriter.WriteStartArray();
            _innerWriter.WriteStartArray();
            base.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            _textWriter.WriteEndArray();
            _innerWriter.WriteEndArray();
            base.WriteEndArray();
        }

        public override void WriteStartConstructor(string name)
        {
            _textWriter.WriteStartConstructor(name);
            _innerWriter.WriteStartConstructor(name);
            base.WriteStartConstructor(name);
        }

        public override void WriteEndConstructor()
        {
            _textWriter.WriteEndConstructor();
            _innerWriter.WriteEndConstructor();
            base.WriteEndConstructor();
        }

        public override void WritePropertyName(string name)
        {
            _textWriter.WritePropertyName(name);
            _innerWriter.WritePropertyName(name);
            base.WritePropertyName(name);
        }

        public override void WritePropertyName(string name, bool escape)
        {
            _textWriter.WritePropertyName(name, escape);
            _innerWriter.WritePropertyName(name, escape);

            // method with escape will error
            base.WritePropertyName(name);
        }

        public override void WriteStartObject()
        {
            _textWriter.WriteStartObject();
            _innerWriter.WriteStartObject();
            base.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            _textWriter.WriteEndObject();
            _innerWriter.WriteEndObject();
            base.WriteEndObject();
        }

        public override void WriteRawValue(string json)
        {
            _textWriter.WriteRawValue(json);
            _innerWriter.WriteRawValue(json);

            // calling base method will write json twice
            InternalWriteValue(JsonToken.Undefined);
        }

        public override void WriteRaw(string json)
        {
            _textWriter.WriteRaw(json);
            _innerWriter.WriteRaw(json);
            base.WriteRaw(json);
        }

        public override void Close()
        {
            _textWriter.Close();
            _innerWriter.Close();
            base.Close();
        }

        public override void Flush()
        {
            _textWriter.Flush();
            _innerWriter.Flush();
        }
    }
}