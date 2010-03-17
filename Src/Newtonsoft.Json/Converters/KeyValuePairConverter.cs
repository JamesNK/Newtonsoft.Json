using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Reflection;

namespace Newtonsoft.Json.Converters
{
  public class KeyValuePairConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Type t = value.GetType();
      PropertyInfo keyProperty = t.GetProperty("Key");
      PropertyInfo valueProperty = t.GetProperty("Value");

      writer.WriteStartObject();
      writer.WritePropertyName("Key");
      serializer.Serialize(writer, ReflectionUtils.GetMemberValue(keyProperty, value));
      writer.WritePropertyName("Value");
      serializer.Serialize(writer, ReflectionUtils.GetMemberValue(valueProperty, value));
      writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      IList<Type> genericArguments = objectType.GetGenericArguments();
      Type keyType = genericArguments[0];
      Type valueType = genericArguments[1];

      reader.Read();
      reader.Read();
      object key = serializer.Deserialize(reader, keyType);
      reader.Read();
      reader.Read();
      object value = serializer.Deserialize(reader, valueType);
      reader.Read();

      return ReflectionUtils.CreateInstance(objectType, key, value);
    }

    public override bool CanConvert(Type objectType)
    {
      if (objectType.IsValueType && objectType.IsGenericType)
        return (objectType.GetGenericTypeDefinition() == typeof (KeyValuePair<,>));

      return false;
    }
  }
}