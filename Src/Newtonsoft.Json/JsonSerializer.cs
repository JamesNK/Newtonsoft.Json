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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Serializes and deserializes objects into and from the JSON format.
    /// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into JSON.
    /// </summary>
    public class JsonSerializer
    {
        internal TypeNameHandling _typeNameHandling;
        internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;
        internal PreserveReferencesHandling _preserveReferencesHandling;
        internal ReferenceLoopHandling _referenceLoopHandling;
        internal MissingMemberHandling _missingMemberHandling;
        internal ObjectCreationHandling _objectCreationHandling;
        internal NullValueHandling _nullValueHandling;
        internal DefaultValueHandling _defaultValueHandling;
        internal ConstructorHandling _constructorHandling;
        internal MetadataPropertyHandling _metadataPropertyHandling;
        internal JsonConverterCollection _converters;
        internal IContractResolver _contractResolver;
        internal ITraceWriter _traceWriter;
        internal IEqualityComparer _equalityComparer;
        internal ISerializationBinder _serializationBinder;
        internal StreamingContext _context;
        private IReferenceResolver _referenceResolver;

        private Formatting? _formatting;
        private DateFormatHandling? _dateFormatHandling;
        private DateTimeZoneHandling? _dateTimeZoneHandling;
        private DateParseHandling? _dateParseHandling;
        private FloatFormatHandling? _floatFormatHandling;
        private FloatParseHandling? _floatParseHandling;
        private StringEscapeHandling? _stringEscapeHandling;
        private CultureInfo _culture;
        private int? _maxDepth;
        private bool _maxDepthSet;
        private bool? _checkAdditionalContent;
        private string _dateFormatString;
        private bool _dateFormatStringSet;

        /// <summary>
        /// Occurs when the <see cref="JsonSerializer"/> errors during serialization and deserialization.
        /// </summary>
        public virtual event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Gets or sets the <see cref="IReferenceResolver"/> used by the serializer when resolving references.
        /// </summary>
        public virtual IReferenceResolver ReferenceResolver
        {
            get { return GetReferenceResolver(); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Reference resolver cannot be null.");
                }

                _referenceResolver = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public virtual SerializationBinder Binder
        {
            get
            {
                if (_serializationBinder == null)
                {
                    return null;
                }

                SerializationBinderAdapter adapter = _serializationBinder as SerializationBinderAdapter;
                if (adapter != null)
                {
                    return adapter.SerializationBinder;
                }

                throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                _serializationBinder = new SerializationBinderAdapter(value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        public virtual ISerializationBinder SerializationBinder
        {
            get { return _serializationBinder; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                _serializationBinder = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
        /// </summary>
        /// <value>The trace writer.</value>
        public virtual ITraceWriter TraceWriter
        {
            get { return _traceWriter; }
            set { _traceWriter = value; }
        }

        /// <summary>
        /// Gets or sets the equality comparer used by the serializer when comparing references.
        /// </summary>
        /// <value>The equality comparer.</value>
        public virtual IEqualityComparer EqualityComparer
        {
            get { return _equalityComparer; }
            set { _equalityComparer = value; }
        }

        /// <summary>
        /// Gets or sets how type name writing and reading is handled by the serializer.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonSerializer.TypeNameHandling"/> should be used with caution when your application deserializes JSON from an external source.
        /// Incoming types should be validated with a custom <see cref="JsonSerializer.SerializationBinder"/>
        /// when deserializing with a value other than <see cref="TypeNameHandling.None"/>.
        /// </remarks>
        public virtual TypeNameHandling TypeNameHandling
        {
            get { return _typeNameHandling; }
            set
            {
                if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get { return (FormatterAssemblyStyle)_typeNameAssemblyFormatHandling; }
            set
            {
                if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get { return _typeNameAssemblyFormatHandling; }
            set
            {
                if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how object references are preserved by the serializer.
        /// </summary>
        public virtual PreserveReferencesHandling PreserveReferencesHandling
        {
            get { return _preserveReferencesHandling; }
            set
            {
                if (value < PreserveReferencesHandling.None || value > PreserveReferencesHandling.All)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _preserveReferencesHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how reference loops (e.g. a class referencing itself) is handled.
        /// </summary>
        public virtual ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _referenceLoopHandling; }
            set
            {
                if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _referenceLoopHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
        /// </summary>
        public virtual MissingMemberHandling MissingMemberHandling
        {
            get { return _missingMemberHandling; }
            set
            {
                if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _missingMemberHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how null values are handled during serialization and deserialization.
        /// </summary>
        public virtual NullValueHandling NullValueHandling
        {
            get { return _nullValueHandling; }
            set
            {
                if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _nullValueHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how default values are handled during serialization and deserialization.
        /// </summary>
        public virtual DefaultValueHandling DefaultValueHandling
        {
            get { return _defaultValueHandling; }
            set
            {
                if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _defaultValueHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how objects are created during deserialization.
        /// </summary>
        /// <value>The object creation handling.</value>
        public virtual ObjectCreationHandling ObjectCreationHandling
        {
            get { return _objectCreationHandling; }
            set
            {
                if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _objectCreationHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how constructors are used during deserialization.
        /// </summary>
        /// <value>The constructor handling.</value>
        public virtual ConstructorHandling ConstructorHandling
        {
            get { return _constructorHandling; }
            set
            {
                if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _constructorHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how metadata properties are used during deserialization.
        /// </summary>
        /// <value>The metadata properties handling.</value>
        public virtual MetadataPropertyHandling MetadataPropertyHandling
        {
            get { return _metadataPropertyHandling; }
            set
            {
                if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _metadataPropertyHandling = value;
            }
        }

        /// <summary>
        /// Gets a collection <see cref="JsonConverter"/> that will be used during serialization.
        /// </summary>
        /// <value>Collection <see cref="JsonConverter"/> that will be used during serialization.</value>
        public virtual JsonConverterCollection Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = new JsonConverterCollection();
                }

                return _converters;
            }
        }

        /// <summary>
        /// Gets or sets the contract resolver used by the serializer when
        /// serializing .NET objects to JSON and vice versa.
        /// </summary>
        public virtual IContractResolver ContractResolver
        {
            get { return _contractResolver; }
            set { _contractResolver = value ?? DefaultContractResolver.Instance; }
        }

        /// <summary>
        /// Gets or sets the <see cref="StreamingContext"/> used by the serializer when invoking serialization callback methods.
        /// </summary>
        /// <value>The context.</value>
        public virtual StreamingContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// Indicates how JSON text output is formatted.
        /// </summary>
        public virtual Formatting Formatting
        {
            get { return _formatting ?? JsonSerializerSettings.DefaultFormatting; }
            set { _formatting = value; }
        }

        /// <summary>
        /// Gets or sets how dates are written to JSON text.
        /// </summary>
        public virtual DateFormatHandling DateFormatHandling
        {
            get { return _dateFormatHandling ?? JsonSerializerSettings.DefaultDateFormatHandling; }
            set { _dateFormatHandling = value; }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> time zones are handled during serialization and deserialization.
        /// </summary>
        public virtual DateTimeZoneHandling DateTimeZoneHandling
        {
            get { return _dateTimeZoneHandling ?? JsonSerializerSettings.DefaultDateTimeZoneHandling; }
            set { _dateTimeZoneHandling = value; }
        }

        /// <summary>
        /// Gets or sets how date formatted strings, e.g. <c>"\/Date(1198908717056)\/"</c> and <c>"2012-03-21T05:40Z"</c>, are parsed when reading JSON.
        /// </summary>
        public virtual DateParseHandling DateParseHandling
        {
            get { return _dateParseHandling ?? JsonSerializerSettings.DefaultDateParseHandling; }
            set { _dateParseHandling = value; }
        }

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public virtual FloatParseHandling FloatParseHandling
        {
            get { return _floatParseHandling ?? JsonSerializerSettings.DefaultFloatParseHandling; }
            set { _floatParseHandling = value; }
        }

        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written as JSON text.
        /// </summary>
        public virtual FloatFormatHandling FloatFormatHandling
        {
            get { return _floatFormatHandling ?? JsonSerializerSettings.DefaultFloatFormatHandling; }
            set { _floatFormatHandling = value; }
        }

        /// <summary>
        /// Gets or sets how strings are escaped when writing JSON text.
        /// </summary>
        public virtual StringEscapeHandling StringEscapeHandling
        {
            get { return _stringEscapeHandling ?? JsonSerializerSettings.DefaultStringEscapeHandling; }
            set { _stringEscapeHandling = value; }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing JSON text,
        /// and the expected date format when reading JSON text.
        /// </summary>
        public virtual string DateFormatString
        {
            get { return _dateFormatString ?? JsonSerializerSettings.DefaultDateFormatString; }
            set
            {
                _dateFormatString = value;
                _dateFormatStringSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public virtual CultureInfo Culture
        {
            get { return _culture ?? JsonSerializerSettings.DefaultCulture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="JsonReaderException"/>.
        /// </summary>
        public virtual int? MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                _maxDepth = value;
                _maxDepthSet = true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether there will be a check for additional JSON content after deserializing an object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if there will be a check for additional JSON content after deserializing an object; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CheckAdditionalContent
        {
            get { return _checkAdditionalContent ?? JsonSerializerSettings.DefaultCheckAdditionalContent; }
            set { _checkAdditionalContent = value; }
        }

        internal bool IsCheckAdditionalContentSet()
        {
            return (_checkAdditionalContent != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        public JsonSerializer()
        {
            _referenceLoopHandling = JsonSerializerSettings.DefaultReferenceLoopHandling;
            _missingMemberHandling = JsonSerializerSettings.DefaultMissingMemberHandling;
            _nullValueHandling = JsonSerializerSettings.DefaultNullValueHandling;
            _defaultValueHandling = JsonSerializerSettings.DefaultDefaultValueHandling;
            _objectCreationHandling = JsonSerializerSettings.DefaultObjectCreationHandling;
            _preserveReferencesHandling = JsonSerializerSettings.DefaultPreserveReferencesHandling;
            _constructorHandling = JsonSerializerSettings.DefaultConstructorHandling;
            _typeNameHandling = JsonSerializerSettings.DefaultTypeNameHandling;
            _metadataPropertyHandling = JsonSerializerSettings.DefaultMetadataPropertyHandling;
            _context = JsonSerializerSettings.DefaultContext;
            _serializationBinder = DefaultSerializationBinder.Instance;

            _culture = JsonSerializerSettings.DefaultCulture;
            _contractResolver = DefaultContractResolver.Instance;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will not use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will not use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer Create()
        {
            return new JsonSerializer();
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will not use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="JsonSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will not use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings)
        {
            JsonSerializer serializer = Create();

            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance.
        /// The <see cref="JsonSerializer"/> will use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/>.
        /// </returns>
        public static JsonSerializer CreateDefault()
        {
            // copy static to local variable to avoid concurrency issues
            JsonSerializerSettings defaultSettings = JsonConvert.DefaultSettings?.Invoke();

            return Create(defaultSettings);
        }

        /// <summary>
        /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/> as well as the specified <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="JsonSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
        /// The <see cref="JsonSerializer"/> will use default settings 
        /// from <see cref="JsonConvert.DefaultSettings"/> as well as the specified <see cref="JsonSerializerSettings"/>.
        /// </returns>
        public static JsonSerializer CreateDefault(JsonSerializerSettings settings)
        {
            JsonSerializer serializer = CreateDefault();
            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        private static void ApplySerializerSettings(JsonSerializer serializer, JsonSerializerSettings settings)
        {
            if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
            {
                // insert settings converters at the beginning so they take precedence
                // if user wants to remove one of the default converters they will have to do it manually
                for (int i = 0; i < settings.Converters.Count; i++)
                {
                    serializer.Converters.Insert(i, settings.Converters[i]);
                }
            }

            // serializer specific
            if (settings._typeNameHandling != null)
            {
                serializer.TypeNameHandling = settings.TypeNameHandling;
            }
            if (settings._metadataPropertyHandling != null)
            {
                serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            }
            if (settings._typeNameAssemblyFormatHandling != null)
            {
                serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            }
            if (settings._preserveReferencesHandling != null)
            {
                serializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
            }
            if (settings._referenceLoopHandling != null)
            {
                serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            }
            if (settings._missingMemberHandling != null)
            {
                serializer.MissingMemberHandling = settings.MissingMemberHandling;
            }
            if (settings._objectCreationHandling != null)
            {
                serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
            }
            if (settings._nullValueHandling != null)
            {
                serializer.NullValueHandling = settings.NullValueHandling;
            }
            if (settings._defaultValueHandling != null)
            {
                serializer.DefaultValueHandling = settings.DefaultValueHandling;
            }
            if (settings._constructorHandling != null)
            {
                serializer.ConstructorHandling = settings.ConstructorHandling;
            }
            if (settings._context != null)
            {
                serializer.Context = settings.Context;
            }
            if (settings._checkAdditionalContent != null)
            {
                serializer._checkAdditionalContent = settings._checkAdditionalContent;
            }

            if (settings.Error != null)
            {
                serializer.Error += settings.Error;
            }

            if (settings.ContractResolver != null)
            {
                serializer.ContractResolver = settings.ContractResolver;
            }
            if (settings.ReferenceResolverProvider != null)
            {
                serializer.ReferenceResolver = settings.ReferenceResolverProvider();
            }
            if (settings.TraceWriter != null)
            {
                serializer.TraceWriter = settings.TraceWriter;
            }
            if (settings.EqualityComparer != null)
            {
                serializer.EqualityComparer = settings.EqualityComparer;
            }
            if (settings.SerializationBinder != null)
            {
                serializer.SerializationBinder = settings.SerializationBinder;
            }

            // reader/writer specific
            // unset values won't override reader/writer set values
            if (settings._formatting != null)
            {
                serializer._formatting = settings._formatting;
            }
            if (settings._dateFormatHandling != null)
            {
                serializer._dateFormatHandling = settings._dateFormatHandling;
            }
            if (settings._dateTimeZoneHandling != null)
            {
                serializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
            }
            if (settings._dateParseHandling != null)
            {
                serializer._dateParseHandling = settings._dateParseHandling;
            }
            if (settings._dateFormatStringSet)
            {
                serializer._dateFormatString = settings._dateFormatString;
                serializer._dateFormatStringSet = settings._dateFormatStringSet;
            }
            if (settings._floatFormatHandling != null)
            {
                serializer._floatFormatHandling = settings._floatFormatHandling;
            }
            if (settings._floatParseHandling != null)
            {
                serializer._floatParseHandling = settings._floatParseHandling;
            }
            if (settings._stringEscapeHandling != null)
            {
                serializer._stringEscapeHandling = settings._stringEscapeHandling;
            }
            if (settings._culture != null)
            {
                serializer._culture = settings._culture;
            }
            if (settings._maxDepthSet)
            {
                serializer._maxDepth = settings._maxDepth;
                serializer._maxDepthSet = settings._maxDepthSet;
            }
        }

        /// <summary>
        /// Populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(TextReader reader, object target)
        {
            Populate(new JsonTextReader(reader), target);
        }

        /// <summary>
        /// Populates the JSON values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(JsonReader reader, object target)
        {
            PopulateInternal(reader, target);
        }

        internal virtual void PopulateInternal(JsonReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            serializerReader.Populate(traceJsonReader ?? reader, target);

            if (traceJsonReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to deserialize.</param>
        /// <returns>The <see cref="Object"/> being deserialized.</returns>
        public object Deserialize(JsonReader reader)
        {
            return Deserialize(reader, null);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="StringReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(TextReader reader, Type objectType)
        {
            return Deserialize(new JsonTextReader(reader), objectType);
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <returns>The instance of <typeparamref name="T"/> being deserialized.</returns>
        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        /// <summary>
        /// Deserializes the JSON structure contained by the specified <see cref="JsonReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(JsonReader reader, Type objectType)
        {
            return DeserializeInternal(reader, objectType);
        }

        internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // set serialization options onto reader
            CultureInfo previousCulture;
            DateTimeZoneHandling? previousDateTimeZoneHandling;
            DateParseHandling? previousDateParseHandling;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            SetupReader(reader, out previousCulture, out previousDateTimeZoneHandling, out previousDateParseHandling, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceJsonReader traceJsonReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceJsonReader(reader)
                : null;

            JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
            object value = serializerReader.Deserialize(traceJsonReader ?? reader, objectType, CheckAdditionalContent);

            if (traceJsonReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonReader.GetDeserializedJsonMessage(), null);
            }

            ResetReader(reader, previousCulture, previousDateTimeZoneHandling, previousDateParseHandling, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        private void SetupReader(JsonReader reader, out CultureInfo previousCulture, out DateTimeZoneHandling? previousDateTimeZoneHandling, out DateParseHandling? previousDateParseHandling, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString)
        {
            if (_culture != null && !_culture.Equals(reader.Culture))
            {
                previousCulture = reader.Culture;
                reader.Culture = _culture;
            }
            else
            {
                previousCulture = null;
            }

            if (_dateTimeZoneHandling != null && reader.DateTimeZoneHandling != _dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
                reader.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
            }
            else
            {
                previousDateTimeZoneHandling = null;
            }

            if (_dateParseHandling != null && reader.DateParseHandling != _dateParseHandling)
            {
                previousDateParseHandling = reader.DateParseHandling;
                reader.DateParseHandling = _dateParseHandling.GetValueOrDefault();
            }
            else
            {
                previousDateParseHandling = null;
            }

            if (_floatParseHandling != null && reader.FloatParseHandling != _floatParseHandling)
            {
                previousFloatParseHandling = reader.FloatParseHandling;
                reader.FloatParseHandling = _floatParseHandling.GetValueOrDefault();
            }
            else
            {
                previousFloatParseHandling = null;
            }

            if (_maxDepthSet && reader.MaxDepth != _maxDepth)
            {
                previousMaxDepth = reader.MaxDepth;
                reader.MaxDepth = _maxDepth;
            }
            else
            {
                previousMaxDepth = null;
            }

            if (_dateFormatStringSet && reader.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = reader.DateFormatString;
                reader.DateFormatString = _dateFormatString;
            }
            else
            {
                previousDateFormatString = null;
            }

            JsonTextReader textReader = reader as JsonTextReader;
            if (textReader != null)
            {
                DefaultContractResolver resolver = _contractResolver as DefaultContractResolver;
                if (resolver != null)
                {
                    textReader.NameTable = resolver.GetNameTable();
                }
            }
        }

        private void ResetReader(JsonReader reader, CultureInfo previousCulture, DateTimeZoneHandling? previousDateTimeZoneHandling, DateParseHandling? previousDateParseHandling, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string previousDateFormatString)
        {
            // reset reader back to previous options
            if (previousCulture != null)
            {
                reader.Culture = previousCulture;
            }
            if (previousDateTimeZoneHandling != null)
            {
                reader.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousDateParseHandling != null)
            {
                reader.DateParseHandling = previousDateParseHandling.GetValueOrDefault();
            }
            if (previousFloatParseHandling != null)
            {
                reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
            }
            if (_maxDepthSet)
            {
                reader.MaxDepth = previousMaxDepth;
            }
            if (_dateFormatStringSet)
            {
                reader.DateFormatString = previousDateFormatString;
            }

            JsonTextReader textReader = reader as JsonTextReader;
            if (textReader != null)
            {
                textReader.NameTable = null;
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(TextWriter textWriter, object value)
        {
            Serialize(new JsonTextWriter(textWriter), value);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="JsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(JsonWriter jsonWriter, object value, Type objectType)
        {
            SerializeInternal(jsonWriter, value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(TextWriter textWriter, object value, Type objectType)
        {
            Serialize(new JsonTextWriter(textWriter), value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the JSON structure
        /// using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(JsonWriter jsonWriter, object value)
        {
            SerializeInternal(jsonWriter, value, null);
        }

        internal virtual void SerializeInternal(JsonWriter jsonWriter, object value, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(jsonWriter, nameof(jsonWriter));

            // set serialization options onto writer
            Formatting? previousFormatting = null;
            if (_formatting != null && jsonWriter.Formatting != _formatting)
            {
                previousFormatting = jsonWriter.Formatting;
                jsonWriter.Formatting = _formatting.GetValueOrDefault();
            }

            DateFormatHandling? previousDateFormatHandling = null;
            if (_dateFormatHandling != null && jsonWriter.DateFormatHandling != _dateFormatHandling)
            {
                previousDateFormatHandling = jsonWriter.DateFormatHandling;
                jsonWriter.DateFormatHandling = _dateFormatHandling.GetValueOrDefault();
            }

            DateTimeZoneHandling? previousDateTimeZoneHandling = null;
            if (_dateTimeZoneHandling != null && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
            {
                previousDateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
                jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.GetValueOrDefault();
            }

            FloatFormatHandling? previousFloatFormatHandling = null;
            if (_floatFormatHandling != null && jsonWriter.FloatFormatHandling != _floatFormatHandling)
            {
                previousFloatFormatHandling = jsonWriter.FloatFormatHandling;
                jsonWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
            }

            StringEscapeHandling? previousStringEscapeHandling = null;
            if (_stringEscapeHandling != null && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
            {
                previousStringEscapeHandling = jsonWriter.StringEscapeHandling;
                jsonWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
            }

            CultureInfo previousCulture = null;
            if (_culture != null && !_culture.Equals(jsonWriter.Culture))
            {
                previousCulture = jsonWriter.Culture;
                jsonWriter.Culture = _culture;
            }

            string previousDateFormatString = null;
            if (_dateFormatStringSet && jsonWriter.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = jsonWriter.DateFormatString;
                jsonWriter.DateFormatString = _dateFormatString;
            }

            TraceJsonWriter traceJsonWriter = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceJsonWriter(jsonWriter)
                : null;

            JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
            serializerWriter.Serialize(traceJsonWriter ?? jsonWriter, value, objectType);

            if (traceJsonWriter != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceJsonWriter.GetSerializedJsonMessage(), null);
            }

            // reset writer back to previous options
            if (previousFormatting != null)
            {
                jsonWriter.Formatting = previousFormatting.GetValueOrDefault();
            }
            if (previousDateFormatHandling != null)
            {
                jsonWriter.DateFormatHandling = previousDateFormatHandling.GetValueOrDefault();
            }
            if (previousDateTimeZoneHandling != null)
            {
                jsonWriter.DateTimeZoneHandling = previousDateTimeZoneHandling.GetValueOrDefault();
            }
            if (previousFloatFormatHandling != null)
            {
                jsonWriter.FloatFormatHandling = previousFloatFormatHandling.GetValueOrDefault();
            }
            if (previousStringEscapeHandling != null)
            {
                jsonWriter.StringEscapeHandling = previousStringEscapeHandling.GetValueOrDefault();
            }
            if (_dateFormatStringSet)
            {
                jsonWriter.DateFormatString = previousDateFormatString;
            }
            if (previousCulture != null)
            {
                jsonWriter.Culture = previousCulture;
            }
        }

        internal IReferenceResolver GetReferenceResolver()
        {
            if (_referenceResolver == null)
            {
                _referenceResolver = new DefaultReferenceResolver();
            }

            return _referenceResolver;
        }

        internal JsonConverter GetMatchingConverter(Type type)
        {
            return GetMatchingConverter(_converters, type);
        }

        internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
        {
#if DEBUG
            ValidationUtils.ArgumentNotNull(objectType, nameof(objectType));
#endif

            if (converters != null)
            {
                for (int i = 0; i < converters.Count; i++)
                {
                    JsonConverter converter = converters[i];

                    if (converter.CanConvert(objectType))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }

        internal void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
    }
}