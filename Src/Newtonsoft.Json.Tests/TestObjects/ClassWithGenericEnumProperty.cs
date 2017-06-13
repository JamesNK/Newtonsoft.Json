using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public sealed class ClassWithGenericEnumProperty<TEnum>
    {
        [JsonConverter(typeof(GenericEnumConverter<>))]
        public TEnum EnumValue { get; set; }
    }

    public sealed class ClassWithGenericEnumCollectionProperty<TEnum>
    {
        [JsonProperty(ItemConverterType = typeof(GenericEnumConverter<>))]
        public TEnum[] EnumValues { get; set; }
    }

    [JsonArray(ItemConverterType = typeof(GenericEnumConverter<>))]
    public sealed class ListWithEnumItemGenericConverter<TEnum> : List<TEnum>
    {
    }

    [JsonDictionary(ItemConverterType = typeof(GenericEnumConverter<>))]
    public sealed class DictionaryWithEnumItemGenericConverter<TKey, TEnum> : Dictionary<TKey, TEnum>
    {
    }

    public sealed class GenericEnumConverter<TEnum> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(TEnum);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return Enum.ToObject(typeof(TEnum), reader.Value);
            }
            return Enum.Parse(typeof(TEnum), reader.Value.ToString().Substring(typeof(TEnum).Name.Length + 1));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string str = value.ToString();
            if (char.IsDigit(str[0]) || str[0] == '-' || str[0] == '+')
            {
                writer.WriteRawValue(str);
            }
            else
            {
                writer.WriteValue($"{typeof(TEnum).Name}.{str}");
            }
        }
    }
}
