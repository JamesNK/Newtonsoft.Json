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
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;

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
    private PreserveReferencesHandling _preserveReferencesHandling;
    private ReferenceLoopHandling _referenceLoopHandling;
    private MissingMemberHandling _missingMemberHandling;
    private ObjectCreationHandling _objectCreationHandling;
    private NullValueHandling _nullValueHandling;
    private DefaultValueHandling _defaultValueHandling;
    private JsonConverterCollection _converters;
    private IContractResolver _contractResolver;
    private IReferenceResolver _referenceResolver;
    private SerializationBinder _binder;

    private static readonly IList<JsonConverter> BuiltInConverters = new List<JsonConverter>
      {
#if !PocketPC && !SILVERLIGHT
        new EntityKeyMemberConverter()
#endif
      };

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
    /// Gets or sets how type name writing and reading is handled by the serializer.
    /// </summary>
    public virtual TypeNameHandling TypeNameHandling
    {
      get { return _typeNameHandling; }
      set
      {
        if (value < TypeNameHandling.None || value > TypeNameHandling.All)
          throw new ArgumentOutOfRangeException("value");

        _typeNameHandling = value;
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
        if (value < DefaultValueHandling.Include || value > DefaultValueHandling.Ignore)
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
    #endregion

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
      _typeNameHandling = JsonSerializerSettings.DefaultTypeNameHandling;
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

        jsonSerializer.TypeNameHandling = settings.TypeNameHandling;
        jsonSerializer.PreserveReferencesHandling = settings.PreserveReferencesHandling;
        jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
        jsonSerializer.MissingMemberHandling = settings.MissingMemberHandling;
        jsonSerializer.ObjectCreationHandling = settings.ObjectCreationHandling;
        jsonSerializer.NullValueHandling = settings.NullValueHandling;
        jsonSerializer.DefaultValueHandling = settings.DefaultValueHandling;

        if (settings.ContractResolver != null)
          jsonSerializer.ContractResolver = settings.ContractResolver;
        if (settings.ReferenceResolver != null)
          jsonSerializer.ReferenceResolver = settings.ReferenceResolver;
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

      JsonSerializerReader serializerReader = new JsonSerializerReader(this);
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
    /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
    /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
    public object Deserialize(JsonReader reader, Type objectType)
    {
      return DeserializeInternal(reader, objectType);
    }

    internal virtual object DeserializeInternal(JsonReader reader, Type objectType)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      JsonSerializerReader serializerReader = new JsonSerializerReader(this);
      return serializerReader.Deserialize(reader, objectType);
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

    /// <summary>
    /// Serializes the specified <see cref="Object"/> and writes the Json structure
    /// to a <c>Stream</c> using the specified <see cref="JsonWriter"/>. 
    /// </summary>
    /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the Json structure.</param>
    /// <param name="value">The <see cref="Object"/> to serialize.</param>
    public void Serialize(JsonWriter jsonWriter, object value)
    {
      SerializeInternal(jsonWriter, value);
    }

    internal virtual void SerializeInternal(JsonWriter jsonWriter, object value)
    {
      ValidationUtils.ArgumentNotNull(jsonWriter, "jsonWriter");

      JsonSerializerWriter serializerWriter = new JsonSerializerWriter(this);
      serializerWriter.Serialize(jsonWriter, value);
    }

    internal bool HasClassConverter(Type objectType, out JsonConverter converter)
    {
      if (objectType == null)
        throw new ArgumentNullException("objectType");

      converter = JsonTypeReflector.GetConverter(objectType, objectType);
      return (converter != null);
    }

    internal bool HasMatchingConverter(Type type, out JsonConverter matchingConverter)
    {
      if (HasMatchingConverter(_converters, type, out matchingConverter))
        return true;
      if (HasMatchingConverter(BuiltInConverters, type, out matchingConverter))
        return true;

      return false;
    }

    internal static bool HasMatchingConverter(IList<JsonConverter> converters, Type objectType, out JsonConverter matchingConverter)
    {
      if (objectType == null)
        throw new ArgumentNullException("objectType");

      if (converters != null)
      {
        for (int i = 0; i < converters.Count; i++)
        {
          JsonConverter converter = converters[i];

          if (converter.CanConvert(objectType))
          {
            matchingConverter = converter;
            return true;
          }
        }
      }

      matchingConverter = null;
      return false;
    }
  }
}