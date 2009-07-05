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
  internal class JsonSerializerWriter
  {
    internal readonly JsonSerializer _serializer;
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

    public JsonSerializerWriter(JsonSerializer serializer)
    {
      ValidationUtils.ArgumentNotNull(serializer, "serializer");

      _serializer = serializer;
    }

    public void Serialize(JsonWriter jsonWriter, object value)
    {
      if (jsonWriter == null)
        throw new ArgumentNullException("jsonWriter");

      SerializeValue(jsonWriter, value, null);
    }

    private JsonSerializerProxy GetInternalSerializer()
    {
      if (_internalSerializer == null)
        _internalSerializer = new JsonSerializerProxy(this);

      return _internalSerializer;
    }

    private void SerializeValue(JsonWriter writer, object value, JsonConverter memberConverter)
    {
      JsonConverter converter = memberConverter;

      if (value == null)
      {
        writer.WriteNull();
      }
      else if (converter != null
        || _serializer.HasClassConverter(value.GetType(), out converter)
        || _serializer.HasMatchingConverter(value.GetType(), out converter))
      {
        SerializeConvertable(writer, converter, value);
      }
      else if (JsonConvert.IsJsonPrimitive(value))
      {
        writer.WriteValue(value);
      }
      else if (value is JToken)
      {
        ((JToken)value).WriteTo(writer, (_serializer.Converters != null) ? _serializer.Converters.ToArray() : null);
      }
      else if (value is JsonRaw)
      {
        writer.WriteRawValue(((JsonRaw)value).Content);
      }
      else
      {
        JsonContract contract = _serializer.ContractResolver.ResolveContract(value.GetType());

        if (contract is JsonObjectContract)
        {
          SerializeObject(writer, value, (JsonObjectContract)contract);
        }
        else if (contract is JsonDictionaryContract)
        {
          SerializeDictionary(writer, (IDictionary)value, (JsonDictionaryContract)contract);
        }
        else if (contract is JsonArrayContract)
        {
          if (value is IList)
          {
            SerializeList(writer, (IList)value, (JsonArrayContract)contract);
          }
          else if (value is IEnumerable)
          {
            SerializeEnumerable(writer, (IEnumerable)value, (JsonArrayContract)contract);
          }
          else
          {
            throw new Exception("Cannot serialize '{0}' into a JSON array. Type does not implement IEnumerable.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
          }
        }
      }
    }

    private bool ShouldWriteReference(object value, JsonProperty property)
    {
      if (value == null)
        return false;
      if (JsonConvert.IsJsonPrimitive(value))
        return false;

      // value could be coming from a dictionary or array and not have a property
      bool? isReference;
      if (property != null)
      {
        isReference = property.IsReference;
      }
      else
      {
        JsonContract memberTypeContract = _serializer.ContractResolver.ResolveContract(value.GetType());
        if (memberTypeContract != null)
          isReference = memberTypeContract.IsReference;
        else
          isReference = null;
      }

      if (isReference == null)
      {
        if (value is IList)
          isReference = HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
        else if (value is IDictionary)
          isReference = HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
        else if (value is IEnumerable)
          isReference = HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
        else
          isReference = HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      }

      if (!isReference.Value)
        return false;

      return _serializer.ReferenceResolver.IsReferenced(value);
    }

    private void WriteMemberInfoProperty(JsonWriter writer, object value, JsonProperty property)
    {
      MemberInfo member = property.Member;
      string propertyName = property.PropertyName;
      JsonConverter memberConverter = property.MemberConverter;
      object defaultValue = property.DefaultValue;

      if (!ReflectionUtils.IsIndexedProperty(member))
      {
        object memberValue = ReflectionUtils.GetMemberValue(member, value);

        if (property.NullValueHandling.GetValueOrDefault(_serializer.NullValueHandling) == NullValueHandling.Ignore && memberValue == null)
          return;

        if (property.DefaultValueHandling.GetValueOrDefault(_serializer.DefaultValueHandling) == DefaultValueHandling.Ignore && Equals(memberValue, defaultValue))
          return;

        if (ShouldWriteReference(memberValue, property))
        {
          writer.WritePropertyName(propertyName ?? member.Name);
          WriteReference(writer, memberValue);
          return;
        }

        if (!CheckForCircularReference(memberValue, property.ReferenceLoopHandling))
          return;

        writer.WritePropertyName(propertyName ?? member.Name);
        SerializeValue(writer, memberValue, memberConverter);
      }
    }

    private bool CheckForCircularReference(object value, ReferenceLoopHandling? referenceLoopHandling)
    {
      if (SerializeStack.IndexOf(value) != -1)
      {
        switch (referenceLoopHandling.GetValueOrDefault(_serializer.ReferenceLoopHandling))
        {
          case ReferenceLoopHandling.Error:
            throw new JsonSerializationException("Self referencing loop");
          case ReferenceLoopHandling.Ignore:
            return false;
          case ReferenceLoopHandling.Serialize:
            return true;
          default:
            throw new InvalidOperationException("Unexpected ReferenceLoopHandling value: '{0}'".FormatWith(CultureInfo.InvariantCulture, _serializer.ReferenceLoopHandling));
        }
      }

      return true;
    }

    private void WriteReference(JsonWriter writer, object value)
    {
      writer.WriteStartObject();
      writer.WritePropertyName(JsonTypeReflector.RefPropertyName);
      writer.WriteValue(_serializer.ReferenceResolver.GetReference(value));
      writer.WriteEndObject();
    }

    private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract)
    {
      contract.InvokeOnSerializing(value);

#if !SILVERLIGHT && !PocketPC
      TypeConverter converter = TypeDescriptor.GetConverter(contract.UnderlyingType);

      // use the objectType's TypeConverter if it has one and can convert to a string
      if (converter != null && !(converter is ComponentConverter) && (converter.GetType() != typeof (TypeConverter) || value is Type))
      {
        if (converter.CanConvertTo(typeof (string)))
        {
          writer.WriteValue(converter.ConvertToInvariantString(value));

          contract.InvokeOnSerialized(value);
          return;
        }
      }
#else
      if (value is Guid || value is Type || value is Uri)
      {
        writer.WriteValue(value.ToString());
        return;
      }
#endif

      SerializeStack.Add(value);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(_serializer.ReferenceResolver.GetReference(value));
      }
      if (HasFlag(_serializer.TypeNameHandling, TypeNameHandling.Objects))
      {
        WriteTypeProperty(writer, contract.UnderlyingType);
      }

      foreach (JsonProperty property in contract.Properties)
      {
        if (!property.Ignored && property.Readable)
          WriteMemberInfoProperty(writer, value, property);
      }

      writer.WriteEndObject();
      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(value);
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

    private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value)
    {
      if (ShouldWriteReference(value, null))
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
      contract.InvokeOnSerializing(values);

      SerializeStack.Add(values);

      bool isReference = contract.IsReference ?? HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Arrays);
      bool includeTypeDetails = HasFlag(_serializer.TypeNameHandling, TypeNameHandling.Arrays);

      if (isReference || includeTypeDetails)
      {
        writer.WriteStartObject();

        if (isReference)
        {
          writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
          writer.WriteValue(_serializer.ReferenceResolver.GetReference(values));
        }
        if (includeTypeDetails)
        {
          WriteTypeProperty(writer, values.GetType());
        }
        writer.WritePropertyName(JsonTypeReflector.ArrayValuesPropertyName);
      }

      writer.WriteStartArray();

      for (int i = 0; i < values.Count; i++)
      {
        object value = values[i];

        if (ShouldWriteReference(value, null))
        {
          WriteReference(writer, value);
        }
        else
        {
          if (!CheckForCircularReference(value, null))
            continue;

          SerializeValue(writer, value, null);
        }
      }

      writer.WriteEndArray();

      if (isReference)
      {
        writer.WriteEndObject();
      }

      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values);
    }

    private void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract)
    {
      contract.InvokeOnSerializing(values);

      SerializeStack.Add(values);
      writer.WriteStartObject();

      bool isReference = contract.IsReference ?? HasFlag(_serializer.PreserveReferencesHandling, PreserveReferencesHandling.Objects);
      if (isReference)
      {
        writer.WritePropertyName(JsonTypeReflector.IdPropertyName);
        writer.WriteValue(_serializer.ReferenceResolver.GetReference(values));
      }
      if (HasFlag(_serializer.TypeNameHandling, TypeNameHandling.Objects))
      {
        WriteTypeProperty(writer, values.GetType());
      }

      foreach (DictionaryEntry entry in values)
      {
        string propertyName = entry.Key.ToString();
        object value = entry.Value;

        if (ShouldWriteReference(value, null))
        {
          writer.WritePropertyName(propertyName);
          WriteReference(writer, value);
        }
        else
        {
          if (!CheckForCircularReference(value, null))
            continue;

          writer.WritePropertyName(propertyName);
          SerializeValue(writer, value, null);
        }
      }

      writer.WriteEndObject();
      SerializeStack.RemoveAt(SerializeStack.Count - 1);

      contract.InvokeOnSerialized(values);
    }
  }
}