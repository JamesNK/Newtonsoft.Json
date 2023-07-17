using System;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class DecimalContainerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            reader.Read();
            var value = reader.ReadAsString();
            var result = new DecimalContainer
            {
                Value = decimal.Parse(value ?? "0"),
            };

            reader.Read();
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var x = (DecimalContainer)value!;
            writer.WriteStartObject();
            writer.WritePropertyName("D");
            writer.WriteRawValue(x.Value.ToString());
            writer.WriteEndObject();
        }
    }
}
