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
using System.Globalization;
using System.IO;
using System.Text;

namespace Newtonsoft.Json.Serialization
{
    internal class TraceJsonReader : JsonReader, IJsonLineInfo
    {
        private readonly JsonReader _innerReader;
        private readonly JsonTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceJsonReader(JsonReader innerReader)
        {
            _innerReader = innerReader;

            _sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large JSON string
            _sw.Write("Deserialized JSON: " + Environment.NewLine);

            _textWriter = new JsonTextWriter(_sw);
            _textWriter.Formatting = Formatting.Indented;
        }

        public string GetDeserializedJsonMessage()
        {
            return _sw.ToString();
        }

        public override bool Read()
        {
            bool value = _innerReader.Read();
            WriteCurrentToken();
            return value;
        }

        public override int? ReadAsInt32()
        {
            int? value = _innerReader.ReadAsInt32();
            WriteCurrentToken();
            return value;
        }

        public override string ReadAsString()
        {
            string value = _innerReader.ReadAsString();
            WriteCurrentToken();
            return value;
        }

        public override byte[] ReadAsBytes()
        {
            byte[] value = _innerReader.ReadAsBytes();
            WriteCurrentToken();
            return value;
        }

        public override decimal? ReadAsDecimal()
        {
            decimal? value = _innerReader.ReadAsDecimal();
            WriteCurrentToken();
            return value;
        }

        public override double? ReadAsDouble()
        {
            double? value = _innerReader.ReadAsDouble();
            WriteCurrentToken();
            return value;
        }

        public override bool? ReadAsBoolean()
        {
            bool? value = _innerReader.ReadAsBoolean();
            WriteCurrentToken();
            return value;
        }

        public override DateTime? ReadAsDateTime()
        {
            DateTime? value = _innerReader.ReadAsDateTime();
            WriteCurrentToken();
            return value;
        }

#if HAVE_DATE_TIME_OFFSET
        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            DateTimeOffset? value = _innerReader.ReadAsDateTimeOffset();
            WriteCurrentToken();
            return value;
        }
#endif

        public void WriteCurrentToken()
        {
            _textWriter.WriteToken(_innerReader, false, false, true);
        }

        public override int Depth => _innerReader.Depth;

        public override string Path => _innerReader.Path;

        public override char QuoteChar
        {
            get => _innerReader.QuoteChar;
            protected internal set => _innerReader.QuoteChar = value;
        }

        public override JsonToken TokenType => _innerReader.TokenType;

        public override object Value => _innerReader.Value;

        public override Type ValueType => _innerReader.ValueType;

        public override void Close()
        {
            _innerReader.Close();
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            return _innerReader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber => (_innerReader is IJsonLineInfo lineInfo) ? lineInfo.LineNumber : 0;

        int IJsonLineInfo.LinePosition => (_innerReader is IJsonLineInfo lineInfo) ? lineInfo.LinePosition : 0;
    }
}