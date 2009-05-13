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
  internal class JsonSerializerReader
  {
    internal readonly JsonSerializer _serializer;
    private JsonSerializerProxy _internalSerializer;

    public JsonSerializerReader(JsonSerializer serializer)
    {
      ValidationUtils.ArgumentNotNull(serializer, "serializer");

      _serializer = serializer;
    }

    public void Populate(JsonReader reader, object target)
    {
      ValidationUtils.ArgumentNotNull(target, "target");

      Type objectType = target.GetType();

      if (reader.TokenType == JsonToken.None)
        reader.Read();

      if (reader.TokenType == JsonToken.StartArray)
      {
        PopulateList(CollectionUtils.CreateCollectionWrapper(target), ReflectionUtils.GetCollectionItemType(objectType), reader, null);
      }
      else if (reader.TokenType == JsonToken.StartObject)
      {
        if (!reader.Read())
          throw new JsonSerializationException("Unexpected end when deserializing object.");

        if (CollectionUtils.IsDictionaryType(objectType))
          PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader);
        else
          PopulateObject(target, reader, objectType);
      }
    }

    public object Deserialize(JsonReader reader, Type objectType)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      if (!reader.Read())
        return null;

      if (objectType != null)
        return CreateValue(reader, objectType, null, null);
      else
        return CreateJToken(reader);
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
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

    private object CreateValue(JsonReader reader, Type objectType, object existingValue, JsonConverter memberConverter)
    {
      object value;
      JsonConverter converter;

      if (memberConverter != null)
      {
        return memberConverter.ReadJson(reader, objectType, GetInternalSerializer());
      }
      else if (objectType != null && _serializer.HasClassConverter(objectType, out converter))
      {
        return converter.ReadJson(reader, objectType, GetInternalSerializer());
      }
      else if (objectType != null && _serializer.HasMatchingConverter(objectType, out converter))
      {
        return converter.ReadJson(reader, objectType, GetInternalSerializer());
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
            else
            {
              CheckedRead(reader);

              if (reader.TokenType == JsonToken.PropertyName)
              {
                string propertyName = reader.Value.ToString();

                if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
                {
                  CheckedRead(reader);
                  string id = reader.Value.ToString();

                  CheckedRead(reader);
                  return _serializer.ReferenceResolver.ResolveObject(id);
                }
                else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal)
                  && !CollectionUtils.IsDictionaryType(objectType)
                  && CollectionUtils.IsListType(objectType))
                {
                  return CreateReferencedList(reader, objectType, existingValue);
                }
              }

              if (CollectionUtils.IsDictionaryType(objectType))
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
            }
            break;
          case JsonToken.StartArray:
            value = CreateList(reader, objectType, existingValue, null);
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

      return value;
    }

    private void CheckedRead(JsonReader reader)
    {
      if (!reader.Read())
        throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateReferencedList(JsonReader reader, Type objectType, object existingValue)
    {
      object value;

      CheckedRead(reader);
      string id = reader.Value.ToString();

      CheckedRead(reader);
      if (reader.TokenType != JsonToken.PropertyName && reader.Value.ToString() != "value")
        throw new JsonSerializationException("Error reading referenced list.");

      CheckedRead(reader);
      if (reader.TokenType != JsonToken.StartArray)
        throw new JsonSerializationException("Error reading referenced list.");

      value = CreateList(reader, objectType, existingValue, id);

      CheckedRead(reader);
      return value;
    }

    private object CreateList(JsonReader reader, Type objectType, object existingValue, string reference)
    {
      object value;
      if (objectType != null)
      {
        if (existingValue == null)
          value = CreateAndPopulateList(reader, objectType, reference);
        else
          value = PopulateList(CollectionUtils.CreateCollectionWrapper(existingValue), ReflectionUtils.GetCollectionItemType(objectType), reader, reference);
      }
      else
      {
        value = CreateJToken(reader);
      }
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

    private void SetObjectMember(JsonReader reader, object target, Type targetType, string memberName)
    {
      JsonMemberMappingCollection memberMappings = _serializer.GetMemberMappings(targetType);

      JsonMemberMapping memberMapping;
      // attempt exact case match first
      // then try match ignoring case
      if (memberMappings.TryGetClosestMatchMapping(memberName, out memberMapping))
      {
        SetMappingValue(memberMapping, reader, target);
      }
      else
      {
        if (_serializer.MissingMemberHandling == MissingMemberHandling.Error)
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

      if ((_serializer.ObjectCreationHandling == ObjectCreationHandling.Auto || _serializer.ObjectCreationHandling == ObjectCreationHandling.Reuse)
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

      object value = CreateValue(reader, memberType, (useExistingValue) ? currentValue : null, JsonTypeReflector.GetConverter(memberMapping.Member, memberType));

      if (!useExistingValue && ShouldSetMappingValue(memberMapping, value))
        ReflectionUtils.SetMemberValue(memberMapping.Member, target, value);
    }

    private bool ShouldSetMappingValue(JsonMemberMapping memberMapping, object value)
    {
      if (memberMapping.NullValueHandling.GetValueOrDefault(_serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
        return false;

      if (memberMapping.DefaultValueHandling.GetValueOrDefault(_serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && Equals(value, memberMapping.DefaultValue))
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

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            if (reader.Value is string && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
            {
              reader.Read();
              _serializer.ReferenceResolver.AddObjectReference(reader.Value.ToString(), dictionary.UnderlyingDictionary);
            }
            else
            {
              object keyValue = EnsureType(reader.Value, dictionaryKeyType);
              reader.Read();

              dictionary.Add(keyValue, CreateValue(reader, dictionaryValueType, null, null));
            }
            break;
          case JsonToken.EndObject:
            return dictionary;
          default:
            throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw new JsonSerializationException("Unexpected end when deserializing object.");
    }

    private object CreateAndPopulateList(JsonReader reader, Type objectType, string reference)
    {
      if (IsTypeGenericCollectionInterface(objectType))
      {
        Type itemType = ReflectionUtils.GetCollectionItemType(objectType);
        objectType = ReflectionUtils.MakeGenericType(typeof(List<>), itemType);
      }

      return CollectionUtils.CreateAndPopulateList(objectType, (l, isTemporaryListReference) =>
        {
          if (reference != null && isTemporaryListReference)
            throw new JsonSerializationException("Cannot preserve reference to array or readonly list: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));

          PopulateList(l, ReflectionUtils.GetCollectionItemType(objectType), reader, reference);
        });
    }

    private IList PopulateList(IList list, Type listItemType, JsonReader reader, string reference)
    {
      if (reference != null)
        _serializer.ReferenceResolver.AddObjectReference(reference, list);

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.EndArray:
            return list;
          case JsonToken.Comment:
            break;
          default:
            object value = CreateValue(reader, listItemType, null, null);

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

      return (genericDefinition == typeof(IDictionary<,>));
    }

    private bool IsTypeGenericCollectionInterface(Type type)
    {
      if (!type.IsGenericType)
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof(IList<>)
              || genericDefinition == typeof(ICollection<>)
              || genericDefinition == typeof(IEnumerable<>));
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
      JsonMemberMappingCollection memberMappings = _serializer.GetMemberMappings(objectType);
      IDictionary<JsonMemberMapping, object> mappingValues = memberMappings.ToDictionary(kv => kv, kv => (object)null);

      bool exit = false;
      do
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
                mappingValues[memberMapping] = CreateValue(reader, memberType, null, memberMapping.MemberConverter);
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
      JsonMemberMappingCollection memberMappings = _serializer.GetMemberMappings(objectType);
      Dictionary<string, bool> requiredMappings =
        memberMappings.Where(m => m.Required).ToDictionary(m => m.MappingName, m => false);

      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            if (!reader.Read())
              throw new JsonSerializationException("Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            if (string.Equals(memberName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
            {
              _serializer.ReferenceResolver.AddObjectReference(reader.Value.ToString(), newObject);
            }
            else
            {
              if (reader.TokenType != JsonToken.Null)
                SetRequiredMapping(memberName, requiredMappings);

              SetObjectMember(reader, newObject, objectType, memberName);
            }
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
      } while (reader.Read());

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
  }
}