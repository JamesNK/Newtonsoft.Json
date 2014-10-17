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
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Tests.TestObjects;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class MissingMemberHandlingTests : TestFixtureBase
    {
        [Test]
        public void MissingMemberDeserialize()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small", "Medium", "Large" };

            string output = JsonConvert.SerializeObject(product, Formatting.Indented);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": new Date(1230422400000),
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            ExceptionAssert.Throws<JsonSerializationException>(() => { ProductShort deserializedProductShort = (ProductShort)JsonConvert.DeserializeObject(output, typeof(ProductShort), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }, @"Could not find member 'Price' on object of type 'ProductShort'. Path 'Price', line 4, position 11.");
        }

        [Test]
        public void MissingMemberDeserializeOkay()
        {
            Product product = new Product();

            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.Sizes = new string[] { "Small", "Medium", "Large" };

            string output = JsonConvert.SerializeObject(product);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": new Date(1230422400000),
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.MissingMemberHandling = MissingMemberHandling.Ignore;

            object deserializedValue;

            using (JsonReader jsonReader = new JsonTextReader(new StringReader(output)))
            {
                deserializedValue = jsonSerializer.Deserialize(jsonReader, typeof(ProductShort));
            }

            ProductShort deserializedProductShort = (ProductShort)deserializedValue;

            Assert.AreEqual("Apple", deserializedProductShort.Name);
            Assert.AreEqual(new DateTime(2008, 12, 28), deserializedProductShort.ExpiryDate);
            Assert.AreEqual("Small", deserializedProductShort.Sizes[0]);
            Assert.AreEqual("Medium", deserializedProductShort.Sizes[1]);
            Assert.AreEqual("Large", deserializedProductShort.Sizes[2]);
        }

        [Test]
        public void MissingMemberIgnoreComplexValue()
        {
            JsonSerializer serializer = new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore };
            serializer.Converters.Add(new JavaScriptDateTimeConverter());

            string response = @"{""PreProperty"":1,""DateProperty"":new Date(1225962698973),""PostProperty"":2}";

            MyClass myClass = (MyClass)serializer.Deserialize(new StringReader(response), typeof(MyClass));

            Assert.AreEqual(1, myClass.PreProperty);
            Assert.AreEqual(2, myClass.PostProperty);
        }

        [Test]
        public void MissingMemeber()
        {
            string json = @"{""Missing"":1}";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }, "Could not find member 'Missing' on object of type 'DoubleClass'. Path 'Missing', line 1, position 11.");
        }

        [Test]
        public void MissingJson()
        {
            string json = @"{}";

            JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }
    }
}