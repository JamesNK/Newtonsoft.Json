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

#if !PocketPC && !SILVERLIGHT && !NET20
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  public class EntityKeyMemberConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      EntityKeyMember entityKeyMember = (EntityKeyMember) value;
      Type keyType = (entityKeyMember.Value != null) ? entityKeyMember.Value.GetType() : null;

      writer.WriteStartObject();
      writer.WritePropertyName("Key");
      writer.WriteValue(entityKeyMember.Key);
      writer.WritePropertyName("Type");
      writer.WriteValue((keyType != null) ? keyType.FullName : null);

      writer.WritePropertyName("Value");

      if (keyType != null)
      {
        string valueJson;
        if (JsonSerializerWriter.TryConvertToString(entityKeyMember.Value, keyType, out valueJson))
          writer.WriteValue(valueJson);
        else
          writer.WriteValue(entityKeyMember.Value);
      }
      else
      {
        writer.WriteNull();
      }

      writer.WriteEndObject();
    }

    private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
    {
      ReadAndAssert(reader);

      if (reader.TokenType != JsonToken.PropertyName || reader.Value.ToString() != propertyName)
        throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
    }

    private static void ReadAndAssert(JsonReader reader)
    {
      if (!reader.Read())
        throw new JsonSerializationException("Unexpected end.");
    }

    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      EntityKeyMember entityKeyMember = new EntityKeyMember();

      ReadAndAssertProperty(reader, "Key");
      ReadAndAssert(reader);
      entityKeyMember.Key = reader.Value.ToString();

      ReadAndAssertProperty(reader, "Type");
      ReadAndAssert(reader);
      string type = reader.Value.ToString();

      Type t = Type.GetType(type);

      ReadAndAssertProperty(reader, "Value");
      ReadAndAssert(reader);
      entityKeyMember.Value = serializer.Deserialize(reader, t);

      ReadAndAssert(reader);

      return entityKeyMember;
    }

    public override bool CanConvert(Type objectType)
    {
      return typeof(EntityKeyMember).IsAssignableFrom(objectType);
    }
  }
}
#endif