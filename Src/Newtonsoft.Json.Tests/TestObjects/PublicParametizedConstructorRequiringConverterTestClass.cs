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

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class NameContainer
    {
        public string Value { get; set; }
    }

    public class NameContainerConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NameContainer nameContainer = value as NameContainer;

            if (nameContainer != null)
            {
                writer.WriteValue(nameContainer.Value);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            NameContainer nameContainer = new NameContainer();
            nameContainer.Value = (string)reader.Value;

            return nameContainer;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NameContainer);
        }
    }

    public class PublicParametizedConstructorRequiringConverterTestClass
    {
        private readonly NameContainer _nameContainer;

        public PublicParametizedConstructorRequiringConverterTestClass(NameContainer nameParameter)
        {
            _nameContainer = nameParameter;
        }

        public NameContainer Name
        {
            get { return _nameContainer; }
        }
    }

    public class PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass
    {
        private readonly NameContainer _nameContainer;

        public PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass([JsonConverter(typeof(NameContainerConverter))] NameContainer nameParameter)
        {
            _nameContainer = nameParameter;
        }

        public NameContainer Name
        {
            get { return _nameContainer; }
        }
    }

    public class PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass
    {
        private readonly NameContainer _nameContainer;

        public PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass(NameContainer name)
        {
            _nameContainer = name;
        }

        [JsonConverter(typeof(NameContainerConverter))]
        public NameContainer Name
        {
            get { return _nameContainer; }
        }
    }
}