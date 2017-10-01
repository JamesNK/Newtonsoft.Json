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
using System.ComponentModel;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(NET35 || NET20 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
    internal class TypeConverterJsonConverter : JsonConverter
    {
        private TypeConverter GetConverter(Type type)
        {
            var converters = ReflectionUtils.GetAttributes(type, typeof(TypeConverterAttribute), true).Union(
                from t in type.GetInterfaces()
                from c in ReflectionUtils.GetAttributes(t, typeof(TypeConverterAttribute), true)
                select c).Distinct();

            return
                (from c in converters
                 let converter =
                     (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)c).ConverterTypeName))
                 where converter.CanConvertFrom(typeof(string))
                       && converter.CanConvertTo(typeof(string))
                 select converter)
                    .FirstOrDefault();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var converter = GetConverter(value.GetType());
            var text = converter.ConvertToInvariantString(value);

            writer.WriteValue(text);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var converter = GetConverter(objectType);
            return converter.ConvertFromInvariantString(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return GetConverter(objectType) != null;
        }
    }
#endif
}
