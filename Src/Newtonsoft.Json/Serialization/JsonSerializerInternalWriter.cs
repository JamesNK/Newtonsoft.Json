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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Runtime.Serialization;

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

    public JsonSerializerInternalWriter(JsonSerializer serializer) : base(serializer)
    {
    }

    public void Serialize(JsonWriter jsonWriter, object value)
    {
      if (jsonWriter == null)
        throw new ArgumentNullException("jsonWriter");

      SerializeValue(jsonWriter, value, null, GetContractSafe(value));
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

    private void SerializeValue(JsonWriter writer, object value, JsonConverter memberConverter, JsonContract contract)
    {
      JsonConverter converter = memberConverter;

      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      if (converter != null
          || ((converter = contract.Converter) != null)
          || Serializer.HasMatchingConverter(contract.UnderlyingType, out converter)
          || ((converter = contract.InternalConverter) != null))
      {
        SerializeConvertable(writer, converter, value, contract);
      }
      else if (contract is JsonPrimitiveContract)
      {
        writer.WriteValue(value);
      }
      else if (value is JToken)
      {
        ((JToken) value).WriteTo(writer, (Serializer.Converters != null) ? Serializer.Converters.ToArray() : null);
      }
      else if (contract is JsonObjectContract)
      {
        SerializeObject(writer, value, (JsonObjectContract) contract);
      }
      else if (contract is JsonDictionaryContract)
      {
        SerializeDictionary(writer, (IDictionary) value, (JsonDictionaryContract) contract);
      }
      else if (contract is JsonArrayContract)
      {
        if (value is IList)
        {
          SerializeList(writer, (IList) value, (JsonArrayContract) contract);
        }
        else if (value is IEnumerable)
        {
          SerializeEnumerable(writer, (IEnumerable) value, (JsonArrayContract) contract);
        }
        else
        {
          throw new Exception(
            "Cannot serialize '{0}' into a JSON array. Type does not implement IEnumerable.".FormatWith(
              CultureInfo.InvariantCulture, value.GetType()));
        }
      }
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
        if (value is IList)
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
        else if (value is IDictionary)
          isReference = HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
        else if (value is IEnumerable)
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
      JsonConverter memberConverter = property.MemberConverter;
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

      if (!CheckForCircularReference(memberValue, property.ReferenceLoopHandling))
        return;

      writer.WritePropertyName(propertyName);
      SerializeValue(writer, memberValue, memberConverter, contract);
    }

    private bool CheckForCircularReference(object value, ReferenceLoopHandling? referenceLoopHandling)
    {
      if (SerializeStack.IndexOf(value) != -1)
      {
        switch (referenceLoopHandling.GetValueOrDefault(Serializer.ReferenceLoopHandling))
        {
          case ReferenceLoopHandling.Error:
            throw new JsonSerializationException("Self referencing loop");
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

    private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract)
    {
      contract.InvokeOnSerializing(value, Serializer.Context);

      string s;
      if (TryConvertToString(value, contract.UnderlyingType, out s))
      {
        writer.WriteValue(s);
        contract.InvokeOnSerialized(value, Serializer.Context);

        return;
      }

      SerializeStack.Add(value);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(value));
      }
      if (HasFlag(Serializer.TypeNameHandling, TypeNameHandling.Objects))
      {
        WriteTypeProperty(writer, contract.UnderlyingType);
      }

      int initialDepth = writer.Top;

      foreach (JsonProperty property in contract.Properties)
      {
        try
        {
          if (!property.Ignored && property.Readable)
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
      writer.WriteValue(type.AssemblyQualifiedName);
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
        if (!CheckForCircularReference(value, null))
          return;

        SerializeStack.Add(value);

        converter.WriteJson(writer, value, GetInternalSerializer());

        SerializeStack.RemoveAt(SerializeStack.Count - 1);
      }
    }

    private void SerializeEnumerable(JsonWriter writer, IEnumerable values, JsonArrayContract contract)
    {
      SerializeList(writer, values.Cast<object>().ToList(), contract);
    }

    private void SerializeList(JsonWriter writer, IList values, JsonArrayContract contract)
    {
      contract.InvokeOnSerializing(values, Serializer.Context);

      SerializeStack.Add(values);

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
      bool includeTypeDetails = HasFlag(Serializer.TypeNameHandling, TypeNameHandling.Arrays);

      if (isReference || includeTypeDetails)
      {
        writer.WriteStartObject();

        if (isReference)
        {
          writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
          writer.WriteValue(Serializer.ReferenceResolver.GetReference(values));
        }
        if (includeTypeDetails)
        {
          WriteTypeProperty(writer, values.GetType());
        }
        writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName);
      }

      writer.WriteStartArray();

      int initialDepth = writer.Top;

      for (int i = 0; i < values.Count; i++)
      {
        try
        {
          object value = values[i];
          JsonContract valueContract = GetContractSafe(value);

          if (ShouldWriteReference(value, null, valueContract))
          {
            WriteReference(writer, value);
          }
          else
          {
            if (!CheckForCircularReference(value, null))
              continue;

            SerializeValue(writer, value, null, valueContract);
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values, contract, i, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndArray();

      if (isReference || includeTypeDetails)
      {
        writer.WriteEndObject();
      }

      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values, Serializer.Context);
    }

    private void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract)
    {
      contract.InvokeOnSerializing(values, Serializer.Context);

      SerializeStack.Add(values);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(Serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(Serializer.ReferenceResolver.GetReference(values));
      }
      if (HasFlag(Serializer.TypeNameHandling, TypeNameHandling.Objects))
      {
        WriteTypeProperty(writer, values.GetType());
      }

      int initialDepth = writer.Top;

      foreach (DictionaryEntry entry in values)
      {
        string propertyName = GetPropertyName(entry);

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
            if (!CheckForCircularReference(value, null))
              continue;

            writer.WritePropertyName(propertyName);

            SerializeValue(writer, value, null, valueContract);
          }
        }
        catch (Exception ex)
        {
          if (IsErrorHandled(values, contract, propertyName, ex))
            HandleError(writer, initialDepth);
          else
            throw;
        }
      }

      writer.WriteEndObject();
      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values, Serializer.Context);
    }

    private string GetPropertyName(DictionaryEntry entry)
    {
      string propertyName;

      if (entry.Key is IConvertible)
        return Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
      else if (TryConvertToString(entry.Key, entry.Key.GetType(), out propertyName))
        return propertyName;

      throw new JsonSerializationException("Could not create string property name from type '{0}'.".FormatWith(CultureInfo.InvariantCulture, entry.Key.GetType()));
    }

    private void HandleError(JsonWriter writer, int initialDepth)
    {
      ClearErrorContext();

      while (writer.Top > initialDepth)
      {
        writer.WriteEnd();
      }
    }
  }
}