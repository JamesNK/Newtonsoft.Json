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
    public class Issue1552 : TestFixtureBase
    {
        [Test]
        public void Test_Error()
        {
            RefAndRefReadonlyTestClass c = new RefAndRefReadonlyTestClass(123);
            c.SetRefField(456);

            JsonSerializationException ex = ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.SerializeObject(c),
                "Error getting value from 'RefField' on 'Newtonsoft.Json.Tests.Issues.RefAndRefReadonlyTestClass'.");

            Assert.AreEqual("Could not create getter for Int32& RefField. ByRef return values are not supported.", ex.InnerException.Message);
        }

        [Test]
        public void Test_Ignore()
        {
            RefAndRefReadonlyIgnoredTestClass c = new RefAndRefReadonlyIgnoredTestClass(123);
            c.SetRefField(456);

            string json = JsonConvert.SerializeObject(c);

            Assert.AreEqual("{}", json);
        }
    }

    public class RefAndRefReadonlyTestClass
    {
        private int _refField;
        private readonly int _refReadonlyField;

        public RefAndRefReadonlyTestClass(int refReadonlyField)
        {
            _refReadonlyField = refReadonlyField;
        }

        public ref int RefField => ref _refField;

        public ref readonly int RefReadonlyField => ref _refReadonlyField;

        public void SetRefField(int value)
        {
            _refField = value;
        }
    }

    public class RefAndRefReadonlyIgnoredTestClass
    {
        private int _refField;
        private readonly int _refReadonlyField;

        public RefAndRefReadonlyIgnoredTestClass(int refReadonlyField)
        {
            _refReadonlyField = refReadonlyField;
        }

        [JsonIgnore]
        public ref int RefField => ref _refField;

        [JsonIgnore]
        public ref readonly int RefReadonlyField => ref _refReadonlyField;

        public void SetRefField(int value)
        {
            _refField = value;
        }
    }
}