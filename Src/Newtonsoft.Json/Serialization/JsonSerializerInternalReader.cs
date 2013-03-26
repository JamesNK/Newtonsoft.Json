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
#if !(NET35 || NET20)
using System.ComponentModel;
using System.Dynamic;
#endif
using System.Diagnostics;
using System.Globalization;
#if !(PORTABLE || NET35 || NET20 || SILVERLIGHT)
using System.Numerics;
#endif
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
    internal enum PropertyPresence
    {
      None,
      Null,
      Value
    }

    private JsonSerializerProxy _internalSerializer;
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
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
          PopulateList(CollectionUtils.CreateCollectionWrapper(target), reader, (JsonArrayContract) contract, null, null);
        else
          throw JsonSerializationException.Create(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
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
          PopulateDictionary(CollectionUtils.CreateDictionaryWrapper(target), reader, (JsonDictionaryContract) contract, null, id);
        else if (contract.ContractType == JsonContractType.Object)
          PopulateObject(target, reader, (JsonObjectContract) contract, null, id);
        else
          throw JsonSerializationException.Create(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
      }
      else
      {
        throw JsonSerializationException.Create(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
      }
    }

    private JsonContract GetContractSafe(Type type)
    {
      if (type == null)
        return null;

      return Serializer.ContractResolver.ResolveContract(type);
    }

    public object Deserialize(JsonReader reader, Type objectType, bool checkAdditionalContent)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      JsonContract contract = GetContractSafe(objectType);

      try
      {
        JsonConverter converter = GetConverter(contract, null, null, null);

        if (reader.TokenType == JsonToken.None && !ReadForType(reader, contract, converter != null))
        {
          if (contract != null && !contract.IsNullable)
            throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          return null;
        }

        object deserializedValue;

        if (converter != null && converter.CanRead)
          deserializedValue = DeserializeConvertable(converter, reader, objectType, null);
        else
          deserializedValue = CreateValueInternal(reader, objectType, contract, null, null, null, null);

        if (checkAdditionalContent)
        {
          if (reader.Read() && reader.TokenType != JsonToken.Comment)
            throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
        }

        return deserializedValue;
      }
      catch (Exception ex)
      {
        if (IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
        {
          HandleError(reader, false, 0);
          return null;
        }
        else
        {
          throw;
        }
      }
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
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
          writer.WriteToken(reader, reader.Depth - 1, true);
        else
          writer.WriteEndObject();

        token = writer.Token;
      }

      return token;
    }

    private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
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
            return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
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
            if (string.IsNullOrEmpty((string)reader.Value) && objectType != typeof(string) && objectType != typeof(object) && contract != null && contract.IsNullable)
              return null;

            // string that needs to be returned as a byte array should be base 64 decoded
            if (objectType == typeof (byte[]))
              return Convert.FromBase64String((string) reader.Value);

            return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
          case JsonToken.StartConstructor:
            string constructorName = reader.Value.ToString();

            return EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);
          case JsonToken.Null:
          case JsonToken.Undefined:
#if !(NETFX_CORE || PORTABLE)
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
            throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
        }
      } while (reader.Read());

      throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
    }

    internal string GetExpectedDescription(JsonContract contract)
    {
      switch (contract.ContractType)
      {
        case JsonContractType.Object:
        case JsonContractType.Dictionary:
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
        case JsonContractType.Serializable:
#endif
#if !(NET35 || NET20)
        case JsonContractType.Dynamic:
#endif
          return @"JSON object (e.g. {""name"":""value""})";
        case JsonContractType.Array:
          return @"JSON array (e.g. [1,2,3])";
        case JsonContractType.Primitive:
          return @"JSON primitive value (e.g. string, number, boolean, null)";
        case JsonContractType.String:
          return @"JSON string value";
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      JsonConverter converter = null;
      if (memberConverter != null)
      {
        // member attribute converter
        converter = memberConverter;
      }
      else if (containerProperty != null && containerProperty.ItemConverter != null)
      {
        converter = containerProperty.ItemConverter;
      }
      else if (containerContract != null && containerContract.ItemConverter != null)
      {
        converter = containerContract.ItemConverter;
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

    private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
    {
      CheckedRead(reader);

      string id;
      object newValue;
      if (ReadSpecialProperties(reader, ref objectType, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id))
        return newValue;

      if (!HasDefinedType(objectType))
        return CreateJObject(reader);

      if (contract == null)
        throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      switch (contract.ContractType)
      {
        case JsonContractType.Object:
          bool createdFromNonDefaultConstructor = false;
          JsonObjectContract objectContract = (JsonObjectContract) contract;
          object targetObject;
          if (existingValue != null)
            targetObject = existingValue;
          else
            targetObject = CreateNewObject(reader, objectContract, member, containerMember, id, out createdFromNonDefaultConstructor);

          // don't populate if read from non-default constructor because the object has already been read
          if (createdFromNonDefaultConstructor)
            return targetObject;

          return PopulateObject(targetObject, reader, objectContract, member, id);
        case JsonContractType.Primitive:
          JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;
          // if the content is inside $value then read past it
          if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
          {
            CheckedRead(reader);

            // the token should not be an object because the $type value could have been included in the object
            // without needing the $value property
            if (reader.TokenType == JsonToken.StartObject)
              throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);

            object value = CreateValueInternal(reader, objectType, primitiveContract, member, null, null, existingValue);

            CheckedRead(reader);
            return value;
          }
          break;
        case JsonContractType.Dictionary:
          JsonDictionaryContract dictionaryContract = (JsonDictionaryContract) contract;
          bool isTemporaryDictionary = false;
          object targetDictionary;
          if (existingValue != null)
            targetDictionary = existingValue;
          else
            targetDictionary = CreateNewDictionary(reader, dictionaryContract, out isTemporaryDictionary);

          object dictionary = PopulateDictionary(dictionaryContract.CreateWrapper(targetDictionary), reader, dictionaryContract, member, id);

          if (isTemporaryDictionary)
          {
            if (dictionaryContract.IsReadOnlyDictionary)
            {
              dictionary = ReflectionUtils.CreateInstance(contract.CreatedType, dictionary);
            }
          }

          return dictionary;
#if !(NET35 || NET20)
        case JsonContractType.Dynamic:
          JsonDynamicContract dynamicContract = (JsonDynamicContract) contract;
          return CreateDynamic(reader, dynamicContract, member, id);
#endif
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
        case JsonContractType.Serializable:
          JsonISerializableContract serializableContract = (JsonISerializableContract) contract;
          return CreateISerializable(reader, serializableContract, id);
#endif
      }

      throw JsonSerializationException.Create(reader, @"Cannot deserialize the current JSON object (e.g. {{""name"":""value""}}) into type '{0}' because the type requires a {1} to deserialize correctly.
To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object.
".FormatWith(CultureInfo.InvariantCulture, objectType, GetExpectedDescription(contract)));
    }

    private bool ReadSpecialProperties(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id)
    {
      id = null;
      newValue = null;

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
                throw JsonSerializationException.Create(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

              string reference = (reader.Value != null) ? reader.Value.ToString() : null;

              CheckedRead(reader);

              if (reference != null)
              {
                if (reader.TokenType == JsonToken.PropertyName)
                  throw JsonSerializationException.Create(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));

                newValue = Serializer.ReferenceResolver.ResolveReference(this, reference);

                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
                  TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, reference, newValue.GetType())), null);

                return true;
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

              TypeNameHandling resolvedTypeNameHandling =
                ((member != null) ? member.TypeNameHandling : null)
                ?? ((containerContract != null) ? containerContract.ItemTypeNameHandling : null)
                ?? ((containerMember != null) ? containerMember.ItemTypeNameHandling : null)
                ?? Serializer.TypeNameHandling;

              if (resolvedTypeNameHandling != TypeNameHandling.None)
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
                  throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
                }

                if (specifiedType == null)
                  throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));

                if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                  TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved type '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName, specifiedType)), null);

                if (objectType != null
#if !(NET35 || NET20)
                    && objectType != typeof (IDynamicMetaObjectProvider)
#endif
                    && !objectType.IsAssignableFrom(specifiedType))
                  throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));

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
              newValue = list;
              return true;
            }
            else
            {
              specialProperty = false;
            }
          } while (specialProperty
                   && reader.TokenType == JsonToken.PropertyName);
        }
      }
      return false;
    }

    private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
    {
      if (contract == null)
        throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));

      JsonArrayContract arrayContract = contract as JsonArrayContract;
      if (arrayContract == null)
        throw JsonSerializationException.Create(reader, @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly.
To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array.
".FormatWith(CultureInfo.InvariantCulture, objectType, GetExpectedDescription(contract)));

      return arrayContract;
    }

    private void CheckedRead(JsonReader reader)
    {
      if (!reader.Read())
        throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
    }

    private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string id)
    {
      object value;
      if (HasDefinedType(objectType))
      {
        JsonArrayContract arrayContract = EnsureArrayContract(reader, objectType, contract);

        if (existingValue == null)
        {
          bool isTemporaryListReference;
          IList list = CollectionUtils.CreateList(contract.CreatedType, out isTemporaryListReference);

          if (id != null && isTemporaryListReference)
            throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          if (contract.OnSerializingCallbacks.Count > 0 && isTemporaryListReference)
            throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          if (contract.OnErrorCallbacks.Count > 0 && isTemporaryListReference)
            throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

          if (!arrayContract.IsMultidimensionalArray)
            PopulateList(arrayContract.CreateWrapper(list), reader, arrayContract, member, id);
          else
            PopulateMultidimensionalArray(list, reader, arrayContract, member, id);

          // create readonly and fixed sized collections using the temporary list
          if (isTemporaryListReference)
          {
            if (arrayContract.IsMultidimensionalArray)
            {
              list = CollectionUtils.ToMultidimensionalArray(list, ReflectionUtils.GetCollectionItemType(contract.CreatedType), contract.CreatedType.GetArrayRank());
            }
            else if (contract.CreatedType.IsArray)
            {
              list = CollectionUtils.ToArray(((List<object>) list).ToArray(), ReflectionUtils.GetCollectionItemType(contract.CreatedType));
            }
            else if (ReflectionUtils.InheritsGenericDefinition(contract.CreatedType, typeof(ReadOnlyCollection<>)))
            {
              list = (IList) ReflectionUtils.CreateInstance(contract.CreatedType, list);
            }
            else
            {
              return ReflectionUtils.CreateInstance(contract.CreatedType, list);
            }
          }
          else if (list is IWrappedCollection)
          {
            return ((IWrappedCollection)list).UnderlyingCollection;
          }

          value = list;
        }
        else
        {
          value = PopulateList(arrayContract.CreateWrapper(existingValue), reader, arrayContract, member, id);
        }
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
#if !(NET35 || NET20)
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

#if !(PORTABLE || NET35 || NET20 || SILVERLIGHT)
            if (value is BigInteger)
              return ConvertUtils.FromBigInteger((BigInteger)value, targetType);
#endif

            return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
          }

          return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
        }
        catch (Exception ex)
        {
          throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(value), targetType), ex);
        }
      }

      return value;
    }

    private void SetPropertyValue(JsonProperty property, JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target)
    {
      object currentValue;
      bool useExistingValue;
      JsonContract propertyContract;
      bool gottenCurrentValue;

      if (CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out useExistingValue, out currentValue, out propertyContract, out gottenCurrentValue))
        return;

      object value;

      if (propertyConverter != null && propertyConverter.CanRead)
      {
        if (!gottenCurrentValue && target != null && property.Readable)
          currentValue = property.ValueProvider.GetValue(target);

        value = DeserializeConvertable(propertyConverter, reader, property.PropertyType, currentValue);
      }
      else
      {
        value = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, (useExistingValue) ? currentValue : null);
      }

      // always set the value if useExistingValue is false,
      // otherwise also set it if CreateValue returns a new value compared to the currentValue
      // this could happen because of a JsonConverter against the type
      if ((!useExistingValue || value != currentValue)
        && ShouldSetPropertyValue(property, value))
      {
        property.ValueProvider.SetValue(target, value);

        if (property.SetIsSpecified != null)
        {
          if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
            TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);

          property.SetIsSpecified(target, true);
        }
      }
    }

    private bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target, out bool useExistingValue, out object currentValue, out JsonContract propertyContract, out bool gottenCurrentValue)
    {
      currentValue = null;
      useExistingValue = false;
      propertyContract = null;
      gottenCurrentValue = false;

      if (property.Ignored)
      {
        reader.Skip();
        return true;
      }

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
        return true;
      }

      // test tokentype here because null might not be convertable to some types, e.g. ignoring null when applied to DateTime
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore && reader.TokenType == JsonToken.Null)
      {
        reader.Skip();
        return true;
      }

      // test tokentype here because default value might not be convertable to actual type, e.g. default of "" for DateTime
      if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
          && JsonReader.IsPrimitiveToken(reader.TokenType)
          && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
      {
        reader.Skip();
        return true;
      }

      if (property.PropertyContract == null)
        property.PropertyContract = GetContractSafe(property.PropertyType);

      if (currentValue == null)
      {
        propertyContract = property.PropertyContract;
      }
      else
      {
        propertyContract = GetContractSafe(currentValue.GetType());

        if (propertyContract != property.PropertyContract)
          propertyConverter = GetConverter(propertyContract, property.MemberConverter, containerContract, containerProperty);
      }

      return false;
    }

    private void AddReference(JsonReader reader, string id, object value)
    {
      try
      {
        if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
          TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Read object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, id, value.GetType())), null);

        Serializer.ReferenceResolver.AddReference(this, id, value);
      }
      catch (Exception ex)
      {
        throw JsonSerializationException.Create(reader, "Error reading object reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, id), ex);
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
        && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
        return false;

      if (!property.Writable)
        return false;

      return true;
    }

    public object CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool isTemporaryDictionary)
    {
      object dictionary;

      if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
      {
        dictionary = contract.DefaultCreator();
        isTemporaryDictionary = false;
      }
      else if (contract.IsReadOnlyDictionary)
      {
        dictionary = contract.CreateTemporaryDictionary();
        isTemporaryDictionary = true;
      }
      else
      {
        throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
      }

      return dictionary;
    }

    private void OnDeserializing(JsonReader reader, JsonContract contract, object value)
    {
      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);

      contract.InvokeOnDeserializing(value, Serializer.Context);
    }

    private void OnDeserialized(JsonReader reader, JsonContract contract, object value)
    {
      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);

      contract.InvokeOnDeserialized(value, Serializer.Context);
    }

    private object PopulateDictionary(IWrappedDictionary wrappedDictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty containerProperty, string id)
    {
      object dictionary = wrappedDictionary.UnderlyingDictionary;

      if (id != null)
        AddReference(reader, id, dictionary);

      OnDeserializing(reader, contract, dictionary);

      int initialDepth = reader.Depth;

      if (contract.KeyContract == null)
        contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);

      if (contract.ItemContract == null)
        contract.ItemContract = GetContractSafe(contract.DictionaryValueType);

      JsonConverter dictionaryValueConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);

      bool finished = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            object keyValue = reader.Value;
            try
            {
              try
              {
                if (contract.DictionaryKeyType == typeof(DateTime) && JsonConvert.TryParseDateTime(keyValue.ToString(), DateParseHandling.DateTime, reader.DateTimeZoneHandling, out keyValue))
                {
                }
#if !NET20
                else if (contract.DictionaryKeyType == typeof(DateTimeOffset) && JsonConvert.TryParseDateTime(keyValue.ToString(), DateParseHandling.DateTimeOffset, reader.DateTimeZoneHandling, out keyValue))
                {
                }
#endif
                else
                {
                  keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                }
              }
              catch (Exception ex)
              {
                throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
              }

              if (!ReadForType(reader, contract.ItemContract, dictionaryValueConverter != null))
                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");

              object itemValue;
              if (dictionaryValueConverter != null && dictionaryValueConverter.CanRead)
                itemValue = DeserializeConvertable(dictionaryValueConverter, reader, contract.DictionaryValueType, null);
              else
                itemValue = CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null);

              wrappedDictionary[keyValue] = itemValue;
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(dictionary, contract, keyValue, reader as IJsonLineInfo, reader.Path, ex))
                HandleError(reader, true, initialDepth);
              else
                throw;
            }
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            finished = true;
            break;
          default:
            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!finished && reader.Read());

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, dictionary, "Unexpected end when deserializing object.");

      OnDeserialized(reader, contract, dictionary);
      return dictionary;
    }

    private object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
    {
      int rank = contract.UnderlyingType.GetArrayRank();

      if (id != null)
        AddReference(reader, id, list);

      OnDeserializing(reader, contract, list);

      JsonContract collectionItemContract = GetContractSafe(contract.CollectionItemType);
      JsonConverter collectionItemConverter = GetConverter(collectionItemContract, null, contract, containerProperty);

      int? previousErrorIndex = null;
      Stack<IList> listStack = new Stack<IList>();
      listStack.Push(list);
      IList currentList = list;

      bool finished = false;
      do
      {
        int initialDepth = reader.Depth;

        if (listStack.Count == rank)
        {
          try
          {
            if (ReadForType(reader, collectionItemContract, collectionItemConverter != null))
            {
              switch (reader.TokenType)
              {
                case JsonToken.EndArray:
                  listStack.Pop();
                  currentList = listStack.Peek();
                  previousErrorIndex = null;
                  break;
                case JsonToken.Comment:
                  break;
                default:
                  object value;

                  if (collectionItemConverter != null && collectionItemConverter.CanRead)
                    value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                  else
                    value = CreateValueInternal(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null);

                  currentList.Add(value);
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
            JsonPosition errorPosition = reader.GetPosition(initialDepth);

            if (IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
            {
              HandleError(reader, true, initialDepth);

              if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
              {
                // reader index has not moved since previous error handling
                // break out of reading array to prevent infinite loop
                throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
              }
              else
              {
                previousErrorIndex = errorPosition.Position;
              }
            }
            else
            {
              throw;
            }
          }
        }
        else
        {
          if (reader.Read())
          {
            switch (reader.TokenType)
            {
              case JsonToken.StartArray:
                IList newList = new List<object>();
                currentList.Add(newList);
                listStack.Push(newList);
                currentList = newList;
                break;
              case JsonToken.EndArray:
                listStack.Pop();

                if (listStack.Count > 0)
                {
                  currentList = listStack.Peek();
                }
                else
                {
                  finished = true;
                }
                break;
              case JsonToken.Comment:
                break;
              default:
                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
            }
          }
          else
          {
            break;
          }
        }
      } while (!finished);

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");

      OnDeserialized(reader, contract, list);
      return list;
    }

    private void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object currentObject, string message)
    {
      try
      {
        throw JsonSerializationException.Create(reader, message);
      }
      catch (Exception ex)
      {
        if (IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
          HandleError(reader, false, 0);
        else
          throw;
      }
    }

    private object PopulateList(IWrappedCollection wrappedList, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
    {
      object list = wrappedList.UnderlyingCollection;

      if (id != null)
        AddReference(reader, id, list);

      // can't populate an existing array
      if (wrappedList.IsFixedSize)
      {
        reader.Skip();
        return list;
      }

      OnDeserializing(reader, contract, list);

      int initialDepth = reader.Depth;

      JsonContract collectionItemContract = GetContractSafe(contract.CollectionItemType);
      JsonConverter collectionItemConverter = GetConverter(collectionItemContract, null, contract, containerProperty);

      int? previousErrorIndex = null;

      bool finished = false;
      do
      {
        try
        {
          if (ReadForType(reader, collectionItemContract, collectionItemConverter != null))
          {
            switch (reader.TokenType)
            {
              case JsonToken.EndArray:
                finished = true;
                break;
              case JsonToken.Comment:
                break;
              default:
                object value;

                if (collectionItemConverter != null && collectionItemConverter.CanRead)
                  value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                else
                  value = CreateValueInternal(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null);

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
          JsonPosition errorPosition = reader.GetPosition(initialDepth);

          if (IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
          {
            HandleError(reader, true, initialDepth);

            if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
            {
              // reader index has not moved since previous error handling
              // break out of reading array to prevent infinite loop
              throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
            }
            else
            {
              previousErrorIndex = errorPosition.Position;
            }
          }
          else
          {
            throw;
          }
        }
      } while (!finished);

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");

      OnDeserialized(reader, contract, list);
      return list;
    }

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
    private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, string id)
    {
      Type objectType = contract.UnderlyingType;

      if (!JsonTypeReflector.FullyTrusted)
      {
        throw JsonSerializationException.Create(reader, @"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data.
To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true.
".FormatWith(CultureInfo.InvariantCulture, objectType));
      }

      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, GetFormatterConverter());

      bool finished = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();
            if (!reader.Read())
              throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

            serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            finished = true;
            break;
          default:
            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!finished && reader.Read());

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");

      if (contract.ISerializableCreator == null)
        throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, objectType));

      object createdObject = contract.ISerializableCreator(serializationInfo, Serializer.Context);

      if (id != null)
        AddReference(reader, id, createdObject);

      // these are together because OnDeserializing takes an object but for an ISerializable the object is fully created in the constructor
      OnDeserializing(reader, contract, createdObject);
      OnDeserialized(reader, contract, createdObject);

      return createdObject;
    }
#endif

#if !(NET35 || NET20)
    private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, JsonProperty member, string id)
    {
      IDynamicMetaObjectProvider newObject;

      if (contract.UnderlyingType.IsInterface() || contract.UnderlyingType.IsAbstract())
        throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (contract.DefaultCreator != null &&
        (!contract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
        newObject = (IDynamicMetaObjectProvider) contract.DefaultCreator();
      else
        throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));

      if (id != null)
        AddReference(reader, id, newObject);

      OnDeserializing(reader, contract, newObject);

      int initialDepth = reader.Depth;

      bool finished = false;
      do
      {
        switch (reader.TokenType)
        {
          case JsonToken.PropertyName:
            string memberName = reader.Value.ToString();

            try
            {
              if (!reader.Read())
                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              // first attempt to find a settable property, otherwise fall back to a dynamic set without type
              JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

              if (property != null && property.Writable && !property.Ignored)
              {
                if (property.PropertyContract == null)
                  property.PropertyContract = GetContractSafe(property.PropertyType);

                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, null, null);

                SetPropertyValue(property, propertyConverter, null, member, reader, newObject);
              }
              else
              {
                Type t = (JsonReader.IsPrimitiveToken(reader.TokenType)) ? reader.ValueType : typeof (IDynamicMetaObjectProvider);

                JsonContract dynamicMemberContract = GetContractSafe(t);
                JsonConverter dynamicMemberConverter = GetConverter(dynamicMemberContract, null, null, member);

                object value;
                if (dynamicMemberConverter != null && dynamicMemberConverter.CanRead)
                  value = DeserializeConvertable(dynamicMemberConverter, reader, t, null);
                else
                  value = CreateValueInternal(reader, t, dynamicMemberContract, null, null, member, null);

                contract.TrySetMember(newObject, memberName, value);
              }
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                HandleError(reader, true, initialDepth);
              else
                throw;
            }
            break;
          case JsonToken.EndObject:
            finished = true;
            break;
          default:
            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!finished && reader.Read());

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");

      OnDeserialized(reader, contract, newObject);

      return newObject;
    }
#endif

    private object CreateObjectFromNonDefaultConstructor(JsonReader reader, JsonObjectContract contract, JsonProperty containerProperty, ConstructorInfo constructorInfo, string id)
    {
      ValidationUtils.ArgumentNotNull(constructorInfo, "constructorInfo");

      Type objectType = contract.UnderlyingType;

      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using a non-default constructor '{1}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, constructorInfo)), null);

      IDictionary<JsonProperty, object> propertyValues = ResolvePropertyAndConstructorValues(contract, containerProperty, reader, objectType);

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
        AddReference(reader, id, createdObject);

      OnDeserializing(reader, contract, createdObject);

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
            JsonArrayContract propertyArrayContract = (JsonArrayContract)propertyContract;

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
            JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)propertyContract;

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

      OnDeserialized(reader, contract, createdObject);
      return createdObject;
    }

    private object DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object existingValue)
    {
      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);

      object value = converter.ReadJson(reader, objectType, existingValue, GetInternalSerializer());

      if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Info)
        TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);

      return value;
    }

    private IDictionary<JsonProperty, object> ResolvePropertyAndConstructorValues(JsonObjectContract contract, JsonProperty containerProperty, JsonReader reader, Type objectType)
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

              JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, containerProperty);

              if (!ReadForType(reader, property.PropertyContract, propertyConverter != null))
                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (!property.Ignored)
              {
                if (property.PropertyContract == null)
                  property.PropertyContract = GetContractSafe(property.PropertyType);

                object propertyValue;
                if (propertyConverter != null && propertyConverter.CanRead)
                  propertyValue = DeserializeConvertable(propertyConverter, reader, property.PropertyType, null);
                else
                  propertyValue = CreateValueInternal(reader, property.PropertyType, property.PropertyContract, property, contract, containerProperty, null);

                propertyValues[property] = propertyValue;
              }
              else
              {
                reader.Skip();
              }
            }
            else
            {
              if (!reader.Read())
                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

              if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);

              if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));

              reader.Skip();
            }
            break;
          case JsonToken.Comment:
            break;
          case JsonToken.EndObject:
            exit = true;
            break;
          default:
            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!exit && reader.Read());

      return propertyValues;
    }

    private bool ReadForType(JsonReader reader, JsonContract contract, bool hasConverter)
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

    public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty containerMember, JsonProperty containerProperty, string id, out bool createdFromNonDefaultConstructor)
    {
      object newObject = null;

      if (objectContract.UnderlyingType.IsInterface() || objectContract.UnderlyingType.IsAbstract())
        throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));

      if (objectContract.OverrideConstructor != null)
      {
        if (objectContract.OverrideConstructor.GetParameters().Length > 0)
        {
          createdFromNonDefaultConstructor = true;
          return CreateObjectFromNonDefaultConstructor(reader, objectContract, containerMember, objectContract.OverrideConstructor, id);
        }

        newObject = objectContract.OverrideConstructor.Invoke(null);
      }
      else if (objectContract.DefaultCreator != null &&
        (!objectContract.DefaultCreatorNonPublic || Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParametrizedConstructor == null))
      {
        // use the default constructor if it is...
        // public
        // non-public and the user has change constructor handling settings
        // non-public and there is no other constructor
        newObject = objectContract.DefaultCreator();
      }
      else if (objectContract.ParametrizedConstructor != null)
      {
        createdFromNonDefaultConstructor = true;
        return CreateObjectFromNonDefaultConstructor(reader, objectContract, containerMember, objectContract.ParametrizedConstructor, id);
      }

      if (newObject == null)
        throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));

      createdFromNonDefaultConstructor = false;
      return newObject;
    }

    private object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty member, string id)
    {
      OnDeserializing(reader, contract, newObject);

      // only need to keep a track of properies presence if they are required or a value should be defaulted if missing
      Dictionary<JsonProperty, PropertyPresence> propertiesPresence = (contract.HasRequiredOrDefaultValueProperties || HasFlag(Serializer.DefaultValueHandling, DefaultValueHandling.Populate))
        ? contract.Properties.ToDictionary(m => m, m => PropertyPresence.None)
        : null;

      if (id != null)
        AddReference(reader, id, newObject);

      int initialDepth = reader.Depth;

      bool finished = false;
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
                  if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                    TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);

                  if (Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                    throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));

                  reader.Skip();
                  continue;
                }

                if (property.PropertyContract == null)
                  property.PropertyContract = GetContractSafe(property.PropertyType);

                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, member);

                if (!ReadForType(reader, property.PropertyContract, propertyConverter != null))
                  throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));

                SetPropertyPresence(reader, property, propertiesPresence);

                SetPropertyValue(property, propertyConverter, contract, member, reader, newObject);
              }
              catch (Exception ex)
              {
                if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                  HandleError(reader, true, initialDepth);
                else
                  throw;
              }
            }
            break;
          case JsonToken.EndObject:
            finished = true;
            break;
          case JsonToken.Comment:
            // ignore
            break;
          default:
            throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
        }
      } while (!finished && reader.Read());

      if (!finished)
        ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");

      EndObject(newObject, reader, contract, initialDepth, propertiesPresence);

      OnDeserialized(reader, contract, newObject);
      return newObject;
    }

    private void EndObject(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, Dictionary<JsonProperty, PropertyPresence> propertiesPresence)
    {
      if (propertiesPresence != null)
      {
        foreach (KeyValuePair<JsonProperty, PropertyPresence> propertyPresence in propertiesPresence)
        {
          JsonProperty property = propertyPresence.Key;
          PropertyPresence presence = propertyPresence.Value;

          if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
          {
            try
            {
              Required resolvedRequired = property._required ?? contract.ItemRequired ?? Required.Default;

              switch (presence)
              {
                case PropertyPresence.None:
                  if (resolvedRequired == Required.AllowNull || resolvedRequired == Required.Always)
                    throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));

                  if (property.PropertyContract == null)
                    property.PropertyContract = GetContractSafe(property.PropertyType);

                  if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
                    property.ValueProvider.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
                  break;
                case PropertyPresence.Null:
                  if (resolvedRequired == Required.Always)
                    throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                  break;
              }
            }
            catch (Exception ex)
            {
              if (IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
                HandleError(reader, true, initialDepth);
              else
                throw;
            }
          }
        }
      }
    }

    private void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence> requiredProperties)
    {
      if (property != null && requiredProperties != null)
      {
        requiredProperties[property] = (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
          ? PropertyPresence.Null
          : PropertyPresence.Value;
      }
    }

    private void HandleError(JsonReader reader, bool readPastError, int initialDepth)
    {
      ClearErrorContext();

      if (readPastError)
      {
        reader.Skip();

        while (reader.Depth > (initialDepth + 1))
        {
          if (!reader.Read())
            break;
        }
      }
    }
  }
}