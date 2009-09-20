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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerInternalReader : JsonSerializerInternalBase
  {
    internal readonly JsonSerializer _serializer;
    private JsonSerializerProxy _internalSerializer;

    public JsonSerializerInternalReader(JsonSerializer serializer)
    {
      ValidationUtils.ArgumentNotNull(serializer, "serializer");

      _serializer = serializer;
    }

    public void Populate(JsonReader reader, object target)
    {
      ValidationUtils.ArgumentNotNull(target, "target");

      Type objectType = target.GetType();

      JsonContract contract = _serializer.ContractResolver.ResolveContract(objectType);

      if (reader.TokenType == JsonToken.None)
        reader.Read();

      if (reader.TokenType == JsonToken.StartArray)
      {
        PopulateList(CollectionUtils.CreateCollectionWrapper(target), reader, null, GetArrayContract(objectType));
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
          throw new JsonSerializationException("Expected a JsonObjectContract or JsonDictionaryContract for type '{0}', got '{1}'.".FormatWith(CultureInfo.InvariantCulture, objectType, contract.GetType()));
      }
    }

    public object Deserialize(JsonReader reader, Type objectType)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      if (reader.TokenType == JsonToken.None && !reader.Read())
        return null;

      return CreateValue(reader, objectType, null, null);
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

    private JToken CreateJToken(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      JToken token;
      using (JTokenWriter writer = new JTokenWriter())
      {
        writer.WriteToken(reader);
        token = writer.Token;
      }

      return token;
    }

    private JToken CreateJObject(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

//          throw new Exception("Expected current token of type {0}, got {1}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.PropertyName, reader.TokenType));

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

    private object CreateValue(JsonReader reader, Type objectType, object existingValue, JsonConverter memberConverter)
    {
      JsonConverter converter;

      if (memberConverter != null)
        return memberConverter.ReadJson(reader, objectType, GetInternalSerializer());

      if (objectType != null && _serializer.HasClassConverter(objectType, out converter))
        return converter.ReadJson(reader, objectType, GetInternalSerializer());

      if (objectType != null && _serializer.HasMatchingConverter(objectType, out converter))
        return converter.ReadJson(reader, objectType, GetInternalSerializer());

      if (objectType == typeof (JsonRaw))
        return JsonRaw.Create(reader);

      do
      {
        switch (reader.TokenType)
        {
            // populate a typed object or generic dictionary/array
            // depending upon whether an objectType was supplied
          case JsonToken.StartObject:
            return CreateObject(reader, objectType, existingValue);
          case JsonToken.StartArray:
            return CreateList(reader, objectType, existingValue, null);
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.String:
          case JsonToken.Boolean:
          case JsonToken.Date:
            // convert empty string to null automatically
            if (reader.Value is string &&
              string.IsNullOrEmpty((string)reader.Value) &&
              objectType != null &&
              ReflectionUtils.IsNullable(objectType))
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

            return null;
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw new JsonSerializationException("Unexpected token while deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateObject(JsonReader reader, Type objectType, object existingValue)
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
            string reference = reader.Value.ToString();

            CheckedRead(reader);
            return _serializer.ReferenceResolver.ResolveReference(reference);
          }
          else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);
            string qualifiedTypeName = reader.Value.ToString();

            CheckedRead(reader);

            if (_serializer.TypeNameHandling != TypeNameHandling.None)
            {
              string typeName;
              string assemblyName;
              ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName, out typeName, out assemblyName);

              Type specifiedType;
              try
              {
                specifiedType = _serializer.Binder.BindToType(assemblyName, typeName);
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
            object list = CreateList(reader, objectType, existingValue, id);
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

      JsonContract contract = _serializer.ContractResolver.ResolveContract(objectType);

      if (contract == null)
        throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      if (contract is JsonDictionaryContract)
      {
        if (existingValue == null)
          return CreateAndPopulateDictionary(reader, (JsonDictionaryContract) contract, id);

        return PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(existingValue), reader, (JsonDictionaryContract) contract, id);
      }
      
      if (contract is JsonObjectContract)
      {
        if (existingValue == null)
          return CreateAndPopulateObject(reader, (JsonObjectContract) contract, id);
        
        return PopulateObject(existingValue, reader, (JsonObjectContract) contract, id);
      }
      
      throw new JsonSerializationException("Expected a JsonObjectContract or JsonDictionaryContract for type '{0}', got '{1}'.".FormatWith(CultureInfo.InvariantCulture, objectType, contract.GetType()));
    }

    private JsonArrayContract GetArrayContract(Type objectType)
    {
      JsonContract contract = _serializer.ContractResolver.ResolveContract(objectType);
      if (contract == null)
        throw new JsonSerializationException("Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      JsonArrayContract arrayContract = contract as JsonArrayContract;
      if (arrayContract == null)
        throw new JsonSerializationException("Expected a JsonArrayContract for type '{0}', got '{1}'.".FormatWith(CultureInfo.InvariantCulture, objectType, contract.GetType()));

      return arrayContract;
    }

    private void CheckedRead(JsonReader reader)
    {
      if (!reader.Read())
        throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateList(JsonReader reader, Type objectType, object existingValue, string reference)
    {
      object value;
      if (HasDefinedType(objectType))
      {
        JsonArrayContract contract = GetArrayContract(objectType);

        if (existingValue == null)
          value = CreateAndPopulateList(reader, reference, contract);
        else
          value = PopulateList(CollectionUtils.CreateCollectionWrapper(existingValue), reader, reference, contract);
      }
      else
      {
        value = CreateJToken(reader);
      }
      return value;
    }

    private bool HasDefinedType(Type type)
    {
      return (type != null && type != typeof (object));
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
        return ConvertUtils.ConvertOrCast(value, CultureInfo.InvariantCulture, targetType);

      return value;
    }

    private void SetObjectMember(JsonReader reader, object target, JsonObjectContract contract, string memberName)
    {
      JsonProperty property;
      // attempt exact case match first
      // then try match ignoring case
      if (contract.Properties.TryGetClosestMatchProperty(memberName, out property))
      {
        SetPropertyValue(property, reader, target);
      }
      else
      {
        if (_serializer.MissingMemberHandling == MissingMemberHandling.Error)
          throw new JsonSerializationException("Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));

        reader.Skip();
      }
    }

    private void SetPropertyValue(JsonProperty property, JsonReader reader, object target)
    {
      if (property.Ignored)
      {
        reader.Skip();
        return;
      }

      // get the member's underlying type
      Type memberType = ReflectionUtils.GetMemberUnderlyingType(property.Member);

      object currentValue = null;
      bool useExistingValue = false;

      if ((_serializer.ObjectCreationHandling == ObjectCreationHandling.Auto || _serializer.ObjectCreationHandling == ObjectCreationHandling.Reuse)
        && (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject)
        && property.Readable)
      {
        currentValue = ReflectionUtils.GetMemberValue(property.Member, target);

        useExistingValue = (currentValue != null && !memberType.IsArray && !ReflectionUtils.InheritsGenericDefinition(memberType, typeof(ReadOnlyCollection<>)));
      }

      if (!property.Writable && !useExistingValue)
      {
        reader.Skip();
        return;
      }

      object value = CreateValue(reader, memberType, (useExistingValue) ? currentValue : null, JsonTypeReflector.GetConverter(property.Member, memberType));

      if (!useExistingValue && ShouldSetPropertyValue(property, value))
        ReflectionUtils.SetMemberValue(property.Member, target, value);
    }

    private bool ShouldSetPropertyValue(JsonProperty property, object value)
    {
      if (property.NullValueHandling.GetValueOrDefault(_serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
        return false;

      if (property.DefaultValueHandling.GetValueOrDefault(_serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && Equals(value, property.DefaultValue))
        return false;

      if (!property.Writable)
        return false;

      return true;
    }

    private object CreateAndPopulateDictionary(JsonReader reader, JsonDictionaryContract contract, string id)
    {
      IWrappedDictionary dictionary = CollectionUtils.CreateDictionaryWrapper(Activator.CreateInstance(contract.DictionaryTypeToCreate));

      PopulateDictionary(dictionary, reader, contract, id);

      return dictionary.UnderlyingDictionary;
    }

    private IDictionary PopulateDictionary(IWrappedDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, string id)
    {
      if (id != null)
        _serializer.ReferenceResolver.AddReference(id, dictionary.UnderlyingDictionary);

      contract.InvokeOnDeserializing(dictionary.UnderlyingDictionary);

      int initialDepth = reader.Depth;

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            object keyValue = EnsureType(reader.Value, contract.DictionaryKeyType);
            CheckedRead(reader);

            try
            {
              dictionary.Add(keyValue, CreateValue(reader, contract.DictionaryValueType, null, null));
            }
            catch (Exception ex)
            {
#if !PocketPC && !SILVERLIGHT && !NET20
              ErrorContext errorContext = GetErrorContext(dictionary.UnderlyingDictionary, keyValue, ex);
              contract.InvokeOnError(dictionary.UnderlyingDictionary, errorContext);

              if (errorContext.Handled)
                HandleError(reader, initialDepth);
              else
#endif
                throw;
            }
            break;
          case JsonToken.EndObject:
            contract.InvokeOnDeserialized(dictionary.UnderlyingDictionary);
            
            return dictionary;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateAndPopulateList(JsonReader reader, string reference, JsonArrayContract contract)
    {
      return CollectionUtils.CreateAndPopulateList(contract.CollectionTypeToCreate, (l, isTemporaryListReference) =>
        {
          if (reference != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot preserve reference to array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

#if !PocketPC && !SILVERLIGHT
          if (contract.OnSerializing != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot call OnSerializing on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
          if (contract.OnError != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot call OnError on an array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
#endif

          PopulateList(l, reader, reference, contract);
        });
    }

    private IList PopulateList(IList list, JsonReader reader, string reference, JsonArrayContract contract)
    {
      if (reference != null)
        _serializer.ReferenceResolver.AddReference(reference, list);

      contract.InvokeOnDeserializing(list);

      int initialDepth = reader.Depth;

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.EndArray:
            contract.InvokeOnDeserialized(list);

            return list;
          case JsonToken.Comment:
            break;
          default:
            try
            {
              object value = CreateValue(reader, contract.CollectionItemType, null, null);

              list.Add(value);
            }
            catch (Exception ex)
            {
#if !PocketPC && !SILVERLIGHT && !NET20
              ErrorContext errorContext = GetErrorContext(list, list.Count, ex);
              contract.InvokeOnError(list, errorContext);

              if (errorContext.Handled)
                HandleError(reader, initialDepth);
              else
#endif
                throw;
            }
            break;
        }
      }

      throw new JsonSerializationException("Unexpected end when deserializing array.");
    }

    private object CreateAndPopulateObject(JsonReader reader, JsonObjectContract contract, string id)
    {
      object newObject;

      if (contract.UnderlyingType.IsInterface || contract.UnderlyingType.IsAbstract)
        throw new JsonSerializationException("Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (ReflectionUtils.HasDefaultConstructor(contract.UnderlyingType))
      {
        newObject = Activator.CreateInstance(contract.UnderlyingType);

        PopulateObject(newObject, reader, contract, id);
        return newObject;
      }

      return CreateObjectFromNonDefaultConstructor(contract, reader);
    }

    private object CreateObjectFromNonDefaultConstructor(JsonObjectContract contract, JsonReader reader)
    {
      Type objectType = contract.UnderlyingType;

      ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

      if (constructors.Length == 0)
        throw new JsonSerializationException("Could not find a public constructor for type {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
      if (constructors.Length > 1)
        throw new JsonSerializationException("Unable to determine which constructor to use for type {0}. A class with no default constructor should have only one constructor with arguments.".FormatWith(CultureInfo.InvariantCulture, objectType));

      // object should have a single constructor
      ConstructorInfo c = constructors[0];

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

            JsonProperty property;
            // attempt exact case match first
            // then try match ignoring case
            if (contract.Properties.TryGetClosestMatchProperty(memberName, out property))
            {
              if (!property.Ignored)
              {
                Type memberType = ReflectionUtils.GetMemberUnderlyingType(property.Member);
                propertyValues[property] = CreateValue(reader, memberType, null, property.MemberConverter);
              }
            }
            else
            {
              if (_serializer.MissingMemberHandling == MissingMemberHandling.Error)
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

      IDictionary<ParameterInfo, object> constructorParameters = c.GetParameters().ToDictionary(p => p, p => (object)null);
      IDictionary<JsonProperty, object> remainingPropertyValues = new Dictionary<JsonProperty, object>();

      foreach (KeyValuePair<JsonProperty, object> propertyValue in propertyValues)
      {
        ParameterInfo matchingConstructorParameter = constructorParameters.ForgivingCaseSensitiveFind(kv => kv.Key.Name, propertyValue.Key.PropertyName).Key;
        if (matchingConstructorParameter != null)
          constructorParameters[matchingConstructorParameter] = propertyValue.Value;
        else
          remainingPropertyValues.Add(propertyValue);
      }

      object createdObject = ReflectionUtils.CreateInstance(objectType, constructorParameters.Values.ToArray());
      contract.InvokeOnDeserializing(createdObject);

      // go through unused values and set the newly created object's properties
      foreach (KeyValuePair<JsonProperty, object> remainingPropertyValue in remainingPropertyValues)
      {
        if (ShouldSetPropertyValue(remainingPropertyValue.Key, remainingPropertyValue.Value))
          ReflectionUtils.SetMemberValue(remainingPropertyValue.Key.Member, createdObject, remainingPropertyValue.Value);
      }

      contract.InvokeOnDeserialized(createdObject);
      return createdObject;
    }

    private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, string id)
    {
      contract.InvokeOnDeserializing(newObject);

      Dictionary<string, bool> requiredProperties =
        contract.Properties.Where(m => m.Required).ToDictionary(m => m.PropertyName, m => false);

      if (id != null)
        _serializer.ReferenceResolver.AddReference(id, newObject);

      int initialDepth = reader.Depth;

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            if (reader.TokenType != JsonToken.Null)
              SetRequiredProperty(memberName, requiredProperties);

            try
            {
              SetObjectMember(reader, newObject, contract, memberName);
            }
            catch (Exception ex)
            {
#if !PocketPC && !SILVERLIGHT && !NET20
              ErrorContext errorContext = GetErrorContext(newObject, memberName, ex);
              contract.InvokeOnError(newObject, errorContext);

              if (errorContext.Handled)
                HandleError(reader, initialDepth);
              else
#endif
                throw;
            }
            break;
          case JsonToken.EndObject:
            foreach (KeyValuePair<string, bool> requiredProperty in requiredProperties)
            {
              if (!requiredProperty.Value)
                throw new JsonSerializationException("Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, requiredProperty.Key));
            }

            contract.InvokeOnDeserialized(newObject);
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

    private void SetRequiredProperty(string memberName, Dictionary<string, bool> requiredProperties)
    {
      // first attempt to find exact case match
      // then attempt case insensitive match
      if (requiredProperties.ContainsKey(memberName))
      {
        requiredProperties[memberName] = true;
      }
      else
      {
        foreach (KeyValuePair<string, bool> requiredProperty in requiredProperties)
        {
          if (string.Compare(requiredProperty.Key, requiredProperty.Key, StringComparison.OrdinalIgnoreCase) == 0)
          {
            requiredProperties[requiredProperty.Key] = true;
            break;
          }
        }
      }
    }

    private void HandleError(JsonReader reader, int initialDepth)
    {
      ClearErrorContext();

      while (reader.Depth > (initialDepth + 1))
      {
        reader.Read();
      }
    }
  }
}