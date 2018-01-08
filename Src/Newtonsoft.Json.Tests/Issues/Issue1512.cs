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

#if !(NET20 || NET35 || NET40)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1512 : TestFixtureBase
    {
        [Test]
        public void Test_Constructor()
        {
            var json = @"[
                            {
                                ""Inners"": [""hi"",""bye""]
                            }
                        ]";
            ImmutableArray<Outer> result = JsonConvert.DeserializeObject<ImmutableArray<Outer>>(json);

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0].Inners.Value.Length);
            Assert.AreEqual("hi", result[0].Inners.Value[0]);
            Assert.AreEqual("bye", result[0].Inners.Value[1]);
        }

        [Test]
        public void Test_Property()
        {
            var json = @"[
                            {
                                ""Inners"": [""hi"",""bye""]
                            }
                        ]";
            ImmutableArray<OuterProperty> result = JsonConvert.DeserializeObject<ImmutableArray<OuterProperty>>(json);

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(2, result[0].Inners.Value.Length);
            Assert.AreEqual("hi", result[0].Inners.Value[0]);
            Assert.AreEqual("bye", result[0].Inners.Value[1]);
        }
    }

    public sealed class Outer
    {
        public Outer(ImmutableArray<string>? inners)
        {
            this.Inners = inners;
        }

        public ImmutableArray<string>? Inners { get; }
    }

    public sealed class OuterProperty
    {
        public ImmutableArray<string>? Inners { get; set; }
    }
}
#endif