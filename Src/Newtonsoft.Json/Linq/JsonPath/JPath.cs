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
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class JPath
    {
        private readonly string _expression;
        public List<PathFilter> Filters { get; private set; }

        private int _currentIndex;

        public JPath(string expression)
        {
            ValidationUtils.ArgumentNotNull(expression, "expression");
            _expression = expression;
            Filters = new List<PathFilter>();

            ParseMain();
        }

        private void ParseMain()
        {
            int currentPartStartIndex = _currentIndex;
            bool followingIndexer = false;
            bool scan = false;

            if (_expression.Length == 0)
                return;

            EatWhitespace();

            if (_expression[_currentIndex] == '$')
            {
                _currentIndex++;

                EnsureLength("Unexpected end while parsing path.");

                if (_expression[_currentIndex] != '.')
                    throw new JsonException("Unexpected character while parsing path: " + _expression[_currentIndex]);

                _currentIndex++;

                EnsureLength("Unexpected end while parsing path.");

                if (_expression[_currentIndex] == '.')
                {
                    scan = true;
                    _currentIndex++;
                }

                currentPartStartIndex = _currentIndex;
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
                            Filters.Add(filter);
                            scan = false;
                        }

                        ParseIndexer(currentChar);
                        _currentIndex++;
                        currentPartStartIndex = _currentIndex;
                        followingIndexer = true;
                        break;
                    case ']':
                    case ')':
                        throw new JsonException("Unexpected character while parsing path: " + currentChar);
                    case ' ':
                        EatWhitespace();
                        if (_currentIndex < _expression.Length)
                            throw new JsonException("Unexpected character while parsing path: " + currentChar);
                        break;
                    case '.':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            if (member == "*")
                                member = null;
                            PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                            Filters.Add(filter);
                            scan = false;
                        }
                        if (_currentIndex + 1 < _expression.Length && _expression[_currentIndex + 1] == '.')
                        {
                            scan = true;
                            _currentIndex++;
                        }
                        _currentIndex++;
                        currentPartStartIndex = _currentIndex;
                        followingIndexer = false;
                        break;
                    default:
                        if (followingIndexer)
                            throw new JsonException("Unexpected character following indexer: " + currentChar);

                        _currentIndex++;
                        break;
                }
            }

            if (_currentIndex > currentPartStartIndex)
            {
                string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex).TrimEnd();
                if (member == "*")
                    member = null;
                PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                Filters.Add(filter);
            }
        }

        private void ParseIndexer(char indexerOpenChar)
        {
            _currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            EnsureLength("Path ended with open indexer.");

            EatWhitespace();

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
            int? end = null;
            List<int> indexes = null;
            int colonCount = 0;
            int? startIndex = null;
            int? endIndex = null;
            int? step = null;

            while (_currentIndex < _expression.Length)
            {
                char currentCharacter = _expression[_currentIndex];

                if (currentCharacter == ' ')
                {
                    end = _currentIndex;
                    EatWhitespace();
                    continue;
                }

                if (currentCharacter == indexerCloseChar)
                {
                    int length = (end ?? _currentIndex) - start;

                    if (indexes != null)
                    {
                        if (length == 0)
                            throw new JsonException("Array index expected.");

                        string indexer = _expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        indexes.Add(index);
                        Filters.Add(new ArrayMultipleIndexFilter { Indexes = indexes });
                    }
                    else if (colonCount > 0)
                    {
                        if (length > 0)
                        {
                            string indexer = _expression.Substring(start, length);
                            int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                            if (colonCount == 1)
                                endIndex = index;
                            else
                                step = index;
                        }

                        Filters.Add(new ArraySliceFilter { Start = startIndex, End = endIndex, Step = step });
                    }
                    else
                    {
                        if (length == 0)
                            throw new JsonException("Array index expected.");

                        string indexer = _expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        Filters.Add(new ArrayIndexFilter { Index = index });
                    }

                    return;
                } else if (currentCharacter == ',')
                {
                    int length = (end ?? _currentIndex) - start;

                    if (length == 0)
                        throw new JsonException("Array index expected.");

                    if (indexes == null)
                        indexes = new List<int>();

                    string indexer = _expression.Substring(start, length);
                    indexes.Add(Convert.ToInt32(indexer, CultureInfo.InvariantCulture));

                    _currentIndex++;

                    EatWhitespace();

                    start = _currentIndex;
                    end = null;
                }
                else if (currentCharacter == '*')
                {
                    _currentIndex++;
                    EnsureLength("Path ended with open indexer.");
                    EatWhitespace();

                    if (_expression[_currentIndex] != indexerCloseChar)
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);

                    Filters.Add(new ArrayIndexFilter());
                    return;
                }
                else if (currentCharacter == ':')
                {
                    int length = (end ?? _currentIndex) - start;

                    if (length > 0)
                    {
                        string indexer = _expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        if (colonCount == 0)
                            startIndex = index;
                        else if (colonCount == 1)
                            endIndex = index;
                        else
                            step = index;
                    }

                    colonCount++;

                    _currentIndex++;

                    EatWhitespace();

                    start = _currentIndex;
                    end = null;
                }
                else if (!char.IsDigit(currentCharacter) && currentCharacter != '-')
                {
                    throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                }
                else
                {
                    if (end != null)
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);

                    _currentIndex++;
                }

            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void EatWhitespace()
        {
            while (_currentIndex < _expression.Length)
            {
                if (_expression[_currentIndex] != ' ')
                    break;

                _currentIndex++;
            }
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
                    
                    Filters.Add(new QueryFilter
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
                    EatWhitespace();

                    if (_expression[_currentIndex] != indexerCloseChar)
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);

                    string indexer = _expression.Substring(start, length);
                    Filters.Add(new FieldFilter { Name = indexer });

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

            foreach (PathFilter filter in Filters)
            {
                current = filter.ExecuteFilter(current, errorWhenNoMatch);
            }

            return current;
        }
    }
}