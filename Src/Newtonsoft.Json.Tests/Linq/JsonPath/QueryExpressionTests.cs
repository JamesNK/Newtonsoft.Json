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
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Linq.JsonPath
{
    [TestFixture]
    public class QueryExpressionTests : TestFixtureBase
    {
        [Test]
        public void AndExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.And)
            {
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
                }
            };

            JObject o1 = new JObject
            {
                { "Title", "Title!" },
                { "FirstName", "FirstName!" },
                { "LastName", "LastName!" }
            };

            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

            JObject o2 = new JObject
            {
                { "Title", "Title!" },
                { "FirstName", "FirstName!" }
            };

            Assert.IsFalse(compositeExpression.IsMatch(o2, o2));

            JObject o3 = new JObject
            {
                { "Title", "Title!" }
            };

            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
        }

        [Test]
        public void OrExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression(QueryOperator.Or)
            {
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("FirstName") }, null),
                    new BooleanQueryExpression(QueryOperator.Exists, new List<PathFilter> { new FieldFilter("LastName") }, null)
                }
            };

            JObject o1 = new JObject
            {
                { "Title", "Title!" },
                { "FirstName", "FirstName!" },
                { "LastName", "LastName!" }
            };

            Assert.IsTrue(compositeExpression.IsMatch(o1, o1));

            JObject o2 = new JObject
            {
                { "Title", "Title!" },
                { "FirstName", "FirstName!" }
            };

            Assert.IsTrue(compositeExpression.IsMatch(o2, o2));

            JObject o3 = new JObject
            {
                { "Title", "Title!" }
            };

            Assert.IsFalse(compositeExpression.IsMatch(o3, o3));
        }
        
        [Test]
        public void BooleanExpressionTest_RegexEqualsOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/foo.*d/"));

            Assert.IsTrue(e1.IsMatch(null, new JArray("food")));
            Assert.IsTrue(e1.IsMatch(null, new JArray("fooood and drink")));
            Assert.IsFalse(e1.IsMatch(null, new JArray("FOOD")));
            Assert.IsFalse(e1.IsMatch(null, new JArray("foo", "foog", "good")));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/Foo.*d/i"));

            Assert.IsTrue(e2.IsMatch(null, new JArray("food")));
            Assert.IsTrue(e2.IsMatch(null, new JArray("fooood and drink")));
            Assert.IsTrue(e2.IsMatch(null, new JArray("FOOD")));
            Assert.IsFalse(e2.IsMatch(null, new JArray("foo", "foog", "good")));
        }

        [Test]
        public void BooleanExpressionTest_RegexEqualsOperator_CornerCase()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/// comment/"));

            Assert.IsTrue(e1.IsMatch(null, new JArray("// comment")));
            Assert.IsFalse(e1.IsMatch(null, new JArray("//comment", "/ comment")));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.RegexEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue("/<tag>.*</tag>/i"));

            Assert.IsTrue(e2.IsMatch(null, new JArray("<Tag>Test</Tag>", "")));
            Assert.IsFalse(e2.IsMatch(null, new JArray("<tag>Test<tag>")));
        }

        [Test]
        public void BooleanExpressionTest()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.LessThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

            Assert.IsTrue(e1.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 3, 4, 5)));
            Assert.IsFalse(e1.IsMatch(null, new JArray(3, 4, 5)));
            Assert.IsFalse(e1.IsMatch(null, new JArray(4, 5)));
            Assert.IsFalse(e1.IsMatch(null, new JArray("11", 5)));

            BooleanQueryExpression e2 = new BooleanQueryExpression(QueryOperator.LessThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

            Assert.IsTrue(e2.IsMatch(null, new JArray(1, 2, 3, 4, 5)));
            Assert.IsTrue(e2.IsMatch(null, new JArray(2, 3, 4, 5)));
            Assert.IsTrue(e2.IsMatch(null, new JArray(3, 4, 5)));
            Assert.IsFalse(e2.IsMatch(null, new JArray(4, 5)));
            Assert.IsFalse(e1.IsMatch(null, new JArray("11", 5)));
        }

        [Test]
        public void BooleanExpressionTest_GreaterThanOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThan, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "26")));
            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 26)));
            Assert.IsFalse(e1.IsMatch(null, new JArray(2, 3)));
            Assert.IsFalse(e1.IsMatch(null, new JArray("2", "3")));
        }

        [Test]
        public void BooleanExpressionTest_GreaterThanOrEqualsOperator()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression(QueryOperator.GreaterThanOrEquals, new List<PathFilter> { new ArrayIndexFilter() }, new JValue(3));

            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "26")));
            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 26)));
            Assert.IsTrue(e1.IsMatch(null, new JArray(2, 3)));
            Assert.IsTrue(e1.IsMatch(null, new JArray("2", "3")));
            Assert.IsFalse(e1.IsMatch(null, new JArray(2, 1)));
            Assert.IsFalse(e1.IsMatch(null, new JArray("2", "1")));
        }
    }
}