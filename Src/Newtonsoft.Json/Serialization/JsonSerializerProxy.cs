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
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization
{
    internal class JsonSerializerProxy : JsonSerializer
    {
        private readonly JsonSerializerInternalReader _serializerReader;
        private readonly JsonSerializerInternalWriter _serializerWriter;
        private readonly JsonSerializer _serializer;

        public override event EventHandler<ErrorEventArgs> Error
        {
            add { _serializer.Error += value; }
            remove { _serializer.Error -= value; }
        }

        public override IReferenceResolver ReferenceResolver
        {
            get { return _serializer.ReferenceResolver; }
            set { _serializer.ReferenceResolver = value; }
        }

        public override ITraceWriter TraceWriter
        {
            get { return _serializer.TraceWriter; }
            set { _serializer.TraceWriter = value; }
        }

        public override IEqualityComparer EqualityComparer
        {
            get { return _serializer.EqualityComparer; }
            set { _serializer.EqualityComparer = value; }
        }

        public override JsonConverterCollection Converters
        {
            get { return _serializer.Converters; }
        }

        public override DefaultValueHandling DefaultValueHandling
        {
            get { return _serializer.DefaultValueHandling; }
            set { _serializer.DefaultValueHandling = value; }
        }

        public override IContractResolver ContractResolver
        {
            get { return _serializer.ContractResolver; }
            set { _serializer.ContractResolver = value; }
        }

        public override MissingMemberHandling MissingMemberHandling
        {
            get { return _serializer.MissingMemberHandling; }
            set { _serializer.MissingMemberHandling = value; }
        }

        public override NullValueHandling NullValueHandling
        {
            get { return _serializer.NullValueHandling; }
            set { _serializer.NullValueHandling = value; }
        }

        public override ObjectCreationHandling ObjectCreationHandling
        {
            get { return _serializer.ObjectCreationHandling; }
            set { _serializer.ObjectCreationHandling = value; }
        }

        public override ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _serializer.ReferenceLoopHandling; }
            set { _serializer.ReferenceLoopHandling = value; }
        }

        public override PreserveReferencesHandling PreserveReferencesHandling
        {
            get { return _serializer.PreserveReferencesHandling; }
            set { _serializer.PreserveReferencesHandling = value; }
        }

        public override TypeNameHandling TypeNameHandling
        {
            get { return _serializer.TypeNameHandling; }
            set { _serializer.TypeNameHandling = value; }
        }

        public override MetadataPropertyHandling MetadataPropertyHandling
        {
            get { return _serializer.MetadataPropertyHandling; }
            set { _serializer.MetadataPropertyHandling = value; }
        }

        public override FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get { return _serializer.TypeNameAssemblyFormat; }
            set { _serializer.TypeNameAssemblyFormat = value; }
        }

        public override ConstructorHandling ConstructorHandling
        {
            get { return _serializer.ConstructorHandling; }
            set { _serializer.ConstructorHandling = value; }
        }

        public override SerializationBinder Binder
        {
            get { return _serializer.Binder; }
            set { _serializer.Binder = value; }
        }

        public override StreamingContext Context
        {
            get { return _serializer.Context; }
            set { _serializer.Context = value; }
        }

        public override Formatting Formatting
        {
            get { return _serializer.Formatting; }
            set { _serializer.Formatting = value; }
        }

        public override DateFormatHandling DateFormatHandling
        {
            get { return _serializer.DateFormatHandling; }
            set { _serializer.DateFormatHandling = value; }
        }

        public override DateTimeZoneHandling DateTimeZoneHandling
        {
            get { return _serializer.DateTimeZoneHandling; }
            set { _serializer.DateTimeZoneHandling = value; }
        }

        public override DateParseHandling DateParseHandling
        {
            get { return _serializer.DateParseHandling; }
            set { _serializer.DateParseHandling = value; }
        }

        public override FloatFormatHandling FloatFormatHandling
        {
            get { return _serializer.FloatFormatHandling; }
            set { _serializer.FloatFormatHandling = value; }
        }

        public override FloatParseHandling FloatParseHandling
        {
            get { return _serializer.FloatParseHandling; }
            set { _serializer.FloatParseHandling = value; }
        }

        public override StringEscapeHandling StringEscapeHandling
        {
            get { return _serializer.StringEscapeHandling; }
            set { _serializer.StringEscapeHandling = value; }
        }

        public override string DateFormatString
        {
            get { return _serializer.DateFormatString; }
            set { _serializer.DateFormatString = value; }
        }

        public override CultureInfo Culture
        {
            get { return _serializer.Culture; }
            set { _serializer.Culture = value; }
        }

        public override int? MaxDepth
        {
            get { return _serializer.MaxDepth; }
            set { _serializer.MaxDepth = value; }
        }

        public override bool CheckAdditionalContent
        {
            get { return _serializer.CheckAdditionalContent; }
            set { _serializer.CheckAdditionalContent = value; }
        }

        internal JsonSerializerInternalBase GetInternalSerializer()
        {
            if (_serializerReader != null)
            {
                return _serializerReader;
            }
            else
            {
                return _serializerWriter;
            }
        }

        public JsonSerializerProxy(JsonSerializerInternalReader serializerReader)
        {
            ValidationUtils.ArgumentNotNull(serializerReader, nameof(serializerReader));

            _serializerReader = serializerReader;
            _serializer = serializerReader.Serializer;
        }

        public JsonSerializerProxy(JsonSerializerInternalWriter serializerWriter)
        {
            ValidationUtils.ArgumentNotNull(serializerWriter, nameof(serializerWriter));

            _serializerWriter = serializerWriter;
            _serializer = serializerWriter.Serializer;
        }

        internal override object DeserializeInternal(JsonReader reader, Type objectType)
        {
            if (_serializerReader != null)
            {
                return _serializerReader.Deserialize(reader, objectType, false);
            }
            else
            {
                return _serializer.Deserialize(reader, objectType);
            }
        }

        internal override void PopulateInternal(JsonReader reader, object target)
        {
            if (_serializerReader != null)
            {
                _serializerReader.Populate(reader, target);
            }
            else
            {
                _serializer.Populate(reader, target);
            }
        }

        internal override void SerializeInternal(JsonWriter jsonWriter, object value, Type rootType)
        {
            if (_serializerWriter != null)
            {
                _serializerWriter.Serialize(jsonWriter, value, rootType);
            }
            else
            {
                _serializer.Serialize(jsonWriter, value);
            }
        }
    }
}