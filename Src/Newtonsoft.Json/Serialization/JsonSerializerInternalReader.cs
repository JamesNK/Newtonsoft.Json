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
using System.Collections.ObjectModel;
#if !(NET35 || NET20 || WINDOWS_PHONE)
using System.Dynamic;
#endif
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerInternalReader : JsonSerializerInternalBase
  {
    private JsonSerializerProxy _internalSerializer;
#if !SILVERLIGHT && !PocketPC && !NETFX_CORE
    private JsonFormatterConverter _formatterConverter;
#endif

    public JsonSerializerInternalReader(JsonSerializer serializer)
      : base(serializer)
    {
    }

    public void Populate(JsonReader reader, object target)
    {
      ValidationUtils.ArgumentNotNull(target, "target");

      Type objectType = target.GetType();

      JsonContract contract = Serializer.ContractResolver.ResolveContract(objectType);

      if (reader.TokenType == JsonToken.None)
        reader.Read();

      if (reader.TokenType == JsonToken.StartArray)
      {
        if (contract.ContractType == JsonContractType.Array)
          PopulateList(CollectionUtils.CreateCollectionWrapper(target), reader, null, (JsonArrayContract) contract);
        else
          throw CreateSerializationException(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }
      else if (reader.TokenType == JsonToken.StartObject)
      {
        CheckedRead(reader);

        string id = null;
        if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
        {
          CheckedRead(reader);
          id = (reader.Value != null) ? reader.Value.ToString() : null;
          CheckedRead(reader);
        }

        if (contract.ContractType == JsonContractType.Dictionary)
          PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader, (JsonDictionaryContract) contract, id);
        else if (contract.ContractType == JsonContractType.Object)
          PopulateObject(target, reader, (JsonObjectContract) contract, id);
        else
          throw CreateSerializationException(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }
      else
      {
        throw CreateSerializationException(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
      }
    }

    private JsonContract GetContractSafe(Type type)
    {
      if (type == null)
        return null;

      return Serializer.ContractResolver.ResolveContract(type);
    }

    public object Deserialize(JsonReader reader, Type objectType)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      JsonContract contract = GetContractSafe(objectType);

      JsonConverter converter = GetConverter(contract, null);

      if (reader.TokenType == JsonToken.None && !ReadForType(reader, contract, converter != null, false))
      {
        if (contract != null && !contract.IsNullable)
          throw new JsonSerializationException("No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

        return null;
      }

      return CreateValueNonProperty(reader, objectType, contract, converter);
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

#if !SILVERLIGHT && !PocketPC && !NETFX_CORE
    private JsonFormatterConverter GetFormatterConverter()
    {
      if (_formatterConverter == null)
        _formatterConverter = new JsonFormatterConverter(GetInternalSerializer());

      return _formatterConverter;
    }
#endif

    private JToken CreateJToken(JsonReader reader, JsonContract contract)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      if (contract != null && contract.UnderlyingType == typeof (JRaw))
      {
        return JRaw.Create(reader);
      }
      else
      {
        JToken token;
        using (JTokenWriter writer = new JTokenWriter())
        {
          writer.WriteToken(reader);
          token = writer.Token;
        }

        return token;
      }
    }

    private JToken CreateJObject(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      // this is needed because we've already read inside the object, looking for special properties
      JToken token;
      using (JTokenWriter writer = new JTokenWriter())
      {
        writer.WriteStartObject();

        if (reader.TokenType == JsonToken.PropertyName)
          writer.WriteToken(reader, reader.Depth - 1);
        else
          writer.WriteEndObject();

        token = writer.Token;
      }

      return token;
    }

    private object CreateValueProperty(JsonReader reader, JsonProperty property, JsonConverter propertyConverter, object target, bool gottenCurrentValue, object currentValue)
    {
      JsonContract contract;
      JsonConverter converter;

      if (property.PropertyContract == null)
        property.PropertyContract = GetContractSafe(property.PropertyType);

      if (currentValue == null)
      {
        contract = property.PropertyContract;
        converter = propertyConverter;
      }
      else
      {
        contract = GetContractSafe(currentValue.GetType());

        if (contract != property.PropertyContract)
          converter = GetConverter(contract, property.MemberConverter);
        else
          converter = propertyConverter;
      }

      Type objectType = property.PropertyType;

      if (converter != null && converter.CanRead)
      {
        if (!gottenCurrentValue && target != null && property.Readable)
          currentValue = property.ValueProvider.GetValue(target);

        return converter.ReadJson(reader, objectType, currentValue, GetInternalSerializer());
      }

      return CreateValueInternal(reader, objectType, contract, property, currentValue);
    }

    private object CreateValueNonProperty(JsonReader reader, Type objectType, JsonContract contract, JsonConverter converter)
    {
      if (converter != null && converter.CanRead)
        return converter.ReadJson(reader, objectType, null, GetInternalSerializer());

      return CreateValueInternal(reader, objectType, contract, null, null);
    }

    private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue)
    {
      if (contract != null && contract.ContractType == JsonContractType.Linq)
        return CreateJToken(reader, contract);

      do
      {
        switch (reader.TokenType)
        {
            // populate a typed object or generic dictionary/array
            // depending upon whether an objectType was supplied
          case JsonToken.StartObject:
            return CreateObject(reader, objectType, contract, member, existingValue);
          case JsonToken.StartArray:
            return CreateList(reader, objectType, contract, member, existingValue, null);
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.Boolean:
          case JsonToken.Date:
          case JsonToken.Bytes:
            return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
          case JsonToken.String:
            // convert empty string to null automatically for nullable types
            if (string.IsNullOrEmpty((string)reader.Value) && objectType != typeof(string) && objectType != typeof(object) && contract.IsNullable)
              return null;

            // string that needs to be returned as a byte array should be base 64 decoded
            if (objectType == typeof (byte[]))
              return Convert.FromBase64String((string) reader.Value);

            return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
          case JsonToken.StartConstructor:
          case JsonToken.EndConstructor:
            string constructorName = reader.Value.ToString();

            return constructorName;
          case JsonToken.Null:
          case JsonToken.Undefined:
#if !NETFX_CORE
            if (objectType == typeof (DBNull))
              return DBNull.Value;
#endif

            return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
          case JsonToken.Raw:
            return new JRaw((string) reader.Value);
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw CreateSerializationException(reader, "Unexpected token while deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw CreateSerializationException(reader, "Unexpected end when deserializing object.");
    }

    private JsonSerializationException CreateSerializationException(JsonReader reader, string message)
    {
      return CreateSerializationException(reader, message, null);
    }

    private JsonSerializationException CreateSerializationException(JsonReader reader, string message, Exception ex)
    {
      return CreateSerializationException(reader as IJsonLineInfo, message, ex);
    }

    private JsonSerializationException CreateSerializationException(IJsonLineInfo lineInfo, string message, Exception ex)
    {
      message = JsonReader.FormatExceptionMessage(lineInfo, message);

      return new JsonSerializationException(message, ex);
    }

    private JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter)
    {
      JsonConverter converter = null;
      if (memberConverter != null)
      {
        // member attribute converter
        converter = memberConverter;
      }
      else if (contract != null)
      {
        JsonConverter matchingConverter;
        if (contract.Converter != null)
          // class attribute converter
          converter = contract.Converter;
        else if ((matchingConverter = Serializer.GetMatchingConverter(contract.UnderlyingType)) != null)
          // passed in converters
          converter = matchingConverter;
        else if (contract.InternalConverter != null)
          // internally specified converter
          converter = contract.InternalConverter;
      }
      return converter;
    }

    private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue)
    {
      CheckedRead(reader);

      string id = null;

      if (reader.TokenType == JsonToken.PropertyName)
      {
        string propertyName = reader.Value.ToString();

        if (propertyName.Length > 0 && propertyName[0] == '$')
        {
          // read 'special' properties
          // $type, $id, $ref, etc
          bool specialProperty;

          do
          {
            propertyName = reader.Value.ToString();

            if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
            {
              CheckedRead(reader);
              if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
                throw CreateSerializationException(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

              string reference = (reader.Value != null) ? reader.Value.ToString() : null;

              CheckedRead(reader);

              if (reference != null)
              {
                if (reader.TokenType == JsonToken.PropertyName)
                  throw CreateSerializationException(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

                return Serializer.ReferenceResolver.ResolveReference(this, reference);
              }
              else
              {
                specialProperty = true;
              }
            }
            else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
            {
              CheckedRead(reader);
              string qualifiedTypeName = reader.Value.ToString();

              if ((((member != null) ? member.TypeNameHandling : null) ?? Serializer.TypeNameHandling) != TypeNameHandling.None)
              {
                string typeName;
                string assemblyName;
                ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName, out typeName, out assemblyName);

                Type specifiedType;
                try
                {
                  specifiedType = Serializer.Binder.BindToType(assemblyName, typeName);
                }
                catch (Exception ex)
                {
                  throw CreateSerializationException(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
                }

                if (specifiedType == null)
                  throw CreateSerializationException(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));

                if (objectType != null && !objectType.IsAssignableFrom(specifiedType))
                  throw CreateSerializationException(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));

                objectType = specifiedType;
                contract = GetContractSafe(specifiedType);
              }

              CheckedRead(reader);

              specialProperty = true;
            }
            else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
            {
              CheckedRead(reader);

              id = (reader.Value != null) ? reader.Value.ToString() : null;

              CheckedRead(reader);
              specialProperty = true;
            }
            else if (string.Equals(propertyName, JsonTypeReflector.ArrayValuesPropertyName, StringComparison.Ordinal))
            {
              CheckedRead(reader);
              object list = CreateList(reader, objectType, contract, member, existingValue, id);
              CheckedRead(reader);
              return list;
            }
            else
            {
              specialProperty = false;
            }
          } while (specialProperty
                   && reader.TokenType == JsonToken.PropertyName);
        }
      }

      if (!HasDefinedType(objectType))
        return CreateJObject(reader);

      if (contract == null)
        throw CreateSerializationException(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      switch (contract.ContractType)
      {
        case JsonContractType.Object:
          JsonObjectContract objectContract = (JsonObjectContract) contract;
          if (existingValue == null)
            return CreateAndPopulateObject(reader, objectContract, id);

          return PopulateObject(existingValue, reader, objectContract, id);
        case JsonContractType.Primitive:
          JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract) contract;
          // if the content is inside $value then read past it
          if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);
            object value = CreateValueInternal(reader, objectType, primitiveContract, member, existingValue);

            CheckedRead(reader);
            return value;
          }
          break;
        case JsonContractType.Dictionary:
          JsonDictionaryContract dictionaryContract = (JsonDictionaryContract) contract;
          if (existingValue == null)
            return CreateAndPopulateDictionary(reader, dictionaryContract, id);

          return PopulateDictionary(dictionaryContract.CreateWrapper(existingValue), reader, dictionaryContract, id);
#if !(NET35 || NET20 || WINDOWS_PHONE)
        case JsonContractType.Dynamic:
          JsonDynamicContract dynamicContract = (JsonDynamicContract) contract;
          return CreateDynamic(reader, dynamicContract, id);
#endif
#if !SILVERLIGHT && !PocketPC && !NETFX_CORE
        case JsonContractType.Serializable:
          JsonISerializableContract serializableContract = (JsonISerializableContract) contract;
          return CreateISerializable(reader, serializableContract, id);
#endif
      }

      throw CreateSerializationException(reader, @"Cannot deserialize JSON object (i.e. {{""name"":""value""}}) into type '{0}'.
The deserialized type should be a normal .NET type (i.e. not a primitive type like integer, not a collection type like an array or List<T>) or a dictionary type (i.e. Dictionary<TKey, TValue>).
To force JSON objects to deserialize add the JsonObjectAttribute to the type.".FormatWith(CultureInfo.InvariantCulture, objectType));
    }

    private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
    {
      if (contract == null)
        throw CreateSerializationException(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      JsonArrayContract arrayContract = contract as JsonArrayContract;
      if (arrayContract == null)
        throw CreateSerializationException(reader, @"Cannot deserialize JSON array (i.e. [1,2,3]) into type '{0}'.
The deserialized type must be an array or implement a collection interface like IEnumerable, ICollection or IList.
To force JSON arrays to deserialize add the JsonArrayAttribute to the type.".FormatWith(CultureInfo.InvariantCulture, objectType));

      return arrayContract;
    }

    private void CheckedRead(JsonReader reader)
    {
      if (!reader.Read())
        throw CreateSerializationException(reader, "Unexpected end when deserializing object.");
    }

    private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string reference)
    {
      object value;
      if (HasDefinedType(objectType))
      {
        JsonArrayContract arrayContract = EnsureArrayContract(reader, objectType, contract);

        if (existingValue == null)
          value = CreateAndPopulateList(reader, reference, arrayContract);
        else
          value = PopulateList(arrayContract.CreateWrapper(existingValue), reader, reference, arrayContract);
      }
      else
      {
        value = CreateJToken(reader, contract);
      }
      return value;
    }

    private bool HasDefinedType(Type type)
    {
      return (type != null && type != typeof (object) && !typeof (JToken).IsSubclassOf(type)
#if !(NET35 || NET20 || WINDOWS_PHONE)
        && type != typeof (IDynamicMetaObjectProvider)
#endif
        );
    }

    private object EnsureType(JsonReader reader, object value, CultureInfo culture, JsonContract contract, Type targetType)
    {
      if (targetType == null)
        return value;

      Type valueType = ReflectionUtils.GetObjectType(value);

      // type of value and type of target don't match
      // attempt to convert value's type to target's type
      if (valueType != targetType)
      {
        try
        {
          if (value == null && contract.IsNullable)
            return null;

          if (contract.IsConvertable)
          {
            if (contract.NonNullableUnderlyingType.IsEnum())
              {
                if (value is string)
                  return Enum.Parse(contract.NonNullableUnderlyingType, value.ToString(), true);
                else if (ConvertUtils.IsInteger(value))
                  return Enum.ToObject(contract.NonNullableUnderlyingType, value);
              }

              return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
          }

          return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
        }
        catch (Exception ex)
        {
          throw CreateSerializationException(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, FormatValueForPrint(value), targetType), ex);
        }
      }

      return value;
    }

    private string FormatValueForPrint(object value)
    {
      if (value == null)
        return "{null}";

      if (value is string)
        return @"""" + value + @"""";

      return value.ToString();
    }

    private void SetPropertyValue(JsonProperty property, JsonConverter propertyConverter, JsonReader reader, object target)
    {
      if (property.Ignored)
      {
        reader.Skip();
        return;
      }

      object currentValue = null;
      bool useExistingValue = false;
      bool gottenCurrentValue = false;

      ObjectCreationHandling objectCreationHandling =
        property.ObjectCreationHandling.GetValueOrDefault(Serializer.ObjectCreationHandling);

      if ((objectCreationHandling == ObjectCreationHandling.Auto || objectCreationHandling == ObjectCreationHandling.Reuse)
        && (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject)
          && property.Readable)
      {
        currentValue = property.ValueProvider.GetValue(target);
        gottenCurrentValue = true;

        useExistingValue = (currentValue != null
          && !property.PropertyType.IsArray
            && !ReflectionUtils.InheritsGenericDefinition(property.PropertyType, typeof (ReadOnlyCollection<>))
              && !property.PropertyType.IsValueType());
      }

      if (!property.Writable && !useExistingValue)
      {
        reader.Skip();
        return;
      }

      // test tokentype here because null might not be convertable to some types, e.g. ignoring null when applied to DateTime
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore && reader.TokenType == JsonToken.Null)
      {
        reader.Skip();
        return;
      }

      // test tokentype here because default value might not be convertable to actual type, e.g. default of "" for DateTime
      if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
        && JsonReader.IsPrimitiveToken(reader.TokenType)
          && MiscellaneousUtils.ValueEquals(reader.Value, property.DefaultValue))
      {
        reader.Skip();
        return;
      }

      object existingValue = (useExistingValue) ? currentValue : null;
      object value = CreateValueProperty(reader, property, propertyConverter, target, gottenCurrentValue, existingValue);

      // always set the value if useExistingValue is false,
      // otherwise also set it if CreateValue returns a new value compared to the currentValue
      // this could happen because of a JsonConverter against the type
      if ((!useExistingValue || value != currentValue)
        && ShouldSetPropertyValue(property, value))
      {
        property.ValueProvider.SetValue(target, value);

        if (property.SetIsSpecified != null)
          property.SetIsSpecified(target, true);
      }
    }

    private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
    {
      return ((value & flag) == flag);
    }

    private bool ShouldSetPropertyValue(JsonProperty property, object value)
    {
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
        return false;

      if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
        && MiscellaneousUtils.ValueEquals(value, property.DefaultValue))
        return false;

      if (!property.Writable)
        return false;

      return true;
    }

    private object CreateAndPopulateDictionary(JsonReader reader, JsonDictionaryContract contract, string id)
    {
      object dictionary;

      if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        dictionary = contract.DefaultCreator();
      else
        throw CreateSerializationException(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      IWrappedDictionary dictionaryWrapper = contract.CreateWrapper(dictionary);

      PopulateDictionary(dictionaryWrapper, reader, contract, id);

      return dictionaryWrapper.UnderlyingDictionary;
    }

    private object PopulateDictionary(IWrappedDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, string id)
    {
      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, dictionary.UnderlyingDictionary);

      contract.InvokeOnDeserializing(dictionary.UnderlyingDictionary, Serializer.Context);

      int initialDepth = reader.Depth;

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            object keyValue = reader.Value;
            try
            {
              if (contract.DictionaryKeyContract == null)
                contract.DictionaryKeyContract = GetContractSafe(contract.DictionaryKeyType);
              
              try
              {
                keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.DictionaryKeyContract, contract.DictionaryKeyType);
              }
              catch (Exception ex)
              {
                throw CreateSerializationException(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
              }

              if (contract.DictionaryValueContract == null)
                contract.DictionaryValueContract = GetContractSafe(contract.DictionaryValueType);

              JsonConverter dictionaryValueConverter = GetConverter(contract.DictionaryValueContract, null);

              if (!ReadForType(reader, contract.DictionaryValueContract, dictionaryValueConverter != null, false))
                throw CreateSerializationException(reader, "Unexpected end when deserializing object.");

              dictionary[keyValue] = CreateValueNonProperty(reader, contract.DictionaryValueType, contract.DictionaryValueContract, dictionaryValueConverter);
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(dictionary, contract, keyValue, reader.Path, ex))
                HandleError(reader, initialDepth);
              else
                throw;
            }
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            contract.InvokeOnDeserialized(dictionary.UnderlyingDictionary, Serializer.Context);

            return dictionary.UnderlyingDictionary;
          default:
            throw CreateSerializationException(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw CreateSerializationException(reader, "Unexpected end when deserializing object.");
    }

    private object CreateAndPopulateList(JsonReader reader, string reference, JsonArrayContract contract)
    {
      return CollectionUtils.CreateAndPopulateList(contract.CreatedType, (l, isTemporaryListReference) =>
        {
          if (reference != null && isTemporaryListReference)
            throw CreateSerializationException(reader, "Cannot preserve reference to array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

#if !PocketPC
          if (contract.OnSerializing != null && isTemporaryListReference)
            throw CreateSerializationException(reader, "Cannot call OnSerializing on an array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
#endif
          if (contract.OnError != null && isTemporaryListReference)
            throw CreateSerializationException(reader, "Cannot call OnError on an array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          PopulateList(contract.CreateWrapper(l), reader, reference, contract);
        });
    }

    private object PopulateList(IWrappedCollection wrappedList, JsonReader reader, string reference, JsonArrayContract contract)
    {
      object list = wrappedList.UnderlyingCollection;

      // can't populate an existing array
      if (wrappedList.IsFixedSize)
      {
        reader.Skip();
        return wrappedList.UnderlyingCollection;
      }

      if (reference != null)
        Serializer.ReferenceResolver.AddReference(this, reference, list);

      contract.InvokeOnDeserializing(list, Serializer.Context);

      int initialDepth = reader.Depth;
      int index = 0;

      JsonContract collectionItemContract = GetContractSafe(contract.CollectionItemType);
      JsonConverter collectionItemConverter = GetConverter(collectionItemContract, null);

      while (true)
      {
        try
        {
          if (ReadForType(reader, collectionItemContract, collectionItemConverter != null, true))
          {
            switch (reader.TokenType)
            {
              case JsonToken.EndArray:
                contract.InvokeOnDeserialized(list, Serializer.Context);

                return wrappedList.UnderlyingCollection;
              case JsonToken.Comment:
                break;
              default:
                object value = CreateValueNonProperty(reader, contract.CollectionItemType, collectionItemContract, collectionItemConverter);

                wrappedList.Add(value);
                break;
            }
          }
          else
          {
            break;
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(list, contract, index, reader.Path, ex))
            HandleError(reader, initialDepth);
          else
            throw;
        }
        finally
        {
          index++;
        }
      }

      throw CreateSerializationException(reader, "Unexpected end when deserializing array.");
    }

#if !SILVERLIGHT && !PocketPC && !NETFX_CORE
    private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, string id)
    {
      Type objectType = contract.UnderlyingType;

      if (!JsonTypeReflector.FullyTrusted)
      {
        throw new JsonSerializationException(@"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data.
To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add to JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, GetFormatterConverter());

      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw CreateSerializationException(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw CreateSerializationException(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      if (contract.ISerializableCreator == null)
        throw CreateSerializationException(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, objectType));

      object createdObject = contract.ISerializableCreator(serializationInfo, Serializer.Context);

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, createdObject);

      // these are together because OnDeserializing takes an object but for an ISerializable the object is full created in the constructor
      contract.InvokeOnDeserializing(createdObject, Serializer.Context);
      contract.InvokeOnDeserialized(createdObject, Serializer.Context);

      return createdObject;
    }
#endif

#if !(NET35 || NET20 || WINDOWS_PHONE)
    private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, string id)
    {
      IDynamicMetaObjectProvider newObject = null;

      if (contract.UnderlyingType.IsInterface() || contract.UnderlyingType.IsAbstract())
        throw CreateSerializationException(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        newObject = (IDynamicMetaObjectProvider) contract.DefaultCreator();
      else
        throw CreateSerializationException(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, newObject);

      contract.InvokeOnDeserializing(newObject, Serializer.Context);

      int initialDepth = reader.Depth;

      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            try
            {
              if (!reader.Read())
                throw CreateSerializationException(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              // first attempt to find a settable property, otherwise fall back to a dynamic set without type
              JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

              if (property != null && property.Writable && !property.Ignored)
              {
                if (property.PropertyContract == null)
                  property.PropertyContract = GetContractSafe(property.PropertyType);

                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter);

                SetPropertyValue(property, propertyConverter, reader, newObject);
              }
              else
              {
                Type t = (JsonReader.IsPrimitiveToken(reader.TokenType)) ? reader.ValueType : typeof (IDynamicMetaObjectProvider);

                JsonContract dynamicMemberContract = GetContractSafe(t);
                JsonConverter dynamicMemberConverter = GetConverter(dynamicMemberContract, null);

                object value = CreateValueNonProperty(reader, t, dynamicMemberContract, dynamicMemberConverter);

                newObject.TrySetMember(memberName, value);
              }
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(newObject, contract, memberName, reader.Path, ex))
                HandleError(reader, initialDepth);
              else
                throw;
            }
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw CreateSerializationException(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      contract.InvokeOnDeserialized(newObject, Serializer.Context);

      return newObject;
    }
#endif

    private object CreateAndPopulateObject(JsonReader reader, JsonObjectContract contract, string id)
    {
      object newObject = null;

      if (contract.UnderlyingType.IsInterface() || contract.UnderlyingType.IsAbstract())
        throw CreateSerializationException(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.OverrideConstructor != null)
      {
        if (contract.OverrideConstructor.GetParameters().Length > 0)
          return CreateObjectFromNonDefaultConstructor(reader, contract, contract.OverrideConstructor, id);

        newObject = contract.OverrideConstructor.Invoke(null);
      }
      else if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || contract.ParametrizedConstructor == null))
      {
        // use the default constructor if it is...
        // public
        // non-public and the user has change constructor handling settings
        // non-public and there is no other constructor
        newObject = contract.DefaultCreator();
      }
      else if (contract.ParametrizedConstructor != null)
      {
        return CreateObjectFromNonDefaultConstructor(reader, contract, contract.ParametrizedConstructor, id);
      }

      if (newObject == null)
        throw CreateSerializationException(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      PopulateObject(newObject, reader, contract, id);
      return newObject;
    }

    private object CreateObjectFromNonDefaultConstructor(JsonReader reader, JsonObjectContract contract, ConstructorInfo constructorInfo, string id)
    {
      ValidationUtils.ArgumentNotNull(constructorInfo, "constructorInfo");

      Type objectType = contract.UnderlyingType;

      IDictionary<JsonProperty, object> propertyValues = ResolvePropertyAndConstructorValues(contract, reader, objectType);

      IDictionary<ParameterInfo, object> constructorParameters = constructorInfo.GetParameters().ToDictionary(p => p, p => (object) null);
      IDictionary<JsonProperty, object> remainingPropertyValues = new Dictionary<JsonProperty, object>();

      foreach (KeyValuePair<JsonProperty, object> propertyValue in propertyValues)
      {
        ParameterInfo matchingConstructorParameter = constructorParameters.ForgivingCaseSensitiveFind(kv => kv.Key.Name, propertyValue.Key.UnderlyingName).Key;
        if (matchingConstructorParameter != null)
          constructorParameters[matchingConstructorParameter] = propertyValue.Value;
        else
          remainingPropertyValues.Add(propertyValue);
      }

      object createdObject = constructorInfo.Invoke(constructorParameters.Values.ToArray());

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, createdObject);

      contract.InvokeOnDeserializing(createdObject, Serializer.Context);

      // go through unused values and set the newly created object's properties
      foreach (KeyValuePair<JsonProperty, object> remainingPropertyValue in remainingPropertyValues)
      {
        JsonProperty property = remainingPropertyValue.Key;
        object value = remainingPropertyValue.Value;

        if (ShouldSetPropertyValue(remainingPropertyValue.Key, remainingPropertyValue.Value))
        {
          property.ValueProvider.SetValue(createdObject, value);
        }
        else if (!property.Writable && value != null)
        {
          // handle readonly collection/dictionary properties
          JsonContract propertyContract = Serializer.ContractResolver.ResolveContract(property.PropertyType);

          if (propertyContract.ContractType == JsonContractType.Array)
          {
            JsonArrayContract propertyArrayContract = propertyContract as JsonArrayContract;

            object createdObjectCollection = property.ValueProvider.GetValue(createdObject);
            if (createdObjectCollection != null)
            {
              IWrappedCollection createdObjectCollectionWrapper = propertyArrayContract.CreateWrapper(createdObjectCollection);
              IWrappedCollection newValues = propertyArrayContract.CreateWrapper(value);

              foreach (object newValue in newValues)
              {
                createdObjectCollectionWrapper.Add(newValue);
              }
            }
          }
          else if (propertyContract.ContractType == JsonContractType.Dictionary)
          {
            JsonDictionaryContract jsonDictionaryContract = propertyContract as JsonDictionaryContract;

            object createdObjectDictionary = property.ValueProvider.GetValue(createdObject);
            if (createdObjectDictionary != null)
            {
              IWrappedDictionary createdObjectDictionaryWrapper = jsonDictionaryContract.CreateWrapper(createdObjectDictionary);
              IWrappedDictionary newValues = jsonDictionaryContract.CreateWrapper(value);

              foreach (DictionaryEntry newValue in newValues)
              {
                createdObjectDictionaryWrapper.Add(newValue.Key, newValue.Value);
              }
            }
          }
        }
      }

      contract.InvokeOnDeserialized(createdObject, Serializer.Context);
      return createdObject;
    }

    private IDictionary<JsonProperty, object> ResolvePropertyAndConstructorValues(JsonObjectContract contract, JsonReader reader, Type objectType)
    {
      IDictionary<JsonProperty, object> propertyValues = new Dictionary<JsonProperty, object>();
      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            // attempt exact case match first
            // then try match ignoring case
            JsonProperty property = contract.ConstructorParameters.GetClosestMatchProperty(memberName) ??
              contract.Properties.GetClosestMatchProperty(memberName);

            if (property != null)
            {
              if (property.PropertyContract == null)
                property.PropertyContract = GetContractSafe(property.PropertyType);

              JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter);

              if (!ReadForType(reader, property.PropertyContract, propertyConverter != null, false))
                throw CreateSerializationException(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (!property.Ignored)
                propertyValues[property] = CreateValueProperty(reader, property, propertyConverter, null, true, null);
              else
                reader.Skip();
            }
            else
            {
              if (!reader.Read())
                throw CreateSerializationException(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                throw CreateSerializationException(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));

              reader.Skip();
            }
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw CreateSerializationException(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      return propertyValues;
    }

    private bool ReadForType(JsonReader reader, JsonContract contract, bool hasConverter, bool inArray)
    {
      // don't read properties with converters as a specific value
      // the value might be a string which will then get converted which will error if read as date for example
      if (hasConverter)
        return reader.Read();

      ReadType t = (contract != null) ? contract.InternalReadType : ReadType.Read;

      switch (t)
      {
        case ReadType.Read:
          do
          {
            if (!reader.Read())
              return false;
          } while (reader.TokenType == JsonToken.Comment);

          return true;
        case ReadType.ReadAsInt32:
          reader.ReadAsInt32();
          break;
        case ReadType.ReadAsDecimal:
          reader.ReadAsDecimal();
          break;
        case ReadType.ReadAsBytes:
          reader.ReadAsBytes();
          break;
        case ReadType.ReadAsString:
          reader.ReadAsString();
          break;
        case ReadType.ReadAsDateTime:
          reader.ReadAsDateTime();
          break;
#if !NET20
        case ReadType.ReadAsDateTimeOffset:
          reader.ReadAsDateTimeOffset();
          break;
#endif
        default:
          throw new ArgumentOutOfRangeException();
      }

      return (reader.TokenType != JsonToken.None);
    }

    private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, string id)
    {
      contract.InvokeOnDeserializing(newObject, Serializer.Context);

      Dictionary<JsonProperty, PropertyPresence> propertiesPresence =
        contract.Properties.ToDictionary(m => m, m => PropertyPresence.None);

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, newObject);

      int initialDepth = reader.Depth;

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            {
              string memberName = reader.Value.ToString();

              try
              {
                // attempt exact case match first
                // then try match ignoring case
                JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                if (property == null)
                {
                  if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                    throw CreateSerializationException(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));

                  reader.Skip();
                  continue;
                }

                if (property.PropertyContract == null)
                  property.PropertyContract = GetContractSafe(property.PropertyType);

                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter);

                if (!ReadForType(reader, property.PropertyContract, propertyConverter != null, false))
                  throw CreateSerializationException(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

                SetPropertyPresence(reader, property, propertiesPresence);

                SetPropertyValue(property, propertyConverter, reader, newObject);
              }
              catch (Exception ex)
              {
                if (IsErrorHandled(newObject, contract, memberName, reader.Path, ex))
                  HandleError(reader, initialDepth);
                else
                  throw;
              }
            }
            break;
          case JsonToken.EndObject:
            {
              foreach (KeyValuePair<JsonProperty, PropertyPresence> propertyPresence in propertiesPresence)
              {
                JsonProperty property = propertyPresence.Key;
                PropertyPresence presence = propertyPresence.Value;

                if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
                {
                  try
                  {
                    switch (presence)
                    {
                      case PropertyPresence.None:
                        if (property.Required == Required.AllowNull || property.Required == Required.Always)
                          throw CreateSerializationException(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));

                        if (property.PropertyContract == null)
                          property.PropertyContract = GetContractSafe(property.PropertyType);

                        if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                            && property.Writable)
                          property.ValueProvider.SetValue(newObject, EnsureType(reader, property.DefaultValue, CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
                        break;
                      case PropertyPresence.Null:
                        if (property.Required == Required.Always)
                          throw CreateSerializationException(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                        break;
                    }
                  }
                  catch (Exception ex)
                  {
                    if (IsErrorHandled(newObject, contract, property.PropertyName, reader.Path, ex))
                      HandleError(reader, initialDepth);
                    else
                      throw;
                  }
                }
              }

              contract.InvokeOnDeserialized(newObject, Serializer.Context);
              return newObject;
            }
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw CreateSerializationException(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw CreateSerializationException(reader, "Unexpected end when deserializing object.");
    }

    private void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence> requiredProperties)
    {
      if (property != null)
      {
        requiredProperties[property] = (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
          ? PropertyPresence.Null
          : PropertyPresence.Value;
      }
    }

    private void HandleError(JsonReader reader, int initialDepth)
    {
      ClearErrorContext();

      reader.Skip();

      while (reader.Depth > (initialDepth + 1))
      {
        reader.Read();
      }
    }

    internal enum PropertyPresence
    {
      None,
      Null,
      Value
    }
  }
}