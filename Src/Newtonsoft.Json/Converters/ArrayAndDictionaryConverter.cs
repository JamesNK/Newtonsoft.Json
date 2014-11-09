using System;
using Newtonsoft.Json.Bson;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Converters
{
    class ArrayAndDictionaryConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (objectType.IsArray)
            {
                return ExpectArray(reader);
            }
            else
            {
                return ExpectObject(reader);
            }
        }

        private static object ExpectDictionaryOrArrayOrPrimitive(JsonReader reader)
        {
            reader.Read();
            var startToken = reader.TokenType;
            switch (startToken)
            {
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.Float:
                case JsonToken.Null:
                    return reader.Value;
                case JsonToken.StartObject:
                    return ExpectObject(reader);
                case JsonToken.StartArray:
                    return ExpectArray(reader);
                default:
                    break;
            }
            throw new Exception("Unrecognized token: " + reader.TokenType.ToString());
        }

        internal static object[] ExpectArray(JArray reader)
        {
            var array = new List<Object>();
            foreach (var token in reader.AsJEnumerable())
            {
                array.Add(ExpectValue(token));
            }
            return array.ToArray();
        }

        internal static object ExpectObject(JsonReader reader)
        {
            var dic = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        dic.Add(reader.Value.ToString(), ExpectDictionaryOrArrayOrPrimitive(reader));
                        break;
                    case JsonToken.EndObject:
                        return dic;
                    default:
                        throw new Exception("Unrecognized token: " + reader.TokenType.ToString());
                }
            }
            throw new Exception("Missing end");
        }

        internal static object ExpectValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return token.ToObject<string>();
                case JTokenType.Integer:
                    return (token.ToObject<Int64>());
                case JTokenType.Boolean:
                    return (token.ToObject<Boolean>());
                case JTokenType.Bytes:
                    return (token.ToObject<Byte[]>());
                case JTokenType.Date:
                    return (token.ToObject<DateTime>());
                case JTokenType.Float:
                    return (token.ToObject<float>());
                case JTokenType.Null:
                    return (null);
                case JTokenType.Array:
                    return (ExpectArray((JArray)token));
                case JTokenType.Object:
                    return (ExpectObject((JObject)token));
                default:
                    throw new Exception("Array: Unrecognized token: " + token.Type.ToString());
            }
        }

        internal static object ExpectObject(JObject token)
        {
            var dic = new Dictionary<string, object>();
            foreach (var property in token.Properties())
            {
                dic.Add(property.Name, ExpectValue(property.Value));
            }
            return dic;
        }

        internal static object ExpectArray(JsonReader reader)
        {
            var array = new List<Object>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Null:
                        array.Add(reader.Value);
                        break;
                    case JsonToken.StartObject:
                        array.Add(ExpectObject(reader));
                        break;
                    case JsonToken.StartArray:
                        array.Add(ExpectArray(reader));
                        break;
                    case JsonToken.EndArray:
                        return array.ToArray();
                    default:
                        throw new Exception("Array: Unrecognized token: " + reader.TokenType.ToString());
                }
            }
            throw new Exception("Missing end");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, object>)
                || objectType.IsArray;
        }
    }
}
