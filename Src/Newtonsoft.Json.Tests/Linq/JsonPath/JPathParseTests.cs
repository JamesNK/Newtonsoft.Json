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
using Newtonsoft.Json.Linq.JsonPath;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.Linq.JsonPath
{
    [TestFixture]
    public class JPathParseTests : TestFixtureBase
    {
        [Test]
        public void SingleProperty()
        {
            JPath path = new JPath("Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SingleQuotedProperty()
        {
            JPath path = new JPath("['Blah']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JPath path = new JPath("[  'Blah'  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SingleQuotedPropertyWithDots()
        {
            JPath path = new JPath("['Blah.Ha']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah.Ha", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SingleQuotedPropertyWithBrackets()
        {
            JPath path = new JPath("['[*]']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("[*]", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SinglePropertyWithRoot()
        {
            JPath path = new JPath("$.Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JPath path = new JPath(" $.Blah ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void RootWithBadWhitespace()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unexpected character while parsing path:  ",
                () => { new JPath("$ .Blah"); });
        }

        [Test]
        public void NoFieldNameAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unexpected end while parsing path.",
                () => { new JPath("$.Blah."); });
        }

        [Test]
        public void RootWithBadWhitespace2()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unexpected character while parsing path:  ",
                () => { new JPath("$. Blah"); });
        }

        [Test]
        public void WildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void WildcardArrayWithRoot()
        {
            JPath path = new JPath("$.[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Test]
        public void RootArrayNoDot()
        {
            JPath path = new JPath("$[1]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Test]
        public void WildcardArray()
        {
            JPath path = new JPath("[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Test]
        public void WildcardArrayWithProperty()
        {
            JPath path = new JPath("[ * ].derp");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual("derp", ((FieldFilter)path.Filters[1]).Name);
        }

        [Test]
        public void QuotedWildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.['*']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("*", ((FieldFilter)path.Filters[0]).Name);
        }

        [Test]
        public void SingleScanWithRoot()
        {
            JPath path = new JPath("$..Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((ScanFilter)path.Filters[0]).Name);
        }

        [Test]
        public void WildcardScanWithRoot()
        {
            JPath path = new JPath("$..*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Test]
        public void WildcardScanWithRootWithWhitespace()
        {
            JPath path = new JPath("$..* ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Test]
        public void TwoProperties()
        {
            JPath path = new JPath("Blah.Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((FieldFilter)path.Filters[1]).Name);
        }

        [Test]
        public void OnePropertyOneScan()
        {
            JPath path = new JPath("Blah..Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[1]).Name);
        }

        [Test]
        public void SinglePropertyAndIndexer()
        {
            JPath path = new JPath("Blah[0]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [Test]
        public void SinglePropertyAndExistsQuery()
        {
            JPath path = new JPath("Blah[ ?( @..name ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Exists, expressions.Operator);
            Assert.AreEqual(1, expressions.Path.Count);
            Assert.AreEqual("name", ((ScanFilter)expressions.Path[0]).Name);
        }

        [Test]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JPath path = new JPath("Blah[ ?( @.name=='hi' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("hi", (string)expressions.Value);
        }

        [Test]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h'i", (string)expressions.Value);
        }

        [Test]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h\\i", (string)expressions.Value);
        }

        [Test]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unknown escape chracter: \i",
                () => { new JPath(@"Blah[ ?( @.name=='h\i' ) ]"); });
        }

        [Test]
        public void SinglePropertyAndFilterWithFalse()
        {
            JPath path = new JPath("Blah[ ?( @.name==false ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(false, (bool)expressions.Value);
        }

        [Test]
        public void SinglePropertyAndFilterWithTrue()
        {
            JPath path = new JPath("Blah[ ?( @.name==true ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(true, (bool)expressions.Value);
        }

        [Test]
        public void SinglePropertyAndFilterWithNull()
        {
            JPath path = new JPath("Blah[ ?( @.name==null ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(null, expressions.Value.Value);
        }

        [Test]
        public void FilterWithScan()
        {
            JPath path = new JPath("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual("name", ((ScanFilter)expressions.Path[0]).Name);
        }

        [Test]
        public void FilterWithNotEquals()
        {
            JPath path = new JPath("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [Test]
        public void FilterWithNotEquals2()
        {
            JPath path = new JPath("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [Test]
        public void FilterWithLessThan()
        {
            JPath path = new JPath("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThan, expressions.Operator);
        }

        [Test]
        public void FilterWithLessThanOrEquals()
        {
            JPath path = new JPath("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [Test]
        public void FilterWithGreaterThan()
        {
            JPath path = new JPath("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThan, expressions.Operator);
        }

        [Test]
        public void FilterWithGreaterThanOrEquals()
        {
            JPath path = new JPath("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [Test]
        public void FilterWithInteger()
        {
            JPath path = new JPath("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12, (int)expressions.Value);
        }

        [Test]
        public void FilterWithNegativeInteger()
        {
            JPath path = new JPath("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(-12, (int)expressions.Value);
        }

        [Test]
        public void FilterWithFloat()
        {
            JPath path = new JPath("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12.1d, (double)expressions.Value);
        }

        [Test]
        public void FilterExistWithAnd()
        {
            JPath path = new JPath("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, expressions.Operator);
            Assert.AreEqual(2, expressions.Expressions.Count);
            Assert.AreEqual("name", ((FieldFilter)((BooleanQueryExpression)expressions.Expressions[0]).Path[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, expressions.Expressions[0].Operator);
            Assert.AreEqual("title", ((FieldFilter)((BooleanQueryExpression)expressions.Expressions[1]).Path[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, expressions.Expressions[1].Operator);
        }

        [Test]
        public void FilterExistWithAndOr()
        {
            JPath path = new JPath("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, andExpression.Operator);
            Assert.AreEqual(2, andExpression.Expressions.Count);
            Assert.AreEqual("name", ((FieldFilter)((BooleanQueryExpression)andExpression.Expressions[0]).Path[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, andExpression.Expressions[0].Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.AreEqual(2, orExpression.Expressions.Count);
            Assert.AreEqual("title", ((FieldFilter)((BooleanQueryExpression)orExpression.Expressions[0]).Path[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orExpression.Expressions[0].Operator);
            Assert.AreEqual("pie", ((FieldFilter)((BooleanQueryExpression)orExpression.Expressions[1]).Path[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orExpression.Expressions[1].Operator);
        }

        [Test]
        public void BadOr1()
        {
            ExceptionAssert.Throws<JsonException>("Unexpected character while parsing path query: )",
                () => new JPath("[?(@.name||)]"));
        }

        [Test]
        public void BaddOr2()
        {
            ExceptionAssert.Throws<JsonException>("Unexpected character while parsing path query: |",
                () => new JPath("[?(@.name|)]"));
        }

        [Test]
        public void BaddOr3()
        {
            ExceptionAssert.Throws<JsonException>("Unexpected character while parsing path query: |",
                () => new JPath("[?(@.name|"));
        }

        [Test]
        public void BaddOr4()
        {
            ExceptionAssert.Throws<JsonException>("Path ended with open query.",
                () => new JPath("[?(@.name||"));
        }

        [Test]
        public void NoAtAfterOr()
        {
            ExceptionAssert.Throws<JsonException>("Unexpected character while parsing path query: s",
                () => new JPath("[?(@.name||s"));
        }

        [Test]
        public void NoPathAfterAt()
        {
            ExceptionAssert.Throws<JsonException>(@"Path ended with open query.",
                () => new JPath("[?(@.name||@"));
        }

        [Test]
        public void NoPathAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(@"Unexpected end while parsing path.",
                () => new JPath("[?(@.name||@."));
        }

        [Test]
        public void NoPathAfterDot2()
        {
            ExceptionAssert.Throws<JsonException>(@"Unexpected end while parsing path.",
                () => new JPath("[?(@.name||@.)]"));
        }

        [Test]
        public void FilterWithFloatExp()
        {
            JPath path = new JPath("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(5.56789e+0, (double)expressions.Value);
        }

        [Test]
        public void MultiplePropertiesAndIndexers()
        {
            JPath path = new JPath("Blah[0]..Two.Three[1].Four");
            Assert.AreEqual(6, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter) path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter) path.Filters[1]).Index);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[2]).Name);
            Assert.AreEqual("Three", ((FieldFilter)path.Filters[3]).Name);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.AreEqual("Four", ((FieldFilter)path.Filters[5]).Name);
        }

        [Test]
        public void BadCharactersInIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Unexpected character while parsing path indexer: [",
                () => { new JPath("Blah[[0]].Two.Three[1].Four"); });
        }

        [Test]
        public void UnclosedIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                @"Path ended with open indexer.",
                () => { new JPath("Blah[0"); });
        }

        [Test]
        public void IndexerOnly()
        {
            JPath path = new JPath("[111119990]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Test]
        public void IndexerOnlyWithWhitespace()
        {
            JPath path = new JPath("[  10  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Test]
        public void MultipleIndexes()
        {
            JPath path = new JPath("[111119990,3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Test]
        public void MultipleIndexesWithWhitespace()
        {
            JPath path = new JPath("[   111119990  ,   3   ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Test]
        public void MultipleQuotedIndexes()
        {
            JPath path = new JPath("['111119990','3']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Test]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JPath path = new JPath("[ '111119990' , '3' ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Test]
        public void SlicingIndexAll()
        {
            JPath path = new JPath("[111119990:3:2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void SlicingIndex()
        {
            JPath path = new JPath("[111119990:3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void SlicingIndexNegative()
        {
            JPath path = new JPath("[-111119990:-3:-2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void SlicingIndexEmptyStop()
        {
            JPath path = new JPath("[  -3  :  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void SlicingIndexEmptyStart()
        {
            JPath path = new JPath("[ : 1 : ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void SlicingIndexWhitespace()
        {
            JPath path = new JPath("[  -111119990  :  -3  :  -2  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Test]
        public void EmptyIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                "Array index expected.",
                () => { new JPath("[]"); });
        }

        [Test]
        public void IndexerCloseInProperty()
        {
            ExceptionAssert.Throws<JsonException>(
                "Unexpected character while parsing path: ]",
                () => { new JPath("]"); });
        }

        [Test]
        public void AdjacentIndexers()
        {
            JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
            Assert.AreEqual(4, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.AreEqual(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [Test]
        public void MissingDotAfterIndexer()
        {
            ExceptionAssert.Throws<JsonException>(
                "Unexpected character following indexer: B",
                () => { new JPath("[1]Blah"); });
        }
    }
}