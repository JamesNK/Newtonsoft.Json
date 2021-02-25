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

using System.Linq;
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
    public class Issue1962
    {
        [Test]
        public void Test_Default()
        {
            string json = @"// comment
[ 1, 2, 42 ]";
            JToken token = JToken.Parse(json);

            Assert.AreEqual(JTokenType.Comment, token.Type);
            Assert.AreEqual(" comment", ((JValue)token).Value);
        }

        [Test]
        public void Test_LoadComments()
        {
            string json = @"// comment
[ 1, 2, 42 ]";
            JToken token = JToken.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(JTokenType.Comment, token.Type);
            Assert.AreEqual(" comment", ((JValue)token).Value);

            int[] obj = token.ToObject<int[]>();
            Assert.IsNull(obj);
        }

        [Test]
        public void Test_IgnoreComments()
        {
            string json = @"// comment
[ 1, 2, 42 ]";
            JToken token = JToken.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            Assert.AreEqual(JTokenType.Array, token.Type);
            Assert.AreEqual(3, token.Count());
            Assert.AreEqual(1, (int)token[0]);
            Assert.AreEqual(2, (int)token[1]);
            Assert.AreEqual(42, (int)token[2]);
        }
    }
}