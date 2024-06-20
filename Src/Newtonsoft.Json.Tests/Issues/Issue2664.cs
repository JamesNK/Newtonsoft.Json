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

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2664 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>  JsonConvert.DeserializeObject<SomData>(@"{""aInt"":1, ""ALong"":2.2}"), "Input string '2.2' is not a valid integer. Path 'ALong', line 1, position 22.");

            ExceptionAssert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<SomData>(@"{""aInt"":1.2, ""ALong"":2}"), "Input string '1.2' is not a valid integer. Path 'aInt', line 1, position 11.");
        }

        private class SomData
        {
            public int AInt { get; set; }
            public long ALong { get; set; }
        }
    }
}
#endif