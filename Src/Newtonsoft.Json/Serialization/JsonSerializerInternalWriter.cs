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

    private void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      if (contract.UnderlyingType == typeof (byte[]))
      {
        bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty);
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

    private void SerializeValue(JsonWriter writer, object value, JsonContract valueContract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      JsonConverter converter;
      if ((((converter = (member != null) ? member.Converter : null) != null)
           || ((converter = (containerProperty != null) ? containerProperty.ItemConverter : null) != null)
           || ((converter = (containerContract != null) ? containerContract.ItemConverter : null) != null)
           || ((converter = valueContract.Converter) != null)
           || ((converter = Serializer.GetMatchingConverter(valueContract.UnderlyingType)) != null)
           || ((converter = valueContract.InternalConverter) != null))
          && converter.CanWrite)
      {
        SerializeConvertable(writer, converter, value, valueContract, containerContract, containerProperty);
        return;
      }

      switch (valueContract.ContractType)
      {
        case JsonContractType.Object:
          SerializeObject(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty);
          break;
        case JsonContractType.Array:
          JsonArrayContract arrayContract = (JsonArrayContract) valueContract;
          SerializeList(writer, arrayContract.CreateWrapper(value), arrayContract, member, containerContract, containerProperty);
          break;
        case JsonContractType.Primitive:
          SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty);
          break;
        case JsonContractType.String:
          SerializeString(writer, value, (JsonStringContract)valueContract);
          break;
        case JsonContractType.Dictionary:
          JsonDictionaryContract dictionaryContract = (JsonDictionaryContract) valueContract;
          SerializeDictionary(writer, dictionaryContract.CreateWrapper(value), dictionaryContract, member, containerContract, containerProperty);
          break;
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
        case JsonContractType.Dynamic:
          SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty);
          break;
#endif
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
        case JsonContractType.Serializable:
          SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty);
          break;
#endif
        case JsonContractType.Linq:
          ((JToken) value).WriteTo(writer, (Serializer.Converters != null) ? Serializer.Converters.ToArray() : null);
          break;
      }
    }

    private bool? ResolveIsReference(JsonContract contract, JsonProperty property, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      bool? isReference = null;

      // value could be coming from a dictionary or array and not have a property
      if (property != null)
        isReference = property.IsReference;

      if (isReference == null && containerProperty != null)
        isReference = containerProperty.ItemIsReference;

      if (isReference == null && collectionContract != null)
        isReference = collectionContract.ItemIsReference;

      if (isReference == null)
        isReference = contract.IsReference;

      return isReference;
    }

    private bool ShouldWriteReference(object value, JsonProperty property, JsonContract valueContract, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      if (value == null)
        return false;
      if (valueContract.ContractType == JsonContractType.Primitive || valueContract.ContractType == JsonContractType.String)
        return false;

      bool? isReference = ResolveIsReference(valueContract, property, collectionContract, containerProperty);

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

    private bool CheckForCircularReference(object value, JsonProperty property, JsonContract contract, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      if (value == null || contract.ContractType == JsonContractType.Primitive || contract.ContractType == JsonContractType.String)
        return true;

      ReferenceLoopHandling? referenceLoopHandling = null;

      if (property != null)
        referenceLoopHandling = property.ReferenceLoopHandling;

      if (referenceLoopHandling == null && containerProperty != null)
        referenceLoopHandling = containerProperty.ItemReferenceLoopHandling;

      if (referenceLoopHandling == null && containerContract != null)
        referenceLoopHandling = containerContract.ItemReferenceLoopHandling;

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

    private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);

      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

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
            JsonContract memberContract = (property.PropertyContract.UnderlyingType.IsSealed()) ? property.PropertyContract : GetContractSafe(memberValue);

            if (ShouldWriteProperty(memberValue, property))
            {
              string propertyName = property.PropertyName;

              if (ShouldWriteReference(memberValue, property, memberContract, contract, member))
              {
                writer.WritePropertyName(propertyName);
                WriteReference(writer, memberValue);
                continue;
              }

              if (!CheckForCircularReference(memberValue, property, memberContract, contract, member))
                continue;

              if (memberValue == null)
              {
                Required resolvedRequired = property._required ?? contract.ItemRequired ?? Required.Default;
                if (resolvedRequired == Required.Always)
                  throw new JsonSerializationException("Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
              }

              writer.WritePropertyName(propertyName);
              SerializeValue(writer, memberValue, memberContract, property, contract, member);
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

    private void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      writer.WriteStartObject();

      bool isReference = ResolveIsReference(contract, member, collectionContract, containerProperty) ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(this, value));
      }
      if (ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
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

    private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      if (ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
      {
        WriteReference(writer, value);
      }
      else
      {
        if (!CheckForCircularReference(value, null, contract, collectionContract, containerProperty))
          return;

        _serializeStack.Add(value);

        converter.WriteJson(writer, value, GetInternalSerializer());

        _serializeStack.RemoveAt(_serializeStack.Count - 1);
      }
    }

    private void SerializeList(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      contract.InvokeOnSerializing(values.UnderlyingCollection, Serializer.Context);

      _serializeStack.Add(values.UnderlyingCollection);

      bool hasWrittenMetadataObject = WriteStartArray(writer, values, contract, member, collectionContract, containerProperty);

      writer.WriteStartArray();

      int initialDepth = writer.Top;

      int index = 0;
      // note that an error in the IEnumerable won't be caught
      foreach (object value in values)
      {
        try
        {
          JsonContract valueContract = contract.FinalItemContract ?? GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract, contract, member))
          {
            WriteReference(writer, value);
          }
          else
          {
            if (CheckForCircularReference(value, null, valueContract, contract, member))
            {
              SerializeValue(writer, value, valueContract, null, contract, member);
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

    private bool WriteStartArray(JsonWriter writer, IWrappedCollection values, JsonArrayContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      bool isReference = ResolveIsReference(contract, member, containerContract, containerProperty) ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
      bool includeTypeDetails = ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
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

      if (contract.ItemContract == null)
        contract.ItemContract = Serializer.ContractResolver.ResolveContract(contract.CollectionItemType ?? typeof (object));

      return writeMetadataObject;
    }

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
#if !NET20
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [SecuritySafeCritical]
#endif
    private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      if (!JsonTypeReflector.FullyTrusted)
      {
        throw new JsonSerializationException(@"Type '{0}' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data.
To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add to JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
      }

      contract.InvokeOnSerializing(value, Serializer.Context);
      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

      SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
      value.GetObjectData(serializationInfo, Serializer.Context);

      foreach (SerializationEntry serializationEntry in serializationInfo)
      {
        writer.WritePropertyName(serializationEntry.Name);
        SerializeValue(writer, serializationEntry.Value, GetContractSafe(serializationEntry.Value), null, null, member);
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
    private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);
      _serializeStack.Add(value);

      WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);

      foreach (string memberName in value.GetDynamicMemberNames())
      {
        object memberValue;
        if (DynamicUtils.TryGetMember(value, memberName, out memberValue))
        {
          string resolvedPropertyName = (contract.PropertyNameResolver != null)
                                          ? contract.PropertyNameResolver(memberName)
                                          : memberName;

          writer.WritePropertyName(resolvedPropertyName);
          SerializeValue(writer, memberValue, GetContractSafe(memberValue), null, null, member);
        }
      }

      writer.WriteEndObject();

      _serializeStack.RemoveAt(_serializeStack.Count - 1);
      contract.InvokeOnSerialized(value, Serializer.Context);
    }
#endif

    private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerProperty)
    {
      TypeNameHandling resolvedTypeNameHandling =
        ((member != null) ? member.TypeNameHandling : null)
        ?? ((containerProperty != null) ? containerProperty.ItemTypeNameHandling : null)
        ?? ((containerContract != null) ? containerContract.ItemTypeNameHandling : null)
        ?? Serializer.TypeNameHandling;

      if (HasFlag(resolvedTypeNameHandling, typeNameHandlingFlag))
        return true;

      // instance type and the property's type's contract default type are different (no need to put the type in JSON because the type will be created by default)
      if (HasFlag(resolvedTypeNameHandling, TypeNameHandling.Auto))
      {
        if (member != null)
        {
          if (contract.UnderlyingType != member.PropertyContract.CreatedType)
            return true;
        }
        else if (containerContract != null && containerContract.ItemContract != null)
        {
          if (contract.UnderlyingType != containerContract.ItemContract.CreatedType)
            return true;
        }
      }

      return false;
    }

    private void SerializeDictionary(JsonWriter writer, IWrappedDictionary values, JsonDictionaryContract contract, JsonProperty member, JsonContainerContract collectionContract, JsonProperty containerProperty)
    {
      contract.InvokeOnSerializing(values.UnderlyingDictionary, Serializer.Context);

      _serializeStack.Add(values.UnderlyingDictionary);

      WriteObjectStart(writer, values.UnderlyingDictionary, contract, member, collectionContract, containerProperty);

      if (contract.ItemContract == null)
        contract.ItemContract = Serializer.ContractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));

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
          JsonContract valueContract = contract.FinalItemContract ?? GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract, contract, member))
          {
            writer.WritePropertyName(propertyName);
            WriteReference(writer, value);
          }
          else
          {
            if (!CheckForCircularReference(value, null, valueContract, contract, member))
              continue;

            writer.WritePropertyName(propertyName);

            SerializeValue(writer, value, valueContract, null, contract, member);
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