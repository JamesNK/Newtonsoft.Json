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
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    internal enum FilterType
    {
        ArrayIndex,
        ArrayMultipleIndex,
        ArraySlice,
        Field,
        Scan,
        Query
    }

    internal abstract class PathFilter
    {
        public abstract FilterType Type { get; }
    }

    internal class ArrayIndexFilter : PathFilter
    {
        public int Index { get; set; }

        public override FilterType Type
        {
            get { return FilterType.ArrayIndex; }
        }
    }

    internal class ArrayMultipleIndexFilter : PathFilter
    {
        public List<int> Indexes { get; set; }

        public override FilterType Type
        {
            get { return FilterType.ArrayMultipleIndex; }
        }
    }

    internal class ArraySliceFilter : PathFilter
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int? Skip { get; set; }

        public override FilterType Type
        {
            get { return FilterType.ArraySlice; }
        }
    }

    internal class FieldFilter : PathFilter
    {
        public string Name { get; set; }

        public override FilterType Type
        {
            get { return FilterType.Field; }
        }
    }

    internal class ScanFilter : PathFilter
    {
        public string Name { get; set; }

        public override FilterType Type
        {
            get { return FilterType.Scan; }
        }
    }

    internal class QueryFilter : PathFilter
    {
        public List<object> Expression { get; set; }

        public override FilterType Type
        {
            get { return FilterType.Query; }
        }
    }

    internal class JPath
    {
        private readonly string _expression;
        public List<PathFilter> Parts { get; private set; }

        private int _currentIndex;

        public JPath(string expression)
        {
            ValidationUtils.ArgumentNotNull(expression, "expression");
            _expression = expression;
            Parts = new List<PathFilter>();

            ParseMain();
        }

        private void ParseMain()
        {
            int currentPartStartIndex = _currentIndex;
            bool followingIndexer = false;
            bool scan = false;

            EnsureLength("Empty path.");

            if (_expression[_currentIndex] == '$')
            {
                _currentIndex++;

                EnsureLength("Unexpected end while parsing path.");

                if (_expression[_currentIndex] != '.')
                    throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);

                _currentIndex++;

                EnsureLength("Unexpected end while parsing path.");

                if (_expression[_currentIndex] == '.')
                {
                    scan = true;
                    _currentIndex++;
                    currentPartStartIndex = 3;
                }
                else
                {
                    currentPartStartIndex = 2;
                }
            }

            while (_currentIndex < _expression.Length)
            {
                char currentChar = _expression[_currentIndex];

                switch (currentChar)
                {
                    case '[':
                    case '(':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                            Parts.Add(filter);
                            scan = false;
                        }

                        ParseIndexer(currentChar);
                        currentPartStartIndex = _currentIndex + 1;
                        followingIndexer = true;
                        break;
                    case ']':
                    case ')':
                        throw new JsonException("Unexpected character while parsing path: " + currentChar);
                    case '.':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                            Parts.Add(filter);
                            scan = false;
                        }
                        if (_currentIndex + 1 < _expression.Length && _expression[_currentIndex + 1] == '.')
                        {
                            scan = true;
                            _currentIndex++;
                        }
                        currentPartStartIndex = _currentIndex + 1;
                        followingIndexer = false;
                        break;
                    default:
                        if (followingIndexer)
                            throw new JsonException("Unexpected character following indexer: " + currentChar);
                        break;
                }

                _currentIndex++;
            }

            if (_currentIndex > currentPartStartIndex)
            {
                string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                Parts.Add(filter);
            }
        }

        private void ParseIndexer(char indexerOpenChar)
        {
            _currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            EnsureLength("Path ended with open indexer.");

            if (_expression[_currentIndex] == '\'')
            {
                ParseQuotedField(indexerCloseChar);
            }
            else if (_expression[_currentIndex] == '?')
            {
                ParseQuery(indexerCloseChar);
            }
            else
            {
                ParseArrayIndexer(indexerCloseChar);
            }
        }

        private void ParseArrayIndexer(char indexerCloseChar)
        {
            int start = _currentIndex;
            List<int> indexes = null;
            int? startIndex = null;
            int? endIndex = null;
            int? step = null;

            while (_currentIndex < _expression.Length)
            {
                char currentCharacter = _expression[_currentIndex];

                if (currentCharacter == indexerCloseChar)
                {
                    int length = _currentIndex - start;

                    if (length == 0)
                        throw new JsonException("Array index expected.");

                    string indexer = _expression.Substring(start, length);
                    int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                    if (indexes != null)
                    {
                        indexes.Add(index);
                        Parts.Add(new ArrayMultipleIndexFilter { Indexes = indexes });
                    }
                    else
                    {
                        Parts.Add(new ArrayIndexFilter { Index = index });
                    }

                    return;
                }

                if (currentCharacter == ',')
                {
                    int length = _currentIndex - start;

                    if (length == 0)
                        throw new JsonException("Array index expected.");

                    if (indexes == null)
                        indexes = new List<int>();

                    string indexer = _expression.Substring(start, length);
                    indexes.Add(Convert.ToInt32(indexer, CultureInfo.InvariantCulture));

                    start = _currentIndex + 1;
                }
                else if (!char.IsDigit(currentCharacter))
                {
                    throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                }

                _currentIndex++;
            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void ParseQuery(char indexerCloseChar)
        {
            _currentIndex++;
            EnsureLength("Path ended with open indexer.");

            if (_expression[_currentIndex] != '(')
                throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);

            List<object> expressions = new List<object>();

            while (true)
            {
                ParseExpression();
                expressions.Add(string.Empty);

                if (_expression[_currentIndex] == ')')
                {
                    _currentIndex++;
                    EnsureLength("Path ended with open indexer.");
                    if (_expression[_currentIndex] != indexerCloseChar)
                        throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
                    
                    Parts.Add(new QueryFilter
                    {
                        Expression = expressions
                    });
                    return;
                }
                else
                {
                    _currentIndex++;
                }
            }
        }

        private void ParseExpression()
        {
            _currentIndex++;
            while (_currentIndex < _expression.Length)
            {
                char currentCharacter = _expression[_currentIndex];

                if (currentCharacter == ')')
                {
                    return;
                }
                else
                {
                    _currentIndex++;
                }
            }
        }

        private void ParseQuotedField(char indexerCloseChar)
        {
            _currentIndex++;
            int start = _currentIndex;

            while (_currentIndex < _expression.Length)
            {
                char currentCharacter = _expression[_currentIndex];
                if (currentCharacter == '\'')
                {
                    int length = _currentIndex - start;

                    if (length == 0)
                        throw new JsonException("Empty path indexer.");

                    // check that character after the quote is to close the index
                    _currentIndex++;
                    EnsureLength("Path ended with open indexer.");
                    if (_expression[_currentIndex] != indexerCloseChar)
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);

                    string indexer = _expression.Substring(start, length);
                    Parts.Add(new FieldFilter { Name = indexer });

                    return;
                }

                _currentIndex++;
            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void EnsureLength(string message)
        {
            if (_currentIndex >= _expression.Length)
                throw new JsonException(message);
        }

        internal IEnumerable<JToken> Evaluate(JToken root, bool errorWhenNoMatch)
        {
            IEnumerable<JToken> current = new [] { root };

            foreach (PathFilter filter in Parts)
            {
                switch (filter.Type)
                {
                    case FilterType.ArrayIndex:
                        current = ExecuteArrayIndexFilter(current, errorWhenNoMatch, (ArrayIndexFilter)filter);
                        break;
                    case FilterType.ArrayMultipleIndex:
                        current = ExecuteArrayMultipleIndexFilter(current, errorWhenNoMatch, (ArrayMultipleIndexFilter)filter);
                        break;
                    case FilterType.Field:
                        current = ExecuteFieldFilter(current, errorWhenNoMatch, (FieldFilter)filter);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return current;
        }

        private IEnumerable<JToken> ExecuteArrayMultipleIndexFilter(IEnumerable<JToken> current, bool errorWhenNoMatch, ArrayMultipleIndexFilter arrayMultipleIndexFilter)
        {
            foreach (JToken t in current)
            {
                foreach (int i in arrayMultipleIndexFilter.Indexes)
                {
                    JToken v = GetTokenIndex(t, errorWhenNoMatch, i);

                    if (v != null)
                        yield return v;
                }
            }
        }

        private static IEnumerable<JToken> ExecuteFieldFilter(IEnumerable<JToken> current, bool errorWhenNoMatch, FieldFilter fieldFilter)
        {
            foreach (JToken t in current)
            {
                JObject o = t as JObject;
                if (o != null)
                {
                    JToken v = o[fieldFilter.Name];

                    if (v == null && errorWhenNoMatch)
                        throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, fieldFilter.Name));

                    yield return v;
                }
                else
                {
                    if (errorWhenNoMatch)
                        throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, fieldFilter.Name, t.GetType().Name));
                }
            }
        }

        private static IEnumerable<JToken> ExecuteArrayIndexFilter(IEnumerable<JToken> current, bool errorWhenNoMatch, ArrayIndexFilter arrayIndexFilter)
        {
            foreach (JToken t in current)
            {
                JToken v = GetTokenIndex(t, errorWhenNoMatch, arrayIndexFilter.Index);

                if (v != null)
                    yield return v;
            }
        }

        private static JToken GetTokenIndex(JToken t, bool errorWhenNoMatch, int index)
        {
            JArray a = t as JArray;
            JConstructor c = t as JConstructor;

            if (a != null)
            {
                if (a.Count <= index)
                {
                    if (errorWhenNoMatch)
                        throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, index));

                    return null;
                }

                return a[index];
            }
            else if (c != null)
            {
                if (c.Count <= index)
                {
                    if (errorWhenNoMatch)
                        throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith(CultureInfo.InvariantCulture, index));

                    return null;
                }

                return c[index];
            }
            else
            {
                if (errorWhenNoMatch)
                    throw new JsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, t.GetType().Name));

                return null;
            }
        }
    }
}