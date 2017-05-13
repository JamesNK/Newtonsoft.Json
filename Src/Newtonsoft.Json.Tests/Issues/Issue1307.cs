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
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1307 : TestFixtureBase
    {
        public class MyOtherClass
        {
            [JsonConverter(typeof(MyJsonConverter))]
            public MyClass2 InstanceOfMyClass { get; set; }
        }

        public class MyClass2
        {
            public int[] Dummy { get; set; }
        }

        internal class MyJsonConverter : JsonConverter
        {
            static private readonly JsonLoadSettings _jsonLoadSettings = new JsonLoadSettings { CommentHandling = CommentHandling.Ignore };

            public override bool CanConvert(Type objectType)
            {
                return typeof(MyClass2).Equals(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader, _jsonLoadSettings);

                if (token.Type == JTokenType.Object)
                {
                    return token.ToObject<MyClass2>();
                }
                else if (token.Type == JTokenType.Array)
                {
                    var result = new MyClass2();
                    result.Dummy = token.Select(t => (int)t).ToArray();
                    return result;
                }
                else if (token.Type == JTokenType.Comment)
                {
                    throw new InvalidProgramException();
                }
                return existingValue;
            }

            #region Do not use this converter for writing.

            public override bool CanWrite { get { return false; } }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            #endregion

        }

        [Test]
        public void Test()
        {
            string json = @"{
  ""instanceOfMyClass"":
    /* Comment explaining that this is a legacy data contract: */
    [ 1, 2, 3 ]
}";

            var c = JsonConvert.DeserializeObject<MyOtherClass>(json);
            Assert.AreEqual(3, c.InstanceOfMyClass.Dummy.Length);
        }
    }
}
