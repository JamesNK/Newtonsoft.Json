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

#if !(NET35 || NET20 || NETFX_CORE)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class DiscriminatedUnionConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeBasicUnion()
        {
            string json = JsonConvert.SerializeObject(Currency.AUD);

            Assert.AreEqual(@"{""Case"":""AUD"",""Fields"":[]}", json);
        }

        [Test]
        public void SerializeUnionWithFields()
        {
            string json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0));

            Assert.AreEqual(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}", json);
        }

        [Test]
        public void DeserializeBasicUnion()
        {
            Currency c = JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":[]}");
            Assert.AreEqual(Currency.AUD, c);

            c = JsonConvert.DeserializeObject<Currency>(@"{""Fields"":[],""Case"":""EUR""}");
            Assert.AreEqual(Currency.EUR, c);

            c = JsonConvert.DeserializeObject<Currency>(@"null");
            Assert.AreEqual(null, c);
        }

        [Test]
        public void DeserializeUnionWithFields()
        {
            Shape c = JsonConvert.DeserializeObject<Shape>(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}");
            Assert.AreEqual(true, c.IsRectangle);

            Shape.Rectangle r = (Shape.Rectangle)c;

            Assert.AreEqual(5.0, r.length);
            Assert.AreEqual(10.0, r.width);
        }

        [Test]
        public void DeserializeBasicUnion_NoMatch()
        {
            ExceptionAssert.Throws<JsonSerializationException>("No union type found with the name 'abcdefg'. Path 'Case', line 1, position 17.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""abcdefg"",""Fields"":[]}"));
        }

        [Test]
        public void DeserializeBasicUnion_MismatchedFieldCount()
        {
            ExceptionAssert.Throws<JsonSerializationException>("The number of field values does not match the number of properties definied by union 'AUD'. Path '', line 1, position 27.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":[1]}"));
        }

        [Test]
        public void DeserializeBasicUnion_NoCaseName()
        {
            ExceptionAssert.Throws<JsonSerializationException>("No 'Case' property with union name found. Path '', line 1, position 14.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Fields"":[1]}"));
        }

        [Test]
        public void DeserializeBasicUnion_NoFields()
        {
            ExceptionAssert.Throws<JsonSerializationException>("No 'Fields' property with union fields found. Path '', line 1, position 14.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD""}"));
        }

        [Test]
        public void DeserializeBasicUnion_UnexpectedEnd()
        {
            ExceptionAssert.Throws<JsonSerializationException>("Unexpected end when reading union. Path 'Case', line 1, position 8.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":"));
        }

        [Test]
        public void DeserializeBasicUnion_FieldsObject()
        {
            ExceptionAssert.Throws<JsonSerializationException>("Union fields must been an array. Path 'Fields', line 1, position 24.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":{}}"));
        }

        [Test]
        public void DeserializeBasicUnion_UnexpectedProperty()
        {
            ExceptionAssert.Throws<JsonSerializationException>("Unexpected property 'Case123' found when reading union. Path 'Case123', line 1, position 11.",
                () => JsonConvert.DeserializeObject<Currency>(@"{""Case123"":""AUD""}"));
        }
    }
}
#endif