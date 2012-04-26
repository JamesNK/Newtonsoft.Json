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
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
using System.Dynamic;
#endif
using System.Globalization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
  {
    private readonly List<object> _serializeStack = new List<object>();
    private JsonSerializerProxy _internalSerializer;

    public JsonSerializerInternalWriter(JsonSerializer serializer)
      : base(serializer)
    {
    }

    public void Serialize(JsonWriter jsonWriter, object value)
    {
      if (jsonWriter == null)
        throw new ArgumentNullException("jsonWriter");

      SerializeValue(jsonWriter, value, GetContractSafe(value), null, null, null);
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

    private void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      if (contract.UnderlyingType == typeof (byte[]))
      {
        bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, collectionValueContract);
        if (includeTypeDetails)
        {
          writer.WriteStartObject();
          WriteTypeProperty(writer, contract.CreatedType);
          writer.WritePropertyName(JsonTypeReflector.ValuePropertyName);
          writer.WriteValue(value);
          writer.WriteEndObject();
          return;
        }
      }

      writer.WriteValue(value);
    }

    private void SerializeValue(JsonWriter writer, object value, JsonContract valueContract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      JsonConverter converter;
      if ((((converter = (member != null) ? member.Converter : null) != null)
           || ((converter = (collectionContract != null) ? collectionContract.ItemConverter : null) != null)
           || ((converter = valueContract.Converter) != null)
           || ((converter = Serializer.GetMatchingConverter(valueContract.UnderlyingType)) != null)
           || ((converter = valueContract.InternalConverter) != null))
          && converter.CanWrite)
      {
        SerializeConvertable(writer, converter, value, valueContract, collectionContract, collectionValueContract);
        return;
      }

      switch (valueContract.ContractType)
      {
        case JsonContractType.Object:
          SerializeObject(writer, value, (JsonObjectContract) valueContract, member, collectionContract, collectionValueContract);
          break;
        case JsonContractType.Array:
          JsonArrayContract arrayContract = (JsonArrayContract) valueContract;
          SerializeList(writer, arrayContract.CreateWrapper(value), arrayContract, member, collectionContract, collectionValueContract);
          break;
        case JsonContractType.Primitive:
          SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, collectionContract, collectionValueContract);
          break;
        case JsonContractType.String:
          SerializeString(writer, value, (JsonStringContract) valueContract);
          break;
        case JsonContractType.Dictionary:
          JsonDictionaryContract dictionaryContract = (JsonDictionaryContract) valueContract;
          SerializeDictionary(writer, dictionaryContract.CreateWrapper(value), dictionaryContract, member, collectionContract, collectionValueContract);
          break;
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
        case JsonContractType.Dynamic:
          SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, collectionContract, collectionValueContract);
          break;
#endif
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
        case JsonContractType.Serializable:
          SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, collectionContract, collectionValueContract);
          break;
#endif
        case JsonContractType.Linq:
          ((JToken) value).WriteTo(writer, (Serializer.Converters != null) ? Serializer.Converters.ToArray() : null);
          break;
      }
    }

    private bool? ResolveIsReference(JsonContract contract, JsonProperty property, JsonContainerContract collectionContract)
    {
      bool? isReference = null;

      // value could be coming from a dictionary or array and not have a property
      if (property != null)
        isReference = property.IsReference;

      if (isReference == null && collectionContract != null)
        isReference = collectionContract.ItemIsReference;

      if (isReference == null)
        isReference = contract.IsReference;

      return isReference;
    }

    private bool ShouldWriteReference(object value, JsonProperty property, JsonContract valueContract, JsonContainerContract collectionContract)
    {
      if (value == null)
        return false;
      if (valueContract.ContractType == JsonContractType.Primitive || valueContract.ContractType == JsonContractType.String)
        return false;

      bool? isReference = ResolveIsReference(valueContract, property, collectionContract);

      if (isReference == null)
      {
        if (valueContract.ContractType == JsonContractType.Array)
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
        else
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      }

      if (!isReference.Value)
        return false;

      return Serializer.ReferenceResolver.IsReferenced(this, value);
    }

    private void WriteMemberInfoProperty(JsonWriter writer, object memberValue, JsonProperty property, JsonContract contract, JsonContainerContract collectionContract)
    {
      string propertyName = property.PropertyName;

      if (ShouldWriteReference(memberValue, property, contract, collectionContract))
      {
        writer.WritePropertyName(propertyName);
        WriteReference(writer, memberValue);
        return;
      }

      if (!CheckForCircularReference(memberValue, property, contract, collectionContract))
        return;

      if (memberValue == null && property.Required == Required.Always)
        throw new JsonSerializationException("Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));

      writer.WritePropertyName(propertyName);
      SerializeValue(writer, memberValue, contract, property, collectionContract, null);
    }

    private bool ShouldWriteProperty(object memberValue, JsonProperty property)
    {
      if (property.NullValueHandling.GetValueOrDefault(Serializer.NullValueHandling) == NullValueHandling.Ignore &&
          memberValue == null)
        return false;

      if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
          && MiscellaneousUtils.ValueEquals(memberValue, property.DefaultValue))
        return false;

      return true;
    }

    private bool CheckForCircularReference(object value, JsonProperty property, JsonContract contract, JsonContainerContract collectionContract)
    {
      if (value == null || contract.ContractType == JsonContractType.Primitive || contract.ContractType == JsonContractType.String)
        return true;

      ReferenceLoopHandling? referenceLoopHandling = null;

      if (property != null)
        referenceLoopHandling = property.ReferenceLoopHandling;

      if (referenceLoopHandling == null && collectionContract != null)
        referenceLoopHandling = collectionContract.ItemReferenceLoopHandling;

      if (_serializeStack.IndexOf(value) != -1)
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
      writer.WriteValue(Serializer.ReferenceResolver.GetReference(this, value));
      writer.WriteEndObject();
    }

    internal static bool TryConvertToString(object value, Type type, out string s)
    {
#if !(PocketPC || NETFX_CORE || PORTABLE)
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

#if SILVERLIGHT || PocketPC || NETFX_CORE
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

    private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);

      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, collectionValueContract);

      int initialDepth = writer.Top;

      foreach (JsonProperty property in contract.Properties)
      {
        try
        {
          if (!property.Ignored && property.Readable && ShouldSerialize(property, value) && IsSpecified(property, value))
          {
            if (property.PropertyContract == null)
              property.PropertyContract = Serializer.ContractResolver.ResolveContract(property.PropertyType);

            object memberValue = property.ValueProvider.GetValue(value);
            JsonContract memberContract = (property.PropertyContract.UnderlyingType.IsSealed) ? property.PropertyContract : GetContractSafe(memberValue);

            if (ShouldWriteProperty(memberValue, property))
            {
              WriteMemberInfoProperty(writer, memberValue, property, memberContract, contract);
            }
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(value, contract, property.PropertyName, writer.ContainerPath, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);

      contract.InvokeOnSerialized(value, Serializer.Context);
    }

    private void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      writer.WriteStartObject();

      bool isReference = ResolveIsReference(contract, member, collectionContract) ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(this, value));
      }
      if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, collectionValueContract))
      {
        WriteTypeProperty(writer, contract.UnderlyingType);
      }
    }

    private void WriteTypeProperty(JsonWriter writer, Type type)
    {
      writer.WritePropertyName(JsonTypeReflector.TypePropertyName);
      writer.WriteValue(ReflectionUtils.GetTypeName(type, Serializer.TypeNameAssemblyFormat, Serializer.Binder));
    }

    private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
    {
      return ((value & flag) == flag);
    }

    private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
    {
      return ((value & flag) == flag);
    }

    private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
    {
      return ((value & flag) == flag);
    }

    private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      if (ShouldWriteReference(value, null, contract, collectionContract))
      {
        WriteReference(writer, value);
      }
      else
      {
        if (!CheckForCircularReference(value, null, contract, collectionContract))
          return;

        _serializeStack.Add(value);

        converter.WriteJson(writer, value, GetInternalSerializer());

        _serializeStack.RemoveAt(_serializeStack.Count - 1);
      }
    }

    private void SerializeList(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(values.UnderlyingCollection, Serializer.Context);

      _serializeStack.Add(values.UnderlyingCollection);

      bool hasWrittenMetadataObject = WriteStartArray(writer, values, contract, member, collectionContract, collectionValueContract);

      writer.WriteStartArray();

      JsonContract collectionItemValueContract = (contract.CollectionItemContract.UnderlyingType.IsSealed) ? contract.CollectionItemContract : null;

      int initialDepth = writer.Top;

      int index = 0;
      // note that an error in the IEnumerable won't be caught
      foreach (object value in values)
      {
        try
        {
          JsonContract valueContract = collectionItemValueContract ?? GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract, contract))
          {
            WriteReference(writer, value);
          }
          else
          {
            if (CheckForCircularReference(value, null, valueContract, contract))
            {
              SerializeValue(writer, value, valueContract, null, contract, contract.CollectionItemContract);
            }
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values.UnderlyingCollection, contract, index, writer.ContainerPath, ex))
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

      if (hasWrittenMetadataObject)
        writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);

      contract.InvokeOnSerialized(values.UnderlyingCollection, Serializer.Context);
    }

    private bool WriteStartArray(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      bool isReference = ResolveIsReference(contract, member, collectionContract) ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
      bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, collectionContract, collectionValueContract);
      bool writeMetadataObject = isReference || includeTypeDetails;

      if (writeMetadataObject)
      {
        writer.WriteStartObject();

        if (isReference)
        {
          writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
          writer.WriteValue(Serializer.ReferenceResolver.GetReference(this, values.UnderlyingCollection));
        }
        if (includeTypeDetails)
        {
          WriteTypeProperty(writer, values.UnderlyingCollection.GetType());
        }
        writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName);
      }

      if (contract.CollectionItemContract == null)
        contract.CollectionItemContract = Serializer.ContractResolver.ResolveContract(contract.CollectionItemType ?? typeof (object));

      return writeMetadataObject;
    }

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
#if !NET20
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [SecuritySafeCritical]
#endif
    private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      if (!JsonTypeReflector.FullyTrusted)
      {
        throw new JsonSerializationException(@"Type '{0}' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data.
To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add to JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
      }

      contract.InvokeOnSerializing(value, Serializer.Context);
      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, collectionValueContract);

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
      value.GetObjectData(serializationInfo, Serializer.Context);

      foreach (SerializationEntry serializationEntry in serializationInfo)
      {
        writer.WritePropertyName(serializationEntry.Name);
        SerializeValue(writer, serializationEntry.Value, GetContractSafe(serializationEntry.Value), null, null, null);
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
    private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);
      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, collectionValueContract);

      foreach (string memberName in value.GetDynamicMemberNames())
      {
        object memberValue;
        if (DynamicUtils.TryGetMember(value, memberName, out memberValue))
        {
          string resolvedPropertyName = (contract.PropertyNameResolver != null)
                                          ? contract.PropertyNameResolver(memberName)
                                          : memberName;

          writer.WritePropertyName(resolvedPropertyName);
          SerializeValue(writer, memberValue, GetContractSafe(memberValue), null, null, null);
        }
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

    private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      TypeNameHandling resolvedTypeNameHandling = ((member != null) ? member.TypeNameHandling : null)
        ?? ((collectionContract != null) ? collectionContract.ItemTypeNameHandling : null) 
        ?? Serializer.TypeNameHandling;

      if (HasFlag(resolvedTypeNameHandling, typeNameHandlingFlag))
        return true;

      if (member != null)
      {
        if ((member.TypeNameHandling ?? Serializer.TypeNameHandling) == TypeNameHandling.Auto
          // instance and property type are different
          && contract.UnderlyingType != member.PropertyType)
        {
          JsonContract memberTypeContract = Serializer.ContractResolver.ResolveContract(member.PropertyType);
          // instance type and the property's type's contract default type are different (no need to put the type in JSON because the type will be created by default)
          if (contract.UnderlyingType != memberTypeContract.CreatedType)
            return true;
        }
      }
      else if (collectionValueContract != null)
      {
        if (Serializer.TypeNameHandling == TypeNameHandling.Auto && contract.UnderlyingType != collectionValueContract.UnderlyingType)
          return true;
      }

      return false;
    }

    private void SerializeDictionary(JsonWriter writer, IWrappedDictionary values, JsonDictionaryContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonContract collectionValueContract)
    {
      contract.InvokeOnSerializing(values.UnderlyingDictionary, Serializer.Context);

      _serializeStack.Add(values.UnderlyingDictionary);

      WriteObjectStart(writer, values.UnderlyingDictionary, contract, member, collectionContract, collectionValueContract);

      if (contract.DictionaryValueContract == null)
        contract.DictionaryValueContract = Serializer.ContractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));

      JsonContract dictionaryValueContract = (contract.DictionaryValueContract.UnderlyingType.IsSealed) ? contract.DictionaryValueContract : null;

      int initialDepth = writer.Top;

      // Mono Unity 3.0 fix
      IWrappedDictionary d = values;

      foreach (DictionaryEntry entry in d)
      {
        string propertyName = GetPropertyName(entry);

        propertyName = (contract.PropertyNameResolver != null)
                         ? contract.PropertyNameResolver(propertyName)
                         : propertyName;

        try
        {
          object value = entry.Value;
          JsonContract valueContract = dictionaryValueContract ?? GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract, contract))
          {
            writer.WritePropertyName(propertyName);
            WriteReference(writer, value);
          }
          else
          {
            if (!CheckForCircularReference(value, null, valueContract, contract))
              continue;

            writer.WritePropertyName(propertyName);

            SerializeValue(writer, value, valueContract, null, contract, dictionaryValueContract);
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values.UnderlyingDictionary, contract, propertyName, writer.ContainerPath, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);

      contract.InvokeOnSerialized(values.UnderlyingDictionary, Serializer.Context);
    }

    private string GetPropertyName(DictionaryEntry entry)
    {
      string propertyName;

      if (ConvertUtils.IsConvertible(entry.Key))
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