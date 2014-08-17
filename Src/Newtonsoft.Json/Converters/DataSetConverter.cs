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

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
using System;
using System.Data;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="DataSet"/> to and from JSON.
    /// </summary>
    public class DataSetConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataSet dataSet = (DataSet)value;
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            DataTableConverter converter = new DataTableConverter();

            writer.WriteStartObject();

            foreach (DataTable table in dataSet.Tables)
            {
                writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(table.TableName) : table.TableName);

                converter.WriteJson(writer, table, serializer);
            }

            writer.WriteEndObject();
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
            // handle typed datasets
            DataSet ds = (objectType == typeof(DataSet))
                ? new DataSet()
                : (DataSet)Activator.CreateInstance(objectType);

            DataTableConverter converter = new DataTableConverter();

            CheckedRead(reader);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                DataTable dt = ds.Tables[(string)reader.Value];
                bool exists = (dt != null);

                dt = (DataTable)converter.ReadJson(reader, typeof(DataTable), dt, serializer);

                if (!exists)
                    ds.Tables.Add(dt);

                CheckedRead(reader);
            }

            return ds;
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
            return typeof(DataSet).IsAssignableFrom(valueType);
        }

        private void CheckedRead(JsonReader reader)
        {
            if (!reader.Read())
                throw JsonSerializationException.Create(reader, "Unexpected end when reading DataSet.");
        }
    }
}

#endif