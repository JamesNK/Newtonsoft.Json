using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq.JsonPath;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
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
    }
}
