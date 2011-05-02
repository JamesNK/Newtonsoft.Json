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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerInternalReader : JsonSerializerInternalBase
  {
    private JsonSerializerProxy _internalSerializer;
#if !SILVERLIGHT && !PocketPC
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
        if (contract is JsonArrayContract)
          PopulateList(CollectionUtils.CreateCollectionWrapper(target), reader, null, (JsonArrayContract)contract);
        else
          throw new JsonSerializationException("Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }
      else if (reader.TokenType == JsonToken.StartObject)
      {
        CheckedRead(reader);

        string id = null;
        if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
        {
          CheckedRead(reader);
          id = reader.Value.ToString();
          CheckedRead(reader);
        }

        if (contract is JsonDictionaryContract)
          PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader, (JsonDictionaryContract)contract, id);
        else if (contract is JsonObjectContract)
          PopulateObject(target, reader, (JsonObjectContract)contract, id);
        else
          throw new JsonSerializationException("Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }
      else
      {
        throw new JsonSerializationException("Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
      }
    }

    private JsonContract GetContractSafe(Type type)
    {
      if (type == null)
        return null;

      return Serializer.ContractResolver.ResolveContract(type);
    }

    private JsonContract GetContractSafe(Type type, object value)
    {
      if (value == null)
        return GetContractSafe(type);

      return Serializer.ContractResolver.ResolveContract(value.GetType());
    }

    public object Deserialize(JsonReader reader, Type objectType)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      if (reader.TokenType == JsonToken.None && !ReadForType(reader, objectType, null))
        return null;

      return CreateValueNonProperty(reader, objectType, GetContractSafe(objectType));
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

#if !SILVERLIGHT && !PocketPC
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

      if (contract != null && contract.UnderlyingType == typeof(JRaw))
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

    private object CreateValueProperty(JsonReader reader, JsonProperty property, object target, bool gottenCurrentValue, object currentValue)
    {
      JsonContract contract = GetContractSafe(property.PropertyType, currentValue);
      Type objectType = property.PropertyType;

      JsonConverter converter = GetConverter(contract, property.MemberConverter);

      if (converter != null && converter.CanRead)
      {
        if (!gottenCurrentValue && target != null && property.Readable)
          currentValue = property.ValueProvider.GetValue(target);

        return converter.ReadJson(reader, objectType, currentValue, GetInternalSerializer());
      }

      return CreateValueInternal(reader, objectType, contract, property, currentValue);
    }

    private object CreateValueNonProperty(JsonReader reader, Type objectType, JsonContract contract)
    {
      JsonConverter converter = GetConverter(contract, null);

      if (converter != null && converter.CanRead)
        return converter.ReadJson(reader, objectType, null, GetInternalSerializer());

      return CreateValueInternal(reader, objectType, contract, null, null);
    }

    private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue)
    {
      if (contract is JsonLinqContract)
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
            return EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
          case JsonToken.String:
            // convert empty string to null automatically for nullable types
            if (string.IsNullOrEmpty((string)reader.Value) &&
              objectType != null &&
              ReflectionUtils.IsNullableType(objectType))
              return null;

            // string that needs to be returned as a byte array should be base 64 decoded
            if (objectType == typeof(byte[]))
              return Convert.FromBase64String((string)reader.Value);

            return EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
          case JsonToken.StartConstructor:
          case JsonToken.EndConstructor:
            string constructorName = reader.Value.ToString();

            return constructorName;
          case JsonToken.Null:
          case JsonToken.Undefined:
            if (objectType == typeof(DBNull))
              return DBNull.Value;

            return EnsureType(reader.Value, CultureInfo.InvariantCulture, objectType);
          case JsonToken.Raw:
            return new JRaw((string)reader.Value);
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw new JsonSerializationException("Unexpected token while deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
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
        bool specialProperty;

        do
        {
          string propertyName = reader.Value.ToString();

          if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);
            if (reader.TokenType != JsonToken.String)
              throw new JsonSerializationException("JSON reference {0} property must have a string value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

            string reference = reader.Value.ToString();

            CheckedRead(reader);
            if (reader.TokenType == JsonToken.PropertyName)
              throw new JsonSerializationException("Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

            return Serializer.ReferenceResolver.ResolveReference(this, reference);
          }
          else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);
            string qualifiedTypeName = reader.Value.ToString();

            CheckedRead(reader);

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
                throw new JsonSerializationException("Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
              }

              if (specifiedType == null)
                throw new JsonSerializationException("Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));

              if (objectType != null && !objectType.IsAssignableFrom(specifiedType))
                throw new JsonSerializationException("Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));

              objectType = specifiedType;
              contract = GetContractSafe(specifiedType);
            }
            specialProperty = true;
          }
          else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);

            id = reader.Value.ToString();
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

      if (!HasDefinedType(objectType))
        return CreateJObject(reader);

      if (contract == null)
        throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      JsonDictionaryContract dictionaryContract = contract as JsonDictionaryContract;
      if (dictionaryContract != null)
      {
        if (existingValue == null)
          return CreateAndPopulateDictionary(reader, dictionaryContract, id);

        return PopulateDictionary(dictionaryContract.CreateWrapper(existingValue), reader, dictionaryContract, id);
      }

      JsonObjectContract objectContract = contract as JsonObjectContract;
      if (objectContract != null)
      {
        if (existingValue == null)
          return CreateAndPopulateObject(reader, objectContract, id);

        return PopulateObject(existingValue, reader, objectContract, id);
      }

#if !SILVERLIGHT && !PocketPC
      JsonISerializableContract serializableContract = contract as JsonISerializableContract;
      if (serializableContract != null)
      {
        return CreateISerializable(reader, serializableContract, id);
      }
#endif

#if !(NET35 || NET20 || WINDOWS_PHONE)
      JsonDynamicContract dynamicContract = contract as JsonDynamicContract;
      if (dynamicContract != null)
      {
        return CreateDynamic(reader, dynamicContract, id);
      }
#endif

      throw new JsonSerializationException("Cannot deserialize JSON object into type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
    }

    private JsonArrayContract EnsureArrayContract(Type objectType, JsonContract contract)
    {
      if (contract == null)
        throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      JsonArrayContract arrayContract = contract as JsonArrayContract;
      if (arrayContract == null)
        throw new JsonSerializationException("Cannot deserialize JSON array into type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));

      return arrayContract;
    }

    private void CheckedRead(JsonReader reader)
    {
      if (!reader.Read())
        throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string reference)
    {
      object value;
      if (HasDefinedType(objectType))
      {
        JsonArrayContract arrayContract = EnsureArrayContract(objectType, contract);

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
      return (type != null && type != typeof(object) && !typeof(JToken).IsAssignableFrom(type)
#if !(NET35 || NET20 || WINDOWS_PHONE)
 && type != typeof(IDynamicMetaObjectProvider)
#endif
);
    }

    private object EnsureType(object value, CultureInfo culture, Type targetType)
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
          return ConvertUtils.ConvertOrCast(value, culture, targetType);
        }
        catch (Exception ex)
        {
          throw new JsonSerializationException("Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, FormatValueForPrint(value), targetType), ex);
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

    private void SetPropertyValue(JsonProperty property, JsonReader reader, object target)
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
          && !ReflectionUtils.InheritsGenericDefinition(property.PropertyType, typeof(ReadOnlyCollection<>))
          && !property.PropertyType.IsValueType);
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
      if (property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling) == DefaultValueHandling.Ignore
        && JsonReader.IsPrimitiveToken(reader.TokenType)
        && Equals(reader.Value, property.DefaultValue))
      {
        reader.Skip();
        return;
      }

      object existingValue = (useExistingValue) ? currentValue : null;
      object value = CreateValueProperty(reader, property, target, gottenCurrentValue, existingValue);

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

    private bool ShouldSetPropertyValue(JsonProperty property, object value)
    {
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
        return false;

      if (property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && MiscellaneousUtils.ValueEquals(value, property.DefaultValue))
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
        throw new JsonSerializationException("Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

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
            object keyValue;
            try
            {
              keyValue = EnsureType(reader.Value, CultureInfo.InvariantCulture, contract.DictionaryKeyType);
            }
            catch (Exception ex)
            {
              throw new JsonSerializationException("Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
            }

            if (!ReadForType(reader, contract.DictionaryValueType, null))
              throw new JsonSerializationException("Unexpected end when deserializing object.");

            try
            {
              dictionary[keyValue] = CreateValueNonProperty(reader, contract.DictionaryValueType, GetContractSafe(contract.DictionaryValueType));
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(dictionary, contract, keyValue, ex))
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
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateAndPopulateList(JsonReader reader, string reference, JsonArrayContract contract)
    {
      return CollectionUtils.CreateAndPopulateList(contract.CreatedType, (l, isTemporaryListReference) =>
        {
          if (reference != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot preserve reference to array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

#if !PocketPC
          if (contract.OnSerializing != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot call OnSerializing on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
#endif
          if (contract.OnError != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot call OnError on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          PopulateList(contract.CreateWrapper(l), reader, reference, contract);
        });
    }

    private bool ReadForTypeArrayHack(JsonReader reader, Type t)
    {
      // this is a nasty hack because calling ReadAsDecimal for example will error when we hit the end of the array
      // need to think of a better way of doing this
      try
      {
        return ReadForType(reader, t, null);
      }
      catch (JsonReaderException)
      {
        if (reader.TokenType == JsonToken.EndArray)
          return true;

        throw;
      }
    }

    private object PopulateList(IWrappedCollection wrappedList, JsonReader reader, string reference, JsonArrayContract contract)
    {
      object list = wrappedList.UnderlyingCollection;

      if (reference != null)
        Serializer.ReferenceResolver.AddReference(this, reference, list);

      contract.InvokeOnDeserializing(list, Serializer.Context);

      int initialDepth = reader.Depth;

      while (ReadForTypeArrayHack(reader, contract.CollectionItemType))
      {
        switch (reader.TokenType)
        {
          case JsonToken.EndArray:
            contract.InvokeOnDeserialized(list, Serializer.Context);

            return wrappedList.UnderlyingCollection;
          case JsonToken.Comment:
            break;
          default:
            try
            {
              object value = CreateValueNonProperty(reader, contract.CollectionItemType, GetContractSafe(contract.CollectionItemType));

              wrappedList.Add(value);
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(list, contract, wrappedList.Count, ex))
                HandleError(reader, initialDepth);
              else
                throw;
            }
            break;
        }
      }

      throw new JsonSerializationException("Unexpected end when deserializing array.");
    }

#if !SILVERLIGHT && !PocketPC
    private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, string id)
    {
      Type objectType = contract.UnderlyingType;

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, GetFormatterConverter());

      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      if (contract.ISerializableCreator == null)
        throw new JsonSerializationException("ISerializable type '{0}' does not have a valid constructor.".FormatWith(CultureInfo.InvariantCulture, objectType));

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

      if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
        throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        newObject = (IDynamicMetaObjectProvider)contract.DefaultCreator();
      else
        throw new JsonSerializationException("Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, newObject);

      contract.InvokeOnDeserializing(newObject, Serializer.Context);

      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            // first attempt to find a settable property, otherwise fall back to a dynamic set without type
            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);
            if (property != null && property.Writable && !property.Ignored)
            {
              SetPropertyValue(property, reader, newObject);
            }
            else
            {
              Type t = (JsonReader.IsPrimitiveToken(reader.TokenType)) ? reader.ValueType : typeof(IDynamicMetaObjectProvider);

              object value = CreateValueNonProperty(reader, t, GetContractSafe(t, null));

              newObject.TrySetMember(memberName, value);
            }
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      contract.InvokeOnDeserialized(newObject, Serializer.Context);

      return newObject;
    }
#endif

    private object CreateAndPopulateObject(JsonReader reader, JsonObjectContract contract, string id)
    {
      object newObject = null;

      if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
        throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.OverrideConstructor != null)
      {
        if (contract.OverrideConstructor.GetParameters().Length > 0)
          return CreateObjectFromNonDefaultConstructor(reader, contract, contract.OverrideConstructor, id);

        newObject = contract.OverrideConstructor.Invoke(null);
      }
      else if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
      {
        newObject = contract.DefaultCreator();
      }
      else if (contract.ParametrizedConstructor != null)
      {
        return CreateObjectFromNonDefaultConstructor(reader, contract, contract.ParametrizedConstructor, id);
      }

      if (newObject == null)
        throw new JsonSerializationException("Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      PopulateObject(newObject, reader, contract, id);
      return newObject;
    }

    private object CreateObjectFromNonDefaultConstructor(JsonReader reader, JsonObjectContract contract, ConstructorInfo constructorInfo, string id)
    {
      ValidationUtils.ArgumentNotNull(constructorInfo, "constructorInfo");

      Type objectType = contract.UnderlyingType;

      IDictionary<JsonProperty, object> propertyValues = ResolvePropertyAndConstructorValues(contract, reader, objectType);

      IDictionary<ParameterInfo, object> constructorParameters = constructorInfo.GetParameters().ToDictionary(p => p, p => (object)null);
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
          property.ValueProvider.SetValue(createdObject, value);
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
              if (!ReadForType(reader, property.PropertyType, property.Converter))
                throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (!property.Ignored)
                propertyValues[property] = CreateValueProperty(reader, property, null, true, null);
              else
                reader.Skip();
            }
            else
            {
              if (!reader.Read())
                throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
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
      } while (!exit && reader.Read());

      return propertyValues;
    }

    private bool ReadForType(JsonReader reader, Type t, JsonConverter propertyConverter)
    {
      // don't read properties with converters as a specific value
      // the value might be a string which will then get converted which will error if read as date for example
      bool hasConverter = (GetConverter(GetContractSafe(t), propertyConverter) != null);

      if (hasConverter)
        return reader.Read();

      if (t == typeof(byte[]))
      {
        reader.ReadAsBytes();
        return true;
      }
      else if ((t == typeof(decimal) || t == typeof(decimal?)))
      {
        reader.ReadAsDecimal();
        return true;
      }
#if !NET20
      else if ((t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?)))
      {
        reader.ReadAsDateTimeOffset();
        return true;
      }
#endif

      do
      {
        if (!reader.Read())
          return false;
      } while (reader.TokenType == JsonToken.Comment);

      return true;
    }

    private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, string id)
    {
      contract.InvokeOnDeserializing(newObject, Serializer.Context);

      Dictionary<JsonProperty, RequiredValue> requiredProperties =
        contract.Properties.Where(m => m.Required != Required.Default).ToDictionary(m => m, m => RequiredValue.None);

      if (id != null)
        Serializer.ReferenceResolver.AddReference(this, id, newObject);

      int initialDepth = reader.Depth;

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            // attempt exact case match first
            // then try match ignoring case
            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

            if (property == null)
            {
              if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));

              reader.Skip();
              continue;
            }

            if (!ReadForType(reader, property.PropertyType, property.Converter))
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            SetRequiredProperty(reader, property, requiredProperties);

            try
            {
              SetPropertyValue(property, reader, newObject);
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(newObject, contract, memberName, ex))
                HandleError(reader, initialDepth);
              else
                throw;
            }
            break;
          case JsonToken.EndObject:
            foreach (KeyValuePair<JsonProperty, RequiredValue> requiredProperty in requiredProperties)
            {
              if (requiredProperty.Value == RequiredValue.None)
                throw new JsonSerializationException("Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, requiredProperty.Key.PropertyName));
              if (requiredProperty.Key.Required == Required.Always && requiredProperty.Value == RequiredValue.Null)
                throw new JsonSerializationException("Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, requiredProperty.Key.PropertyName));
            }

            contract.InvokeOnDeserialized(newObject, Serializer.Context);
            return newObject;
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private void SetRequiredProperty(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, RequiredValue> requiredProperties)
    {
      if (property != null)
      {
        requiredProperties[property] = (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
          ? RequiredValue.Null
          : RequiredValue.Value;
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

    internal enum RequiredValue
    {
      None,
      Null,
      Value
    }
  }
}