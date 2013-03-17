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
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
using ErrorEventArgs=Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Serializes and deserializes objects into and from the JSON format.
  /// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into JSON.
  /// </summary>
  public class JsonSerializer
  {
    #region Properties
    private TypeNameHandling _typeNameHandling;
    private FormatterAssemblyStyle _typeNameAssemblyFormat;
    private PreserveReferencesHandling _preserveReferencesHandling;
    private ReferenceLoopHandling _referenceLoopHandling;
    private MemberNameMatchHandling _memberNameMatchHandling;
    private MemberTypeConversionHandling _memberTypeConversionHandling;
    private MissingMemberHandling _missingMemberHandling;
    private ObjectCreationHandling _objectCreationHandling;
    private NullValueHandling _nullValueHandling;
    private DefaultValueHandling _defaultValueHandling;
    private ConstructorHandling _constructorHandling;
    private JsonConverterCollection _converters;
    private IContractResolver _contractResolver;
    private IReferenceResolver _referenceResolver;
    private ITraceWriter _traceWriter;
    private SerializationBinder _binder;
    private StreamingContext _context;
    private Formatting? _formatting;
    private DateFormatHandling? _dateFormatHandling;
    private DateTimeZoneHandling? _dateTimeZoneHandling;
    private DateParseHandling? _dateParseHandling;
    private FloatFormatHandling? _floatFormatHandling;
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
      get
      {
        if (_referenceResolver == null)
          _referenceResolver = new DefaultReferenceResolver();

        return _referenceResolver;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value", "Reference resolver cannot be null.");

        _referenceResolver = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
    /// </summary>
    public virtual SerializationBinder Binder
    {
      get
      {
        return _binder;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value", "Serialization binder cannot be null.");

        _binder = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
    /// </summary>
    /// <value>The trace writer.</value>
    public virtual ITraceWriter TraceWriter
    {
      get
      {
        return _traceWriter;
      }
      set
      {
        _traceWriter = value;
      }
    }

    /// <summary>
    /// Gets or sets how type name writing and reading is handled by the serializer.
    /// </summary>
    public virtual TypeNameHandling TypeNameHandling
    {
      get { return _typeNameHandling; }
      set
      {
        if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
          throw new ArgumentOutOfRangeException("value");

        _typeNameHandling = value;
      }
    }

    /// <summary>
    /// Gets or sets how a type name assembly is written and resolved by the serializer.
    /// </summary>
    /// <value>The type name assembly format.</value>
    public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
    {
      get { return _typeNameAssemblyFormat; }
      set
      {
        if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
          throw new ArgumentOutOfRangeException("value");

        _typeNameAssemblyFormat = value;
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
          throw new ArgumentOutOfRangeException("value");

        _preserveReferencesHandling = value;
      }
    }

    /// <summary>
    /// Get or set how reference loops (e.g. a class referencing itself) is handled.
    /// </summary>
    public virtual ReferenceLoopHandling ReferenceLoopHandling
    {
      get { return _referenceLoopHandling; }
      set
      {
        if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
          throw new ArgumentOutOfRangeException("value");

        _referenceLoopHandling = value;
      }
    }

    /// <summary>
    /// Gets or sets whether class members are looked up using case sensitive or insensitive manner.
    /// </summary>
    public virtual MemberNameMatchHandling MemberNameMatchHandling
    {
      get { return _memberNameMatchHandling; }
      set
      {
        if (value < MemberNameMatchHandling.CaseInsensitive || value > MemberNameMatchHandling.CaseSensitive)
          throw new ArgumentOutOfRangeException("value");

        _memberNameMatchHandling = value;
      }
    }

    /// <summary>
    /// Gets or sets whether the deserializer attempts to convert basic types (e.g. the string "2" to an integer field)
    /// </summary>
    public virtual MemberTypeConversionHandling MemberTypeConversionHandling
    {
      get { return _memberTypeConversionHandling; }
      set
      {
        if (value < MemberTypeConversionHandling.AllowTypeConversions || value > MemberTypeConversionHandling.ProhibitTypeConversions)
          throw new ArgumentOutOfRangeException("value");

        _memberTypeConversionHandling = value;
      }
    }


    /// <summary>
    /// Gets or sets whether the deserializer replaces lists with default values
    /// </summary>
    public virtual NonEmptyListHandling NonEmptyListHandling { get; set; }

    /// <summary>
    /// Get or set how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
    /// </summary>
    public virtual MissingMemberHandling MissingMemberHandling
    {
      get { return _missingMemberHandling; }
      set
      {
        if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
          throw new ArgumentOutOfRangeException("value");

        _missingMemberHandling = value;
      }
    }

    /// <summary>
    /// Get or set how null values are handled during serialization and deserialization.
    /// </summary>
    public virtual NullValueHandling NullValueHandling
    {
      get { return _nullValueHandling; }
      set
      {
        if (value < NullValueHandling.Include || value > NullValueHandling.Ignore)
          throw new ArgumentOutOfRangeException("value");

        _nullValueHandling = value;
      }
    }

    /// <summary>
    /// Get or set how null default are handled during serialization and deserialization.
    /// </summary>
    public virtual DefaultValueHandling DefaultValueHandling
    {
      get { return _defaultValueHandling; }
      set
      {
        if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
          throw new ArgumentOutOfRangeException("value");

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
          throw new ArgumentOutOfRangeException("value");

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
          throw new ArgumentOutOfRangeException("value");

        _constructorHandling = value;
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
          _converters = new JsonConverterCollection();

        return _converters;
      }
    }

    /// <summary>
    /// Gets or sets the contract resolver used by the serializer when
    /// serializing .NET objects to JSON and vice versa.
    /// </summary>
    public virtual IContractResolver ContractResolver
    {
      get
      {
        if (_contractResolver == null)
          _contractResolver = DefaultContractResolver.Instance;

        return _contractResolver;
      }
      set { _contractResolver = value; }
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
    /// Get or set how dates are written to JSON text.
    /// </summary>
    public virtual DateFormatHandling DateFormatHandling
    {
      get { return _dateFormatHandling ?? JsonSerializerSettings.DefaultDateFormatHandling; }
      set { _dateFormatHandling = value; }
    }

    /// <summary>
    /// Get or set how <see cref="DateTime"/> time zones are handling during serialization and deserialization.
    /// </summary>
    public virtual DateTimeZoneHandling DateTimeZoneHandling
    {
      get { return _dateTimeZoneHandling ?? JsonSerializerSettings.DefaultDateTimeZoneHandling; }
      set { _dateTimeZoneHandling = value; }
    }

    /// <summary>
    /// Get or set how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON.
    /// </summary>
    public virtual DateParseHandling DateParseHandling
    {
      get { return _dateParseHandling ?? JsonSerializerSettings.DefaultDateParseHandling; }
      set { _dateParseHandling = value; }
    }

    /// <summary>
    /// Get or set how special floating point values, e.g. <see cref="F:System.Double.NaN"/>,
    /// <see cref="F:System.Double.PositiveInfinity"/> and <see cref="F:System.Double.NegativeInfinity"/>,
    /// are written as JSON.
    /// </summary>
    public virtual FloatFormatHandling FloatFormatHandling
    {
      get { return _floatFormatHandling ?? JsonSerializerSettings.DefaultFloatFormatHandling; }
      set { _floatFormatHandling = value; }
    }

    /// <summary>
    /// Get or set how strings are escaped when writing JSON text.
    /// </summary>
    public virtual StringEscapeHandling StringEscapeHandling
    {
      get { return _stringEscapeHandling ?? JsonSerializerSettings.DefaultStringEscapeHandling; }
      set { _stringEscapeHandling = value; }
    }

    /// <summary>
    /// Get or set how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatting when writing JSON text.
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
          throw new ArgumentException("Value must be positive.", "value");

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

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
    /// </summary>
    public JsonSerializer()
    {
      _referenceLoopHandling = JsonSerializerSettings.DefaultReferenceLoopHandling;
      _memberNameMatchHandling = JsonSerializerSettings.DefaultMemberNameMatchHandling;
      _memberTypeConversionHandling = JsonSerializerSettings.DefaultMemberTypeConversionHandling;
      _missingMemberHandling = JsonSerializerSettings.DefaultMissingMemberHandling;
      _nullValueHandling = JsonSerializerSettings.DefaultNullValueHandling;
      _defaultValueHandling = JsonSerializerSettings.DefaultDefaultValueHandling;
      _objectCreationHandling = JsonSerializerSettings.DefaultObjectCreationHandling;
      _preserveReferencesHandling = JsonSerializerSettings.DefaultPreserveReferencesHandling;
      _constructorHandling = JsonSerializerSettings.DefaultConstructorHandling;
      _typeNameHandling = JsonSerializerSettings.DefaultTypeNameHandling;
      _context = JsonSerializerSettings.DefaultContext;
      _binder = DefaultSerializationBinder.Instance;
    }

    /// <summary>
    /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The settings to be applied to the <see cref="JsonSerializer"/>.</param>
    /// <returns>A new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/>.</returns>
    public static JsonSerializer Create(JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = new JsonSerializer();

      if (settings != null)
      {
        if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
          jsonSerializer.Converters.AddRange(settings.Converters);

        // serializer specific
        jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
        jsonSerializer.TypeNameAssemblyFormat = settings.TypeNameAssemblyFormat;
        jsonSerializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
        jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
        jsonSerializer.MemberNameMatchHandling = settings.MemberNameMatchHandling;
        jsonSerializer.MemberTypeConversionHandling = settings.MemberTypeConversionHandling;
        jsonSerializer.NonEmptyListHandling = settings.NonEmptyListHandling;
        jsonSerializer.MissingMemberHandling = settings.MissingMemberHandling;
        jsonSerializer.ObjectCreationHandling = settings.ObjectCreationHandling;
        jsonSerializer.NullValueHandling = settings.NullValueHandling;
        jsonSerializer.DefaultValueHandling = settings.DefaultValueHandling;
        jsonSerializer.ConstructorHandling = settings.ConstructorHandling;
        jsonSerializer.Context = settings.Context;
        jsonSerializer._checkAdditionalContent = settings._checkAdditionalContent;

        // reader/writer specific
        // unset values won't override reader/writer set values
        jsonSerializer._formatting = settings._formatting;
        jsonSerializer._dateFormatHandling = settings._dateFormatHandling;
        jsonSerializer._dateTimeZoneHandling = settings._dateTimeZoneHandling;
        jsonSerializer._dateParseHandling = settings._dateParseHandling;
        jsonSerializer._dateFormatString = settings._dateFormatString;
        jsonSerializer._dateFormatStringSet = settings._dateFormatStringSet;
        jsonSerializer._floatFormatHandling = settings._floatFormatHandling;
        jsonSerializer._stringEscapeHandling = settings._stringEscapeHandling;
        jsonSerializer._culture = settings._culture;
        jsonSerializer._maxDepth = settings._maxDepth;
        jsonSerializer._maxDepthSet = settings._maxDepthSet;

        if (settings.Error != null)
          jsonSerializer.Error += settings.Error;

        if (settings.ContractResolver != null)
          jsonSerializer.ContractResolver = settings.ContractResolver;
        if (settings.ReferenceResolver != null)
          jsonSerializer.ReferenceResolver = settings.ReferenceResolver;
        if (settings.TraceWriter != null)
          jsonSerializer.TraceWriter = settings.TraceWriter;
        if (settings.Binder != null)
          jsonSerializer.Binder = settings.Binder;
      }

      return jsonSerializer;
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
      ValidationUtils.ArgumentNotNull(reader, "reader");
      ValidationUtils.ArgumentNotNull(target, "target");

      JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
      serializerReader.Populate(reader, target);
    }

    /// <summary>
    /// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> that contains the JSON structure to deserialize.</param>
    /// <returns>The <see cref="Object"/> being deserialized.</returns>
    public object Deserialize(JsonReader reader)
    {
      return Deserialize(reader, null);
    }

    /// <summary>
    /// Deserializes the Json structure contained by the specified <see cref="StringReader"/>
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
    /// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>
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
    /// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>
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
      ValidationUtils.ArgumentNotNull(reader, "reader");

      // set serialization options onto reader
      CultureInfo previousCulture = null;
      if (_culture != null && !_culture.Equals(reader.Culture))
      {
        previousCulture = reader.Culture;
        reader.Culture = _culture;
      }

      DateTimeZoneHandling? previousDateTimeZoneHandling = null;
      if (_dateTimeZoneHandling != null && reader.DateTimeZoneHandling != _dateTimeZoneHandling)
      {
        previousDateTimeZoneHandling = reader.DateTimeZoneHandling;
        reader.DateTimeZoneHandling = _dateTimeZoneHandling.Value;
      }

      DateParseHandling? previousDateParseHandling = null;
      if (_dateParseHandling != null && reader.DateParseHandling != _dateParseHandling)
      {
        previousDateParseHandling = reader.DateParseHandling;
        reader.DateParseHandling = _dateParseHandling.Value;
      }

      int? previousMaxDepth = null;
      if (_maxDepthSet && reader.MaxDepth != _maxDepth)
      {
        previousMaxDepth = reader.MaxDepth;
        reader.MaxDepth = _maxDepth;
      }

      JsonSerializerInternalReader serializerReader = new JsonSerializerInternalReader(this);
      object value = serializerReader.Deserialize(reader, objectType, CheckAdditionalContent);

      // reset reader back to previous options
      if (previousCulture != null)
        reader.Culture = previousCulture;
      if (previousDateTimeZoneHandling != null)
        reader.DateTimeZoneHandling = previousDateTimeZoneHandling.Value;
      if (previousDateParseHandling != null)
        reader.DateParseHandling = previousDateParseHandling.Value;
      if (_maxDepthSet)
        reader.MaxDepth = previousMaxDepth;

      return value;
    }

    /// <summary>
    /// Serializes the specified <see cref="Object"/> and writes the Json structure
    /// to a <c>Stream</c> using the specified <see cref="TextWriter"/>. 
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> used to write the Json structure.</param>
    /// <param name="value">The <see cref="Object"/> to serialize.</param>
    public void Serialize(TextWriter textWriter, object value)
    {
      Serialize(new JsonTextWriter(textWriter), value);
    }

    public void Serialize(JsonWriter jsonWriter, object value, Type rootType)
    {
      SerializeInternal(jsonWriter, value, rootType);
    }

    public void Serialize(TextWriter textWriter, object value, Type rootType)
    {
      Serialize(new JsonTextWriter(textWriter), value, rootType);
    }

    /// <summary>
    /// Serializes the specified <see cref="Object"/> and writes the Json structure
    /// to a <c>Stream</c> using the specified <see cref="JsonWriter"/>. 
    /// </summary>
    /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the Json structure.</param>
    /// <param name="value">The <see cref="Object"/> to serialize.</param>
    public void Serialize(JsonWriter jsonWriter, object value)
    {
      SerializeInternal(jsonWriter, value, null);
    }

    public void Serialize<T>(JsonWriter jsonWriter, object value)
    {
      SerializeInternal(jsonWriter, value, typeof(T));
    }

    internal virtual void SerializeInternal(JsonWriter jsonWriter, object value, Type rootType)
    {
      ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");

      // set serialization options onto writer
      Formatting? previousFormatting = null;
      if (_formatting != null && jsonWriter.Formatting != _formatting)
      {
        previousFormatting = jsonWriter.Formatting;
        jsonWriter.Formatting = _formatting.Value;
      }

      DateFormatHandling? previousDateFormatHandling = null;
      if (_dateFormatHandling != null && jsonWriter.DateFormatHandling != _dateFormatHandling)
      {
        previousDateFormatHandling = jsonWriter.DateFormatHandling;
        jsonWriter.DateFormatHandling = _dateFormatHandling.Value;
      }

      DateTimeZoneHandling? previousDateTimeZoneHandling = null;
      if (_dateTimeZoneHandling != null && jsonWriter.DateTimeZoneHandling != _dateTimeZoneHandling)
      {
        previousDateTimeZoneHandling = jsonWriter.DateTimeZoneHandling;
        jsonWriter.DateTimeZoneHandling = _dateTimeZoneHandling.Value;
      }

      FloatFormatHandling? previousFloatFormatHandling = null;
      if (_floatFormatHandling != null && jsonWriter.FloatFormatHandling != _floatFormatHandling)
      {
        previousFloatFormatHandling = jsonWriter.FloatFormatHandling;
        jsonWriter.FloatFormatHandling = _floatFormatHandling.Value;
      }

      StringEscapeHandling? previousStringEscapeHandling = null;
      if (_stringEscapeHandling != null && jsonWriter.StringEscapeHandling != _stringEscapeHandling)
      {
        previousStringEscapeHandling = jsonWriter.StringEscapeHandling;
        jsonWriter.StringEscapeHandling = _stringEscapeHandling.Value;
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

      JsonSerializerInternalWriter serializerWriter = new JsonSerializerInternalWriter(this);
      serializerWriter.Serialize(jsonWriter, value, rootType);

      // reset writer back to previous options
      if (previousFormatting != null)
        jsonWriter.Formatting = previousFormatting.Value;
      if (previousDateFormatHandling != null)
        jsonWriter.DateFormatHandling = previousDateFormatHandling.Value;
      if (previousDateTimeZoneHandling != null)
        jsonWriter.DateTimeZoneHandling = previousDateTimeZoneHandling.Value;
      if (previousFloatFormatHandling != null)
        jsonWriter.FloatFormatHandling = previousFloatFormatHandling.Value;
      if (previousStringEscapeHandling != null)
        jsonWriter.StringEscapeHandling = previousStringEscapeHandling.Value;
      if (_dateFormatStringSet)
        jsonWriter.DateFormatString = previousDateFormatString;
      if (previousCulture != null)
        jsonWriter.Culture = previousCulture;
    }

    internal JsonConverter GetMatchingConverter(Type type)
    {
      return GetMatchingConverter(_converters, type);
    }

    internal static JsonConverter GetMatchingConverter(IList<JsonConverter> converters, Type objectType)
    {
#if DEBUG
      ValidationUtils.ArgumentNotNull(objectType, "objectType");
#endif

      if (converters != null)
      {
        for (int i = 0; i < converters.Count; i++)
        {
          JsonConverter converter = converters[i];

          if (converter.CanConvert(objectType))
            return converter;
        }
      }

      return null;
    }

    internal void OnError(ErrorEventArgs e)
    {
      EventHandler<ErrorEventArgs> error = Error;
      if (error != null)
        error(this, e);
    }
  }
}