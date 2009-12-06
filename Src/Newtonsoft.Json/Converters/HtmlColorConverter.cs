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

#if !SILVERLIGHT && !PocketPC
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="Color"/> object to and from JSON.
  /// </summary>
  public class HtmlColorConverter : JsonConverter
  {
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <param name="value">The value.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      writer.WriteValue(ColorTranslator.ToHtml((Color)value));
    }

    /// <summary>
    /// Determines whether this instance can convert the specified value type.
    /// </summary>
    /// <param name="valueType">Type of the value.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type valueType)
    {
      return (valueType == typeof(Color));
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}
#endif