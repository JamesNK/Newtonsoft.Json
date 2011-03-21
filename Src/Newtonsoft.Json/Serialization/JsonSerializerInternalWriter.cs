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
using System.ComponentModel;
#if !(NET35 || NET20 || WINDOWS_PHONE)
using System.Dynamic;
#endif
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
using System.Security;

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
  {
    private JsonSerializerProxy _internalSerializer;
    private List<object> _serializeStack;

    private List<object> SerializeStack
    {
      get
      {
        if (_serializeStack == null)
          _serializeStack = new List<object>();

        return _serializeStack;
      }
    }

    public JsonSerializerInternalWriter(JsonSerializer serializer)
      : base(serializer)
    {
    }

    public void Serialize(JsonWriter jsonWriter, object value)
    {
      if (jsonWriter == null)
        throw new ArgumentNullException("jsonWriter");

      SerializeValue(jsonWriter, value, GetContractSafe(value), null, null);
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

    private JsonContract GetContractSafe(object value)
    {
      if (value == null)
        return null;

      return Serializer.ContractResolver.ResolveContract(value.GetType());
    }

    private void SerializeValue(JsonWriter writer, object value, JsonContract valueContract, JsonProperty member, JsonContract collectionValueContract)
    {
      JsonConverter converter = (member != null) ? member.Converter : null;

      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      if ((converter != null
          || ((converter = valueContract.Converter) != null)
          || ((converter = Serializer.GetMatchingConverter(valueContract.UnderlyingType)) != null)
          || ((converter = valueContract.InternalConverter) != null))
        && converter.CanWrite)
      {
        SerializeConvertable(writer, converter, value, valueContract);
      }
      else if (valueContract is JsonPrimitiveContract)
      {
        writer.WriteValue(value);
      }
      else if (valueContract is JsonStringContract)
      {
        SerializeString(writer, value, (JsonStringContract)valueContract);
      }
      else if (valueContract is JsonObjectContract)
      {
        SerializeObject(writer, value, (JsonObjectContract)valueContract, member, collectionValueContract);
      }
      else if (valueContract is JsonDictionaryContract)
      {
        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)valueContract;
        SerializeDictionary(writer, dictionaryContract.CreateWrapper(value), dictionaryContract, member, collectionValueContract);
      }
      else if (valueContract is JsonArrayContract)
      {
        JsonArrayContract arrayContract = (JsonArrayContract)valueContract;
        SerializeList(writer, arrayContract.CreateWrapper(value), arrayContract, member, collectionValueContract);
      }
      else if (valueContract is JsonLinqContract)
      {
        ((JToken)value).WriteTo(writer, (Serializer.Converters != null) ? Serializer.Converters.ToArray() : null);
      }
#if !SILVERLIGHT && !PocketPC
      else if (valueContract is JsonISerializableContract)
      {
        SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract);
      }
#endif
#if !(NET35 || NET20 || WINDOWS_PHONE)
      else if (valueContract is JsonDynamicContract)
      {
        SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract);
      }
#endif
    }

    private bool ShouldWriteReference(object value, JsonProperty property, JsonContract contract)
    {
      if (value == null)
        return false;
      if (contract is JsonPrimitiveContract)
        return false;

      bool? isReference = null;

      // value could be coming from a dictionary or array and not have a property
      if (property != null)
        isReference = property.IsReference;

      if (isReference == null)
        isReference = contract.IsReference;

      if (isReference == null)
      {
        if (contract is JsonArrayContract)
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
        else
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      }

      if (!isReference.Value)
        return false;

      return Serializer.ReferenceResolver.IsReferenced(value);
    }

    private void WriteMemberInfoProperty(JsonWriter writer, object memberValue, JsonProperty property, JsonContract contract)
    {
      string propertyName = property.PropertyName;
      object defaultValue = property.DefaultValue;

      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore &&
          memberValue == null)
        return;

      if (property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling) ==
          DefaultValueHandling.Ignore && Equals(memberValue, defaultValue))
        return;

      if (ShouldWriteReference(memberValue, property, contract))
      {
        writer.WritePropertyName(propertyName);
        WriteReference(writer, memberValue);
        return;
      }

      if (!CheckForCircularReference(memberValue, property.ReferenceLoopHandling, contract))
        return;

      if (memberValue == null && property.Required == Required.Always)
        throw new JsonSerializationException("Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));

      writer.WritePropertyName(propertyName);
      SerializeValue(writer, memberValue, contract, property, null);
    }

    private bool CheckForCircularReference(object value, ReferenceLoopHandling? referenceLoopHandling, JsonContract contract)
    {
      if (value == null || contract is JsonPrimitiveContract)
        return true;

      if (SerializeStack.IndexOf(value) != -1)
      {
        switch (referenceLoopHandling.GetValueOrDefault(Serializer.ReferenceLoopHandling))
        {
          case ReferenceLoopHandling.Error:
            throw new JsonSerializationException("Self referencing loop detected for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
          case ReferenceLoopHandling.Ignore:
            return false;
          case ReferenceLoopHandling.Serialize:
            return true;
          default:
            throw new InvalidOperationException("Unexpected ReferenceLoopHandling value: '{0}'".FormatWith(CultureInfo.InvariantCulture, Serializer.ReferenceLoopHandling));
        }
      }

      return true;
    }

    private void WriteReference(JsonWriter writer, object value)
    {
      writer.WriteStartObject();
      writer.WritePropertyName(JsonTypeReflector.RefPropertyName);
      writer.WriteValue(Serializer.ReferenceResolver.GetReference(value));
      writer.WriteEndObject();
    }

    internal static bool TryConvertToString(object value, Type type, out string s)
    {
#if !PocketPC
      TypeConverter converter = ConvertUtils.GetConverter(type);

      // use the objectType's TypeConverter if it has one and can convert to a string
      if (converter != null
#if !SILVERLIGHT
 && !(converter is ComponentConverter)
#endif
 && converter.GetType() != typeof(TypeConverter))
      {
        if (converter.CanConvertTo(typeof(string)))
        {
#if !SILVERLIGHT
          s = converter.ConvertToInvariantString(value);
#else
          s = converter.ConvertToString(value);
#endif
          return true;
        }
      }
#endif

#if SILVERLIGHT || PocketPC
      if (value is Guid || value is Uri || value is TimeSpan)
      {
        s = value.ToString();
        return true;
      }
#endif

      if (value is Type)
      {
        s = ((Type)value).AssemblyQualifiedName;
        return true;
      }

      s = null;
      return false;
    }

    private void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);

      string s;
      TryConvertToString(value, contract.UnderlyingType, out s);
      writer.WriteValue(s);

      contract.InvokeOnSerialized(value, Serializer.Context);
    }

    private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty member, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);

      SerializeStack.Add(value);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(value));
      }
      if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionValueContract))
      {
        WriteTypeProperty(writer, contract.UnderlyingType);
      }

      int initialDepth = writer.Top;

      foreach (JsonProperty property in contract.Properties)
      {
        try
        {
          if (!property.Ignored && property.Readable && ShouldSerialize(property, value) && IsSpecified(property, value))
          {
            object memberValue = property.ValueProvider.GetValue(value);
            JsonContract memberContract = GetContractSafe(memberValue);

            WriteMemberInfoProperty(writer, memberValue, property, memberContract);
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(value, contract, property.PropertyName, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndObject();
      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(value, Serializer.Context);
    }

    private void WriteTypeProperty(JsonWriter writer, Type type)
    {
      writer.WritePropertyName(JsonTypeReflector.TypePropertyName);
      writer.WriteValue(ReflectionUtils.GetTypeName(type, Serializer.TypeNameAssemblyFormat));
    }

    private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
    {
      return ((value & flag) == flag);
    }

    private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
    {
      return ((value & flag) == flag);
    }

    private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract)
    {
      if (ShouldWriteReference(value, null, contract))
      {
        WriteReference(writer, value);
      }
      else
      {
        if (!CheckForCircularReference(value, null, contract))
          return;

        SerializeStack.Add(value);

        converter.WriteJson(writer, value, GetInternalSerializer());

        SerializeStack.RemoveAt(SerializeStack.Count - 1);
      }
    }

    private void SerializeList(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(values.UnderlyingCollection, Serializer.Context);

      SerializeStack.Add(values.UnderlyingCollection);

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
      bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, collectionValueContract);

      if (isReference || includeTypeDetails)
      {
        writer.WriteStartObject();

        if (isReference)
        {
          writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
          writer.WriteValue(Serializer.ReferenceResolver.GetReference(values.UnderlyingCollection));
        }
        if (includeTypeDetails)
        {
          WriteTypeProperty(writer, values.UnderlyingCollection.GetType());
        }
        writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName);
      }

      JsonContract childValuesContract = Serializer.ContractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));

      writer.WriteStartArray();

      int initialDepth = writer.Top;

      int index = 0;
      // note that an error in the IEnumerable won't be caught
      foreach (object value in values)
      {
        try
        {
          JsonContract valueContract = GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract))
          {
            WriteReference(writer, value);
          }
          else
          {
            if (CheckForCircularReference(value, null, contract))
            {
              SerializeValue(writer, value, valueContract, null, childValuesContract);
            }
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values.UnderlyingCollection, contract, index, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
        finally
        {
          index++;
        }
      }

      writer.WriteEndArray();

      if (isReference || includeTypeDetails)
      {
        writer.WriteEndObject();
      }

      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values.UnderlyingCollection, Serializer.Context);
    }

#if !SILVERLIGHT && !PocketPC
#if !NET20
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [SecuritySafeCritical]
#endif
    private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);
      SerializeStack.Add(value);

      writer.WriteStartObject();

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
      value.GetObjectData(serializationInfo, Serializer.Context);

      foreach (SerializationEntry serializationEntry in serializationInfo)
      {
        writer.WritePropertyName(serializationEntry.Name);
        SerializeValue(writer, serializationEntry.Value, GetContractSafe(serializationEntry.Value), null, null);
      }

      writer.WriteEndObject();

      SerializeStack.RemoveAt(SerializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

#if !(NET35 || NET20 || WINDOWS_PHONE)
    /// <summary>
    /// Serializes the dynamic.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="contract">The contract.</param>
    private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);
      SerializeStack.Add(value);

      writer.WriteStartObject();

      foreach (string memberName in value.GetDynamicMemberNames())
      {
        object memberValue;
        if (DynamicUtils.TryGetMember(value, memberName, out memberValue))
        {
          string resolvedPropertyName = (contract.PropertyNameResolver != null)
            ? contract.PropertyNameResolver(memberName)
            : memberName;
          
          writer.WritePropertyName(resolvedPropertyName);
          SerializeValue(writer, memberValue, GetContractSafe(memberValue), null, null);
        }
      }

      writer.WriteEndObject();

      SerializeStack.RemoveAt(SerializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

    private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty member, JsonContract collectionValueContract)
    {
      if (HasFlag(((member != null) ? member.TypeNameHandling : null) ?? Serializer.TypeNameHandling, typeNameHandlingFlag))
        return true;

      if (member != null)
      {
        if ((member.TypeNameHandling ?? Serializer.TypeNameHandling) == TypeNameHandling.Auto && contract.UnderlyingType != member.PropertyType)
          return true;
      }
      else if (collectionValueContract != null)
      {
        if (Serializer.TypeNameHandling == TypeNameHandling.Auto && contract.UnderlyingType != collectionValueContract.UnderlyingType)
          return true;
      }

      return false;
    }

    private void SerializeDictionary(JsonWriter writer, IWrappedDictionary values, JsonDictionaryContract contract, JsonProperty member, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(values.UnderlyingDictionary, Serializer.Context);

      SerializeStack.Add(values.UnderlyingDictionary);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(values.UnderlyingDictionary));
      }
      if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionValueContract))
      {
        WriteTypeProperty(writer, values.UnderlyingDictionary.GetType());
      }

      JsonContract childValuesContract = Serializer.ContractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));

      int initialDepth = writer.Top;

      // Mono Unity 3.0 fix
      IDictionary d = values;

      foreach (DictionaryEntry entry in d)
      {
        string propertyName = GetPropertyName(entry);

        propertyName = (contract.PropertyNameResolver != null)
                         ? contract.PropertyNameResolver(propertyName)
                         : propertyName;

        try
        {
          object value = entry.Value;
          JsonContract valueContract = GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract))
          {
            writer.WritePropertyName(propertyName);
            WriteReference(writer, value);
          }
          else
          {
            if (!CheckForCircularReference(value, null, contract))
              continue;

            writer.WritePropertyName(propertyName);

            SerializeValue(writer, value, valueContract, null, childValuesContract);
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values.UnderlyingDictionary, contract, propertyName, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndObject();
      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values.UnderlyingDictionary, Serializer.Context);
    }

    private string GetPropertyName(DictionaryEntry entry)
    {
      string propertyName;

      if (entry.Key is IConvertible)
        return Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
      else if (TryConvertToString(entry.Key, entry.Key.GetType(), out propertyName))
        return propertyName;
      else
        return entry.Key.ToString();
    }

    private void HandleError(JsonWriter writer, int initialDepth)
    {
      ClearErrorContext();

      while (writer.Top > initialDepth)
      {
        writer.WriteEnd();
      }
    }

    private bool ShouldSerialize(JsonProperty property, object target)
    {
      if (property.ShouldSerialize == null)
        return true;

      return property.ShouldSerialize(target);
    }

    private bool IsSpecified(JsonProperty property, object target)
    {
      if (property.GetIsSpecified == null)
        return true;

      return property.GetIsSpecified(target);
    }
  }
}