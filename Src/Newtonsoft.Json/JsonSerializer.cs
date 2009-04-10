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
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Serializes and deserializes objects into and from the Json format.
  /// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into Json.
  /// </summary>
  public class JsonSerializer
  {
    #region Properties
    private ReferenceLoopHandling _referenceLoopHandling;
    private MissingMemberHandling _missingMemberHandling;
    private ObjectCreationHandling _objectCreationHandling;
    private NullValueHandling _nullValueHandling;
    private DefaultValueHandling _defaultValueHandling;
    private int _level;
    private JsonConverterCollection _converters;
    private IMappingResolver _mappingResolver;

    /// <summary>
    /// Get or set how reference loops (e.g. a class referencing itself) is handled.
    /// </summary>
    public ReferenceLoopHandling ReferenceLoopHandling
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
    public MissingMemberHandling MissingMemberHandling
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
    public NullValueHandling NullValueHandling
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
    public DefaultValueHandling DefaultValueHandling
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
    public ObjectCreationHandling ObjectCreationHandling
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
    public JsonConverterCollection Converters
    {
      get
      {
        if (_converters == null)
          _converters = new JsonConverterCollection();

        return _converters;
      }
    }

    public IMappingResolver MappingResolver
    {
      get { return _mappingResolver; }
      set { _mappingResolver = value; }
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
    }


    /// <summary>
    /// Creates a new <see cref="JsonSerializer"/> instance using the specified <see cref="JsonSerializerSettings"/> objects.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    public static JsonSerializer Create(JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = new JsonSerializer();

      if (settings != null)
      {
        if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
          jsonSerializer.Converters.AddRange(settings.Converters);

        jsonSerializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
        jsonSerializer.MissingMemberHandling = settings.MissingMemberHandling;
        jsonSerializer.ObjectCreationHandling = settings.ObjectCreationHandling;
        jsonSerializer.NullValueHandling = settings.NullValueHandling;
        jsonSerializer.DefaultValueHandling = settings.DefaultValueHandling;
        jsonSerializer.MappingResolver = settings.MappingResolver;
      }

      return jsonSerializer;
    }

    #region Deserialize
    /// <summary>
    /// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> that contains the Json structure to deserialize.</param>
    /// <returns>The <see cref="Object"/> being deserialized.</returns>
    public object Deserialize(JsonReader reader)
    {
      return Deserialize(reader, null);
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
      if (reader == null)
        throw new ArgumentNullException("reader");

      if (!reader.Read())
        return null;

      if (objectType != null)
        return CreateObject(reader, objectType, null, null);
      else
        return CreateJToken(reader);
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

    private JToken CreateJToken(JsonReader reader)
    {
      JToken token;
      using (JTokenWriter writer = new JTokenWriter())
      {
        writer.WriteToken(reader);
        token = writer.Token;
      }

      return token;
    }

    private bool HasClassConverter(Type objectType, out JsonConverter converter)
    {
      if (objectType == null)
        throw new ArgumentNullException("objectType");

      converter = JsonTypeReflector.GetConverter(objectType, objectType);
      return (converter != null);
    }

    private object CreateObject(JsonReader reader, Type objectType, object existingValue, JsonConverter memberConverter)
    {
      _level++;

      object value;
      JsonConverter converter;

      if (memberConverter != null)
      {
        return memberConverter.ReadJson(reader, objectType);
      }
      else if (objectType != null && HasClassConverter(objectType, out converter))
      {
        return converter.ReadJson(reader, objectType);
      }
      else if (objectType != null && HasMatchingConverter(objectType, out converter))
      {
        return converter.ReadJson(reader, objectType);
      }
      else if (objectType == typeof(JsonRaw))
      {
        return JsonRaw.Create(reader);
      }
      else
      {
        switch (reader.TokenType)
        {
          // populate a typed object or generic dictionary/array
          // depending upon whether an objectType was supplied
          case JsonToken.StartObject:
            if (objectType == null)
            {
              value = CreateJToken(reader);
            }
            else if (CollectionUtils.IsDictionaryType(objectType))
            {
              if (existingValue == null)
                value = CreateAndPopulateDictionary(reader, objectType);
              else
                value = PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(existingValue), reader);
            }
            else
            {
              if (existingValue == null)
                value = CreateAndPopulateObject(reader, objectType);
              else
                value = PopulateObject(existingValue, reader, objectType);
            }
            break;
          case JsonToken.StartArray:
            if (objectType != null)
            {
              if (existingValue == null)
                value = CreateAndPopulateList(reader, objectType);
              else
                value = PopulateList(CollectionUtils.CreateCollectionWrapper(existingValue), ReflectionUtils.GetCollectionItemType(objectType), reader);
            }
            else
            {
              value = CreateJToken(reader);
            }
            break;
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.String:
          case JsonToken.Boolean:
          case JsonToken.Date:
            value = EnsureType(reader.Value, objectType);
            break;
          case JsonToken.StartConstructor:
          case JsonToken.EndConstructor:
            string constructorName = reader.Value.ToString();

            value = constructorName;
            break;
          case JsonToken.Null:
          case JsonToken.Undefined:
            if (objectType == typeof(DBNull))
              value = DBNull.Value;
            else
              value = null;
            break;
          default:
            throw new JsonSerializationException("Unexpected token while deserializing object: " + reader.TokenType);
        }
      }

      _level--;

      return value;
    }

    private object EnsureType(object value, Type targetType)
    {
      // do something about null value when the targetType is a valuetype?
      if (value == null)
        return null;

      if (targetType == null)
        return value;

      Type valueType = value.GetType();

      // type of value and type of target don't match
      // attempt to convert value's type to target's type
      if (valueType != targetType)
      {
        return ConvertUtils.ConvertOrCast(value, CultureInfo.InvariantCulture, targetType);
      }
      else
      {
        return value;
      }
    }

    private JsonMemberMappingCollection GetMemberMappings(Type objectType)
    {
      ValidationUtils.ArgumentNotNull(objectType, "objectType");

      if (_mappingResolver != null)
        return _mappingResolver.ResolveMappings(objectType);
      
      return DefaultMappingResolver.Instance.ResolveMappings(objectType);
    }

    private void SetObjectMember(JsonReader reader, object target, Type targetType, string memberName)
    {
      JsonMemberMappingCollection memberMappings = GetMemberMappings(targetType);

      JsonMemberMapping memberMapping;
      // attempt exact case match first
      // then try match ignoring case
      if (memberMappings.TryGetClosestMatchMapping(memberName, out memberMapping))
      {
        SetMappingValue(memberMapping, reader, target);
      }
      else
      {
        if (_missingMemberHandling == MissingMemberHandling.Error)
          throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, targetType.Name));

        reader.Skip();
      }
    }

    private void SetMappingValue(JsonMemberMapping memberMapping, JsonReader reader, object target)
    {
      if (memberMapping.Ignored)
      {
        reader.Skip();
        return;
      }

      // get the member's underlying type
      Type memberType = ReflectionUtils.GetMemberUnderlyingType(memberMapping.Member);

      object currentValue = null;
      bool useExistingValue = false;

      if ((_objectCreationHandling == ObjectCreationHandling.Auto || _objectCreationHandling == ObjectCreationHandling.Reuse)
          && (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject))
      {
        currentValue = ReflectionUtils.GetMemberValue(memberMapping.Member, target);

        useExistingValue = (currentValue != null && !memberType.IsArray && !ReflectionUtils.InheritsGenericDefinition(memberType, typeof(ReadOnlyCollection<>)));
      }

      if (!memberMapping.Writable && !useExistingValue)
      {
        reader.Skip();
        return;
      }

      object value = CreateObject(reader, memberType, (useExistingValue) ? currentValue : null, JsonTypeReflector.GetConverter(memberMapping.Member, memberType));

      if (!useExistingValue && ShouldSetMappingValue(memberMapping, value))
        ReflectionUtils.SetMemberValue(memberMapping.Member, target, value);
    }

    private bool ShouldSetMappingValue(JsonMemberMapping memberMapping, object value)
    {
      if (_nullValueHandling == NullValueHandling.Ignore && value == null)
        return false;

      if (_defaultValueHandling == DefaultValueHandling.Ignore && Equals(value, memberMapping.DefaultValue))
        return false;

      if (!memberMapping.Writable)
        return false;

      return true;
    }

    private object CreateAndPopulateDictionary(JsonReader reader, Type objectType)
    {
      if (IsTypeGenericDictionaryInterface(objectType))
      {
        Type keyType;
        Type valueType;
        ReflectionUtils.GetDictionaryKeyValueTypes(objectType, out keyType, out valueType);
        objectType = ReflectionUtils.MakeGenericType(typeof(Dictionary<,>), keyType, valueType);
      }

      IWrappedDictionary dictionary = CollectionUtils.CreateDictionaryWrapper(Activator.CreateInstance(objectType));
      PopulateDictionary(dictionary, reader);

      return dictionary.UnderlyingDictionary;
    }

    private IDictionary PopulateDictionary(IWrappedDictionary dictionary, JsonReader reader)
    {
      Type dictionaryType = dictionary.UnderlyingDictionary.GetType();
      Type dictionaryKeyType = ReflectionUtils.GetDictionaryKeyType(dictionaryType);
      Type dictionaryValueType = ReflectionUtils.GetDictionaryValueType(dictionaryType);

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            object keyValue = EnsureType(reader.Value, dictionaryKeyType);
            reader.Read();

            dictionary.Add(keyValue, CreateObject(reader, dictionaryValueType, null, null));
            break;
          case JsonToken.EndObject:
            return dictionary;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      }

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateAndPopulateList(JsonReader reader, Type objectType)
    {
      if (IsTypeGenericCollectionInterface(objectType))
      {
        Type itemType = ReflectionUtils.GetCollectionItemType(objectType);
        objectType = ReflectionUtils.MakeGenericType(typeof(List<>), itemType);
      }

      return CollectionUtils.CreateAndPopulateList(objectType, l => PopulateList(l, ReflectionUtils.GetCollectionItemType(objectType), reader));
    }

    private IList PopulateList(IList list, Type listItemType, JsonReader reader)
    {
      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.EndArray:
            return list;
          case JsonToken.Comment:
            break;
          default:
            object value = CreateObject(reader, listItemType, null, null);

            list.Add(value);
            break;
        }
      }

      throw new JsonSerializationException("Unexpected end when deserializing array.");
    }

    private bool IsTypeGenericDictionaryInterface(Type type)
    {
      if (!type.IsGenericType)
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof (IDictionary<,>));
    }

    private bool IsTypeGenericCollectionInterface(Type type)
    {
      if (!type.IsGenericType)
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof (IList<>)
              || genericDefinition == typeof (ICollection<>)
              || genericDefinition == typeof (IEnumerable<>));
    }

    private object CreateAndPopulateObject(JsonReader reader, Type objectType)
    {
      object newObject;

      if (objectType.IsInterface || objectType.IsAbstract)
        throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, objectType));
      
      if (ReflectionUtils.HasDefaultConstructor(objectType))
      {
        newObject = Activator.CreateInstance(objectType);

        PopulateObject(newObject, reader, objectType);
        return newObject;
      }
      
      return CreateObjectFromNonDefaultConstructor(objectType, reader);
    }

    private object CreateObjectFromNonDefaultConstructor(Type objectType, JsonReader reader)
    {
      // object should have a single constructor
      ConstructorInfo c = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).SingleOrDefault();

      if (c == null)
        throw new JsonSerializationException("Could not find a public constructor for type {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));

      // create a dictionary to put retrieved values into
      JsonMemberMappingCollection memberMappings = GetMemberMappings(objectType);
      IDictionary<JsonMemberMapping, object> mappingValues = memberMappings.ToDictionary(kv => kv, kv => (object)null);

      bool exit = false;
      while (!exit && reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            JsonMemberMapping memberMapping;
            // attempt exact case match first
            // then try match ignoring case
            if (memberMappings.TryGetClosestMatchMapping(memberName, out memberMapping))
            {
              if (!memberMapping.Ignored)
              {
                Type memberType = ReflectionUtils.GetMemberUnderlyingType(memberMapping.Member);
                mappingValues[memberMapping] = CreateObject(reader, memberType, null, memberMapping.MemberConverter);
              }
            }
            else
            {
              if (_missingMemberHandling == MissingMemberHandling.Error)
                throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));

              reader.Skip();
            }
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      }

      IDictionary<ParameterInfo, object> constructorParameters = c.GetParameters().ToDictionary(p => p, p => (object)null);
      IDictionary<JsonMemberMapping, object> remainingMappingValues = new Dictionary<JsonMemberMapping, object>();

      foreach (KeyValuePair<JsonMemberMapping, object> mappingValue in mappingValues)
      {
        ParameterInfo matchingConstructorParameter = constructorParameters.ForgivingCaseSensitiveFind(kv => kv.Key.Name, mappingValue.Key.MappingName).Key;
        if (matchingConstructorParameter != null)
          constructorParameters[matchingConstructorParameter] = mappingValue.Value;
        else
          remainingMappingValues.Add(mappingValue);
      }

      object createdObject = ReflectionUtils.CreateInstance(objectType, constructorParameters.Values.ToArray());

      // go through unused values and set the newly created object's properties
      foreach (KeyValuePair<JsonMemberMapping, object> remainingMappingValue in remainingMappingValues)
      {
        if (ShouldSetMappingValue(remainingMappingValue.Key, remainingMappingValue.Value))
          ReflectionUtils.SetMemberValue(remainingMappingValue.Key.Member, createdObject, remainingMappingValue.Value);
      }

      return createdObject;
    }

    private object PopulateObject(object newObject, JsonReader reader, Type objectType)
    {
      JsonMemberMappingCollection memberMappings = GetMemberMappings(objectType);
      Dictionary<string, bool> requiredMappings =
        memberMappings.Where(m => m.Required).ToDictionary(m => m.MappingName, m => false);

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            if (reader.TokenType != JsonToken.Null)
              SetRequiredMapping(memberName, requiredMappings);

            SetObjectMember(reader, newObject, objectType, memberName);
            break;
          case JsonToken.EndObject:
            foreach (KeyValuePair<string, bool> requiredMapping in requiredMappings)
            {
              if (!requiredMapping.Value)
                throw new JsonSerializationException("Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, requiredMapping.Key));
            }
            return newObject;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      }

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private void SetRequiredMapping(string memberName, Dictionary<string, bool> requiredMappings)
    {
      // first attempt to find exact case match
      // then attempt case insensitive match
      if (requiredMappings.ContainsKey(memberName))
      {
        requiredMappings[memberName] = true;
      }
      else
      {
        foreach (KeyValuePair<string, bool> requiredMapping in requiredMappings)
        {
          if (string.Compare(requiredMapping.Key, requiredMapping.Key, StringComparison.OrdinalIgnoreCase) == 0)
          {
            requiredMappings[requiredMapping.Key] = true;
            break;
          }
        }                
      }
    }

    #endregion

    #region Serialize
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
      if (jsonWriter == null)
        throw new ArgumentNullException("jsonWriter");

      if (value is JToken)
        ((JToken)value).WriteTo(jsonWriter, (_converters != null) ? _converters.ToArray() : null);
      else
        SerializeValue(jsonWriter, value, null);
    }


    private void SerializeValue(JsonWriter writer, object value, JsonConverter memberConverter)
    {
      JsonConverter converter;

      if (value == null)
      {
        writer.WriteNull();
      }
      else if (memberConverter != null)
      {
        memberConverter.WriteJson(writer, value);
      }
      else if (HasClassConverter(value.GetType(), out converter))
      {
        converter.WriteJson(writer, value);
      }
      else if (HasMatchingConverter(value.GetType(), out converter))
      {
        converter.WriteJson(writer, value);
      }
      else if (JsonConvert.IsJsonPrimitive(value))
      {
        writer.WriteValue(value);
      }
      else if (value is IList)
      {
        SerializeList(writer, (IList)value);
      }
      else if (value is IDictionary)
      {
        SerializeDictionary(writer, (IDictionary)value);
      }
      else if (value is ICollection)
      {
        SerializeCollection(writer, (ICollection)value);
      }
      else if (value is IEnumerable)
      {
        SerializeEnumerable(writer, (IEnumerable)value);
      }
      else if (value is JsonRaw)
      {
        writer.WriteRawValue(((JsonRaw)value).Content);
      }
      else
      {
        SerializeObject(writer, value);
      }
    }

    private bool HasMatchingConverter(Type type, out JsonConverter matchingConverter)
    {
      return HasMatchingConverter(_converters, type, out matchingConverter);
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

    private void WriteMemberInfoProperty(JsonWriter writer, object value, JsonMemberMapping memberMapping)
    {
      MemberInfo member = memberMapping.Member;
      string propertyName = memberMapping.MappingName;
      JsonConverter memberConverter = memberMapping.MemberConverter;
      object defaultValue = memberMapping.DefaultValue;

      if (!ReflectionUtils.IsIndexedProperty(member))
      {
        object memberValue = ReflectionUtils.GetMemberValue(member, value);

        if (_nullValueHandling == NullValueHandling.Ignore && memberValue == null)
          return;

        if (_defaultValueHandling == DefaultValueHandling.Ignore && object.Equals(memberValue, defaultValue))
          return;

        if (writer.SerializeStack.IndexOf(memberValue) != -1)
        {
          switch (_referenceLoopHandling)
          {
            case ReferenceLoopHandling.Error:
              throw new JsonSerializationException("Self referencing loop");
            case ReferenceLoopHandling.Ignore:
              // return from method
              return;
            case ReferenceLoopHandling.Serialize:
              // continue
              break;
            default:
              throw new InvalidOperationException("Unexpected ReferenceLoopHandling value: '{0}'".FormatWith(CultureInfo.InvariantCulture, _referenceLoopHandling));
          }
        }

        writer.WritePropertyName(propertyName ?? member.Name);
        SerializeValue(writer, memberValue, memberConverter);
      }
    }

    private void SerializeObject(JsonWriter writer, object value)
    {
      Type objectType = value.GetType();

#if !SILVERLIGHT && !PocketPC
      TypeConverter converter = TypeDescriptor.GetConverter(objectType);

      // use the objectType's TypeConverter if it has one and can convert to a string
      if (converter != null && !(converter is ComponentConverter) && (converter.GetType() != typeof(TypeConverter) || value is Type))
      {
        if (converter.CanConvertTo(typeof(string)))
        {
          writer.WriteValue(converter.ConvertToInvariantString(value));
          return;
        }
      }
#else
      if (value is Guid || value is Type)
      {
        writer.WriteValue(value.ToString());
        return;
      }
#endif

      writer.SerializeStack.Add(value);

      writer.WriteStartObject();

      JsonMemberMappingCollection memberMappings = GetMemberMappings(objectType);

      foreach (JsonMemberMapping memberMapping in memberMappings)
      {
        if (!memberMapping.Ignored && memberMapping.Readable)
          WriteMemberInfoProperty(writer, value, memberMapping);
      }

      writer.WriteEndObject();

      writer.SerializeStack.RemoveAt(writer.SerializeStack.Count - 1);
    }

    private void SerializeEnumerable(JsonWriter writer, IEnumerable values)
    {
      SerializeList(writer, values.Cast<object>().ToList());
    }

    private void SerializeCollection(JsonWriter writer, ICollection values)
    {
      SerializeList(writer, values.Cast<object>().ToList());
    }

    private void SerializeList(JsonWriter writer, IList values)
    {
      writer.WriteStartArray();

      for (int i = 0; i < values.Count; i++)
      {
        SerializeValue(writer, values[i], null);
      }

      writer.WriteEndArray();
    }

    private void SerializeDictionary(JsonWriter writer, IDictionary values)
    {
      writer.WriteStartObject();

      foreach (DictionaryEntry entry in values)
      {
        writer.WritePropertyName(entry.Key.ToString());
        SerializeValue(writer, entry.Value, null);
      }

      writer.WriteEndObject();
    }
    #endregion
  }
}