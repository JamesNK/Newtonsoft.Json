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
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;
using System.Reflection;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class ListOfIds<T> : JsonConverter where T : Bar, new()
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IList<T> list = (IList<T>)value;

            writer.WriteStartArray();
            foreach (T item in list)
            {
                writer.WriteValue(item.Id);
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IList<T> list = new List<T>();

            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
            {
                long id = (long)reader.Value;

                list.Add(new T
                {
                    Id = Convert.ToInt32(id)
                });

                reader.Read();
            }

            return list;
        }

        public override bool CanConvert(Type objectType)
        {
#if DNXCORE50
            return Newtonsoft.Json.Utilities.TypeExtensions.IsAssignableFrom(typeof(IList<T>), objectType);
#else
            return typeof(IList<T>).IsAssignableFrom(objectType);
#endif
        }
    }
}