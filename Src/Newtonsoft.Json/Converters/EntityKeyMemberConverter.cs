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

#if !(SILVERLIGHT || NET20 || NETFX_CORE || PORTABLE40 || PORTABLE)
using System;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  internal interface IEntityKeyMember
  {
    string Key { get; set; }
    object Value { get; set; }
  }

  /// <summary>
  /// Converts an Entity Framework EntityKey to and from JSON.
  /// </summary>
  public class EntityKeyMemberConverter : JsonConverter
  {
    private const string EntityKeyMemberFullTypeName = "System.Data.EntityKeyMember";

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(value);
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
        if (JsonSerializerInternalWriter.TryConvertToString(entityKeyMember.Value, keyType, out valueJson))
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

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      IEntityKeyMember entityKeyMember = DynamicWrapper.CreateWrapper<IEntityKeyMember>(Activator.CreateInstance(objectType));

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

      return DynamicWrapper.GetUnderlyingObject(entityKeyMember);
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
      return (objectType.AssignableToTypeName(EntityKeyMemberFullTypeName));
    }
  }
}
#endif