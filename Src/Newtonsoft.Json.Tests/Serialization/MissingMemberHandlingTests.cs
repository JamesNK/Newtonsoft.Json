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
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
#if DNXCORE50
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

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                ProductShort deserializedProductShort = (ProductShort)JsonConvert.DeserializeObject(output, typeof(ProductShort), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            }, @"Could not find member 'Price' on object of type 'ProductShort'. Path 'Price', line 4, position 10.");
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
        public void CaseInsensitive()
        {
            string json = @"{""height"":1}";

            DoubleClass c = JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });

            Assert.AreEqual(1d, c.Height);
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

        [Test]
        public void MissingErrorAttribute()
        {
            string json = @"{""Missing"":1}";

            ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.DeserializeObject<NameWithMissingError>(json); }, "Could not find member 'Missing' on object of type 'NameWithMissingError'. Path 'Missing', line 1, position 11.");
        }

        [JsonObject(MissingMemberHandling = MissingMemberHandling.Error)]
        public class NameWithMissingError
        {
            public string First { get; set; }
        }

        public class Name
        {
            public string First { get; set; }
        }

        public class Person
        {
            public Name Name { get; set; }
        }

        [Test]
        public void MissingMemberHandling_RootObject()
        {
            IList<string> errors = new List<string>();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //This works on properties but not on a objects property.
                /* So nameERROR:{"first":"ni"} would throw. The payload name:{"firstERROR":"hi"} would not */
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    // A more concrete error type would be nice but we are limited by Newtonsofts library here.
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            Person p = new Person();

            JsonConvert.PopulateObject(@"{nameERROR:{""first"":""hi""}}", p, settings);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Could not find member 'nameERROR' on object of type 'Person'. Path 'nameERROR', line 1, position 11.", errors[0]);
        }

        [Test]
        public void MissingMemberHandling_InnerObject()
        {
            IList<string> errors = new List<string>();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //This works on properties but not on a objects property.
                /* So nameERROR:{"first":"ni"} would throw. The payload name:{"firstERROR":"hi"} would not */
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    // A more concrete error type would be nice but we are limited by Newtonsofts library here.
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            Person p = new Person();

            JsonConvert.PopulateObject(@"{name:{""firstERROR"":""hi""}}", p, settings);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Could not find member 'firstERROR' on object of type 'Name'. Path 'name.firstERROR', line 1, position 20.", errors[0]);
        }

        [JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
        public class SimpleExtendableObject
        {
            [JsonExtensionData]
            public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        }

        public class ObjectWithExtendableChild
        {
            public SimpleExtendableObject Data;
        }

        [Test]
        public void TestMissingMemberHandlingForDirectObjects()
        {
            string json = @"{""extensionData1"": [1,2,3]}";
            SimpleExtendableObject e2 = JsonConvert.DeserializeObject<SimpleExtendableObject>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            JArray o1 = (JArray)e2.Data["extensionData1"];
            Assert.AreEqual(JTokenType.Array, o1.Type);
        }

        [Test]
        public void TestMissingMemberHandlingForChildObjects()
        {
            string json = @"{""Data"":{""extensionData1"": [1,2,3]}}";
            ObjectWithExtendableChild e3 = JsonConvert.DeserializeObject<ObjectWithExtendableChild>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            JArray o1 = (JArray)e3.Data.Data["extensionData1"];
            Assert.AreEqual(JTokenType.Array, o1.Type);
        }

        [Test]
        public void TestMissingMemberHandlingForChildObjectsWithInvalidData()
        {
            string json = @"{""InvalidData"":{""extensionData1"": [1,2,3]}}";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<ObjectWithExtendableChild>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            }, "Could not find member 'InvalidData' on object of type 'ObjectWithExtendableChild'. Path 'InvalidData', line 1, position 15.");
        }
    }
}