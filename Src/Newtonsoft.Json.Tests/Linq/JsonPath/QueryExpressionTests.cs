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

namespace Newtonsoft.Json.Tests.Linq.JsonPath
{
    [TestFixture]
    public class QueryExpressionTests : TestFixtureBase
    {
        [Test]
        public void AndExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression
            {
                Operator = QueryOperator.And,
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression
                    {
                        Operator = QueryOperator.Exists,
                        Path = new List<PathFilter>
                        {
                            new FieldFilter
                            {
                                Name = "FirstName"
                            }
                        }
                    },
                    new BooleanQueryExpression
                    {
                        Operator = QueryOperator.Exists,
                        Path = new List<PathFilter>
                        {
                            new FieldFilter
                            {
                                Name = "LastName"
                            }
                        }
                    }
                }
            };

            JObject o1 = new JObject
            {
                {"Title","Title!"},
                {"FirstName", "FirstName!"},
                {"LastName", "LastName!"}
            };

            Assert.IsTrue(compositeExpression.IsMatch(o1));

            JObject o2 = new JObject
            {
                {"Title","Title!"},
                {"FirstName", "FirstName!"}
            };

            Assert.IsFalse(compositeExpression.IsMatch(o2));

            JObject o3 = new JObject
            {
                {"Title","Title!"}
            };

            Assert.IsFalse(compositeExpression.IsMatch(o3));
        }
        
        [Test]
        public void OrExpressionTest()
        {
            CompositeExpression compositeExpression = new CompositeExpression
            {
                Operator = QueryOperator.Or,
                Expressions = new List<QueryExpression>
                {
                    new BooleanQueryExpression
                    {
                        Operator = QueryOperator.Exists,
                        Path = new List<PathFilter>
                        {
                            new FieldFilter
                            {
                                Name = "FirstName"
                            }
                        }
                    },
                    new BooleanQueryExpression
                    {
                        Operator = QueryOperator.Exists,
                        Path = new List<PathFilter>
                        {
                            new FieldFilter
                            {
                                Name = "LastName"
                            }
                        }
                    }
                }
            };

            JObject o1 = new JObject
            {
                {"Title","Title!"},
                {"FirstName", "FirstName!"},
                {"LastName", "LastName!"}
            };

            Assert.IsTrue(compositeExpression.IsMatch(o1));

            JObject o2 = new JObject
            {
                {"Title","Title!"},
                {"FirstName", "FirstName!"}
            };

            Assert.IsTrue(compositeExpression.IsMatch(o2));

            JObject o3 = new JObject
            {
                {"Title","Title!"}
            };

            Assert.IsFalse(compositeExpression.IsMatch(o3));
        }

        [Test]
        public void BooleanExpressionTest()
        {
            BooleanQueryExpression e1 = new BooleanQueryExpression
            {
                Operator = QueryOperator.LessThan,
                Value = new JValue(3),
                Path = new List<PathFilter>
                {
                    new ArrayIndexFilter()
                }
            };

            Assert.IsTrue(e1.IsMatch(new JArray(1, 2, 3, 4, 5)));
            Assert.IsTrue(e1.IsMatch(new JArray(2, 3, 4, 5)));
            Assert.IsFalse(e1.IsMatch(new JArray(3, 4, 5)));
            Assert.IsFalse(e1.IsMatch(new JArray(4, 5)));

            BooleanQueryExpression e2 = new BooleanQueryExpression
            {
                Operator = QueryOperator.LessThanOrEquals,
                Value = new JValue(3),
                Path = new List<PathFilter>
                {
                    new ArrayIndexFilter()
                }
            };

            Assert.IsTrue(e2.IsMatch(new JArray(1, 2, 3, 4, 5)));
            Assert.IsTrue(e2.IsMatch(new JArray(2, 3, 4, 5)));
            Assert.IsTrue(e2.IsMatch(new JArray(3, 4, 5)));
            Assert.IsFalse(e2.IsMatch(new JArray(4, 5)));
        }
    }
}
