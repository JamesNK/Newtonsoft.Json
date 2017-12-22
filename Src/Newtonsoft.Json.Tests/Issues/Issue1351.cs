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

#if !NET20
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
#if !NET20
using System.Xml.Linq;
#endif
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
    public class Issue1351 : TestFixtureBase
    {
        public class Color
        {
            public Color()
            {
            }

            public Color(uint colorCode)
            {
                A = (byte)((colorCode & 0xff000000) >> 24);
                R = (byte)((colorCode & 0x00ff0000) >> 16);
                G = (byte)((colorCode & 0x0000ff00) >> 8);
                B = (byte)(colorCode & 0x000000ff);
            }

            public byte A { get; set; }
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
        }

        public static class Colors
        {
            public static Color White = new Color(0xFFFFFFFF);
        }

        [DataContract]
        public class TestClass
        {
            public TestClass()
            {
                Color = Colors.White;
            }

            [DataMember]
            public Color Color { get; set; }
        }

        [Test]
        public void Test()
        {
            var t = new List<TestClass>
            {
                new TestClass
                {
                    Color = new Color
                    {
                        A = 1,
                        G = 1,
                        B = 1,
                        R = 1
                    }
                },
                new TestClass
                {
                    Color = new Color
                    {
                        A = 2,
                        G = 2,
                        B = 2,
                        R = 2
                    }
                }
            };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(t, settings);

            var exception = ExceptionAssert.Throws<JsonSerializationException>(() =>
                {
                    JsonConvert.DeserializeObject<List<TestClass>>(json, settings);
                },
                "Error reading object reference '4'. Path '[1].Color.A', line 16, position 10.");

            Assert.AreEqual("A different Id has already been assigned for value 'Newtonsoft.Json.Tests.Issues.Issue1351+Color'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.", exception.InnerException.Message);
        }

        [Test]
        public void Test_Replace()
        {
            var t = new List<TestClass>
            {
                new TestClass
                {
                    Color = new Color
                    {
                        A = 1,
                        G = 1,
                        B = 1,
                        R = 1
                    }
                },
                new TestClass
                {
                    Color = new Color
                    {
                        A = 2,
                        G = 2,
                        B = 2,
                        R = 2
                    }
                }
            };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            var json = JsonConvert.SerializeObject(t, settings);

            var obj = JsonConvert.DeserializeObject<List<TestClass>>(json, settings);

            var o1 = obj[0];
            Assert.AreEqual(1, o1.Color.A);

            var o2 = obj[1];
            Assert.AreEqual(2, o2.Color.A);
        }
    }
}
#endif