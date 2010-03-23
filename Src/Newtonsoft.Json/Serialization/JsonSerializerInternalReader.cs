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

    public JsonSerializerInternalReader(JsonSerializer serializer) : base(serializer)
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
          PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader, (JsonDictionaryContract) contract, id);
        else if (contract is JsonObjectContract)
          PopulateObject(target, reader, (JsonObjectContract) contract, id);
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

      if (reader.TokenType == JsonToken.None && !reader.Read())
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

      return CreateValueInternal(reader, objectType, contract, currentValue);
    }

    private object CreateValueNonProperty(JsonReader reader, Type objectType, JsonContract contract)
    {
      JsonConverter converter = GetConverter(contract, null);

      if (converter != null && converter.CanRead)
        return converter.ReadJson(reader, objectType, null, GetInternalSerializer());

      return CreateValueInternal(reader, objectType, contract, null);
    }

    private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, object existingValue)
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
            return CreateObject(reader, objectType, contract, existingValue);
          case JsonToken.StartArray:
            return CreateList(reader, objectType, contract, existingValue, null);
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.Boolean:
          case JsonToken.Date:
          case JsonToken.Bytes:
            return EnsureType(reader.Value, objectType);
          case JsonToken.String:
            // convert empty string to null automatically for nullable types
            if (string.IsNullOrEmpty((string)reader.Value) &&
              objectType != null &&
              ReflectionUtils.IsNullableType(objectType))
              return null;

            return EnsureType(reader.Value, objectType);
          case JsonToken.StartConstructor:
          case JsonToken.EndConstructor:
            string constructorName = reader.Value.ToString();

            return constructorName;
          case JsonToken.Null:
          case JsonToken.Undefined:
            if (objectType == typeof (DBNull))
              return DBNull.Value;

            return EnsureType(reader.Value, objectType);
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

    private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, object existingValue)
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

            return Serializer.ReferenceResolver.ResolveReference(reference);
          }
          else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);
            string qualifiedTypeName = reader.Value.ToString();

            CheckedRead(reader);

            if (Serializer.TypeNameHandling != TypeNameHandling.None)
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
            object list = CreateList(reader, objectType, contract, existingValue, id);
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

    private object CreateList(JsonReader reader, Type objectType, JsonContract contract, object existingValue, string reference)
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
      return (type != null && type != typeof (object) && !type.IsSubclassOf(typeof(JToken)));
    }

    private object EnsureType(object value, Type targetType)
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
          return ConvertUtils.ConvertOrCast(value, CultureInfo.InvariantCulture, targetType);
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

        useExistingValue = (currentValue != null && !property.PropertyType.IsArray && !ReflectionUtils.InheritsGenericDefinition(property.PropertyType, typeof(ReadOnlyCollection<>)));
      }

      if (!property.Writable && !useExistingValue)
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
        property.ValueProvider.SetValue(target, value);
    }

    private bool ShouldSetPropertyValue(JsonProperty property, object value)
    {
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
        return false;

      if (property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && Equals(value, property.DefaultValue))
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
        Serializer.ReferenceResolver.AddReference(id, dictionary.UnderlyingDictionary);

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
              keyValue = EnsureType(reader.Value, contract.DictionaryKeyType);
            }
            catch (Exception ex)
            {
              throw new JsonSerializationException("Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
            }

            CheckedRead(reader);

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

    private object PopulateList(IWrappedCollection wrappedList, JsonReader reader, string reference, JsonArrayContract contract)
    {
      object list = wrappedList.UnderlyingCollection;

      if (reference != null)
        Serializer.ReferenceResolver.AddReference(reference, list);

      contract.InvokeOnDeserializing(list, Serializer.Context);

      int initialDepth = reader.Depth;

      while (reader.Read())
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
        Serializer.ReferenceResolver.AddReference(id, createdObject);

      contract.InvokeOnDeserializing(createdObject, Serializer.Context);
      contract.InvokeOnDeserialized(createdObject, Serializer.Context);

      return createdObject;
    }
#endif

    private object CreateAndPopulateObject(JsonReader reader, JsonObjectContract contract, string id)
    {
      object newObject = null;

      if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
        throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
      {
        newObject = contract.DefaultCreator();
      }

      if (newObject != null)
      {
        PopulateObject(newObject, reader, contract, id);
        return newObject;
      }

      return CreateObjectFromNonDefaultConstructor(reader, contract, id);
    }

    private object CreateObjectFromNonDefaultConstructor(JsonReader reader, JsonObjectContract contract, string id)
    {
      Type objectType = contract.UnderlyingType;

      if (contract.ParametrizedConstructor == null)
        throw new JsonSerializationException("Unable to find a constructor to use for type {0}. A class should either have a default constructor or only one constructor with arguments.".FormatWith(CultureInfo.InvariantCulture, objectType));

      // create a dictionary to put retrieved values into
      IDictionary<JsonProperty, object> propertyValues = contract.Properties.Where(p => !p.Ignored).ToDictionary(kv => kv, kv => (object)null);

      bool exit = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            // attempt exact case match first
            // then try match ignoring case
            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

            if (property != null)
            {
              if (!property.Ignored)
                propertyValues[property] = CreateValueProperty(reader, property, null, true, null);
              else
                reader.Skip();
            }
            else
            {
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

      IDictionary<ParameterInfo, object> constructorParameters = contract.ParametrizedConstructor.GetParameters().ToDictionary(p => p, p => (object)null);
      IDictionary<JsonProperty, object> remainingPropertyValues = new Dictionary<JsonProperty, object>();

      foreach (KeyValuePair<JsonProperty, object> propertyValue in propertyValues)
      {
        ParameterInfo matchingConstructorParameter = constructorParameters.ForgivingCaseSensitiveFind(kv => kv.Key.Name, propertyValue.Key.PropertyName).Key;
        if (matchingConstructorParameter != null)
          constructorParameters[matchingConstructorParameter] = propertyValue.Value;
        else
          remainingPropertyValues.Add(propertyValue);
      }

      object createdObject = contract.ParametrizedConstructor.Invoke(constructorParameters.Values.ToArray());

      if (id != null)
        Serializer.ReferenceResolver.AddReference(id, createdObject);

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

    private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, string id)
    {
      contract.InvokeOnDeserializing(newObject, Serializer.Context);

      Dictionary<JsonProperty, RequiredValue> requiredProperties =
        contract.Properties.Where(m => m.Required != Required.Default).ToDictionary(m => m, m => RequiredValue.None);

      if (id != null)
        Serializer.ReferenceResolver.AddReference(id, newObject);

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

            if (property.PropertyType == typeof(byte[]))
            {
              reader.ReadAsBytes();
            }
            else
            {
              if (!reader.Read())
                throw new JsonSerializationException(
                  "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
            }

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