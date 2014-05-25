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
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public abstract class ConverterPrecedenceClassConverter : JsonConverter
    {
        public abstract string ConverterType { get; }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ConverterPrecedenceClass c = (ConverterPrecedenceClass)value;

            JToken j = new JArray(ConverterType, c.TestValue);

            j.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken j = JArray.Load(reader);

            string converter = (string)j[0];
            if (converter != ConverterType)
                throw new Exception(StringUtils.FormatWith("Serialize converter {0} and deserialize converter {1} do not match.", CultureInfo.InvariantCulture, converter, ConverterType));

            string testValue = (string)j[1];
            return new ConverterPrecedenceClass(testValue);
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ConverterPrecedenceClass));
        }
    }
}