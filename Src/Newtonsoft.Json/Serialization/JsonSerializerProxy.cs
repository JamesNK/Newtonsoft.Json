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
            add => _serializer.Error += value;
            remove => _serializer.Error -= value;
        }

        public override IReferenceResolver ReferenceResolver
        {
            get => _serializer.ReferenceResolver;
            set => _serializer.ReferenceResolver = value;
        }

        public override ITraceWriter TraceWriter
        {
            get => _serializer.TraceWriter;
            set => _serializer.TraceWriter = value;
        }

        public override IEqualityComparer EqualityComparer
        {
            get => _serializer.EqualityComparer;
            set => _serializer.EqualityComparer = value;
        }

        public override JsonConverterCollection Converters => _serializer.Converters;

        public override DefaultValueHandling DefaultValueHandling
        {
            get => _serializer.DefaultValueHandling;
            set => _serializer.DefaultValueHandling = value;
        }

        public override IContractResolver ContractResolver
        {
            get => _serializer.ContractResolver;
            set => _serializer.ContractResolver = value;
        }

        public override MissingMemberHandling MissingMemberHandling
        {
            get => _serializer.MissingMemberHandling;
            set => _serializer.MissingMemberHandling = value;
        }

        public override NullValueHandling NullValueHandling
        {
            get => _serializer.NullValueHandling;
            set => _serializer.NullValueHandling = value;
        }

        public override ObjectCreationHandling ObjectCreationHandling
        {
            get => _serializer.ObjectCreationHandling;
            set => _serializer.ObjectCreationHandling = value;
        }

        public override ReferenceLoopHandling ReferenceLoopHandling
        {
            get => _serializer.ReferenceLoopHandling;
            set => _serializer.ReferenceLoopHandling = value;
        }

        public override PreserveReferencesHandling PreserveReferencesHandling
        {
            get => _serializer.PreserveReferencesHandling;
            set => _serializer.PreserveReferencesHandling = value;
        }

        public override TypeNameHandling TypeNameHandling
        {
            get => _serializer.TypeNameHandling;
            set => _serializer.TypeNameHandling = value;
        }

        public override MetadataPropertyHandling MetadataPropertyHandling
        {
            get => _serializer.MetadataPropertyHandling;
            set => _serializer.MetadataPropertyHandling = value;
        }

        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public override FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get => _serializer.TypeNameAssemblyFormat;
            set => _serializer.TypeNameAssemblyFormat = value;
        }

        public override TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get => _serializer.TypeNameAssemblyFormatHandling;
            set => _serializer.TypeNameAssemblyFormatHandling = value;
        }

        public override ConstructorHandling ConstructorHandling
        {
            get => _serializer.ConstructorHandling;
            set => _serializer.ConstructorHandling = value;
        }

        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public override SerializationBinder Binder
        {
            get => _serializer.Binder;
            set => _serializer.Binder = value;
        }

        public override ISerializationBinder SerializationBinder
        {
            get => _serializer.SerializationBinder;
            set => _serializer.SerializationBinder = value;
        }

        public override StreamingContext Context
        {
            get => _serializer.Context;
            set => _serializer.Context = value;
        }

        public override Formatting Formatting
        {
            get => _serializer.Formatting;
            set => _serializer.Formatting = value;
        }

        public override DateFormatHandling DateFormatHandling
        {
            get => _serializer.DateFormatHandling;
            set => _serializer.DateFormatHandling = value;
        }

        public override DateTimeZoneHandling DateTimeZoneHandling
        {
            get => _serializer.DateTimeZoneHandling;
            set => _serializer.DateTimeZoneHandling = value;
        }

        public override DateParseHandling DateParseHandling
        {
            get => _serializer.DateParseHandling;
            set => _serializer.DateParseHandling = value;
        }

        public override FloatFormatHandling FloatFormatHandling
        {
            get => _serializer.FloatFormatHandling;
            set => _serializer.FloatFormatHandling = value;
        }

        public override FloatParseHandling FloatParseHandling
        {
            get => _serializer.FloatParseHandling;
            set => _serializer.FloatParseHandling = value;
        }

        public override StringEscapeHandling StringEscapeHandling
        {
            get => _serializer.StringEscapeHandling;
            set => _serializer.StringEscapeHandling = value;
        }

        public override string DateFormatString
        {
            get => _serializer.DateFormatString;
            set => _serializer.DateFormatString = value;
        }

        public override CultureInfo Culture
        {
            get => _serializer.Culture;
            set => _serializer.Culture = value;
        }

        public override int? MaxDepth
        {
            get => _serializer.MaxDepth;
            set => _serializer.MaxDepth = value;
        }

        public override bool CheckAdditionalContent
        {
            get => _serializer.CheckAdditionalContent;
            set => _serializer.CheckAdditionalContent = value;
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