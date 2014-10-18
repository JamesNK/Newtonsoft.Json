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
using System.Text;
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

            EatWhitespace();

            if (_expression.Length == _currentIndex)
                return;

            if (_expression[_currentIndex] == '$')
            {
                if (_expression.Length == 1)
                    return;

                // only increment position for "$." or "$["
                // otherwise assume property that starts with $
                char c = _expression[_currentIndex + 1];
                if (c == '.' || c == '[')
                {
                    _currentIndex++;
                    currentPartStartIndex = _currentIndex;
                }
            }

            if (!ParsePath(Filters, currentPartStartIndex, false))
            {
                int lastCharacterIndex = _currentIndex;

                EatWhitespace();

                if (_currentIndex < _expression.Length)
                    throw new JsonException("Unexpected character while parsing path: " + _expression[lastCharacterIndex]);
            }
        }

        private bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
        {
            bool scan = false;
            bool followingIndexer = false;
            bool followingDot = false;

            bool ended = false;
            while (_currentIndex < _expression.Length && !ended)
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
                            filters.Add(filter);
                            scan = false;
                        }

                        filters.Add(ParseIndexer(currentChar));
                        _currentIndex++;
                        currentPartStartIndex = _currentIndex;
                        followingIndexer = true;
                        followingDot = false;
                        break;
                    case ']':
                    case ')':
                        ended = true;
                        break;
                    case ' ':
                        //EatWhitespace();
                        if (_currentIndex < _expression.Length)
                            ended = true;
                        break;
                    case '.':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            if (member == "*")
                                member = null;
                            PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                            filters.Add(filter);
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
                        followingDot = true;
                        break;
                    default:
                        if (query && (currentChar == '=' || currentChar == '<' || currentChar == '!' || currentChar == '>' || currentChar == '|' || currentChar == '&'))
                        {
                            ended = true;
                        }
                        else
                        {
                            if (followingIndexer)
                                throw new JsonException("Unexpected character following indexer: " + currentChar);

                            _currentIndex++;
                        }
                        break;
                }
            }

            bool atPathEnd = (_currentIndex == _expression.Length);

            if (_currentIndex > currentPartStartIndex)
            {
                string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex).TrimEnd();
                if (member == "*")
                    member = null;
                PathFilter filter = (scan) ? (PathFilter)new ScanFilter() { Name = member } : new FieldFilter() { Name = member };
                filters.Add(filter);
            }
            else
            {
                // no field name following dot in path and at end of base path/query
                if (followingDot && (atPathEnd || query))
                    throw new JsonException("Unexpected end while parsing path.");
            }

            return atPathEnd;
        }

        private PathFilter ParseIndexer(char indexerOpenChar)
        {
            _currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            EnsureLength("Path ended with open indexer.");

            EatWhitespace();

            if (_expression[_currentIndex] == '\'')
            {
                return ParseQuotedField(indexerCloseChar);
            }
            else if (_expression[_currentIndex] == '?')
            {
                return ParseQuery(indexerCloseChar);
            }
            else
            {
                return ParseArrayIndexer(indexerCloseChar);
            }
        }

        private PathFilter ParseArrayIndexer(char indexerCloseChar)
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
                        return new ArrayMultipleIndexFilter { Indexes = indexes };
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

                        return new ArraySliceFilter { Start = startIndex, End = endIndex, Step = step };
                    }
                    else
                    {
                        if (length == 0)
                            throw new JsonException("Array index expected.");

                        string indexer = _expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        return new ArrayIndexFilter { Index = index };
                    }
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

                    return new ArrayIndexFilter();
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

        private PathFilter ParseQuery(char indexerCloseChar)
        {
            _currentIndex++;
            EnsureLength("Path ended with open indexer.");

            if (_expression[_currentIndex] != '(')
                throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);

            _currentIndex++;

            QueryExpression expression = ParseExpression();

            _currentIndex++;
            EnsureLength("Path ended with open indexer.");
            EatWhitespace();

            if (_expression[_currentIndex] != indexerCloseChar)
                throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);

            return new QueryFilter
            {
                Expression = expression
            };
        }

        private QueryExpression ParseExpression()
        {
            QueryExpression rootExpression = null;
            CompositeExpression parentExpression = null;

            while (_currentIndex < _expression.Length)
            {
                EatWhitespace();

                if (_expression[_currentIndex] != '@')
                    throw new JsonException("Unexpected character while parsing path query: " + _expression[_currentIndex]);

                _currentIndex++;

                List<PathFilter> expressionPath = new List<PathFilter>();

                if (ParsePath(expressionPath, _currentIndex, true))
                    throw new JsonException("Path ended with open query.");

                EatWhitespace();
                EnsureLength("Path ended with open query.");

                QueryOperator op;
                object value = null;
                if (_expression[_currentIndex] == ')'
                    || _expression[_currentIndex] == '|'
                    || _expression[_currentIndex] == '&')
                {
                    op = QueryOperator.Exists;
                }
                else
                {
                    op = ParseOperator();

                    EatWhitespace();
                    EnsureLength("Path ended with open query.");

                    value = ParseValue();

                    EatWhitespace();
                    EnsureLength("Path ended with open query.");
                }

                BooleanQueryExpression booleanExpression = new BooleanQueryExpression
                {
                    Path = expressionPath,
                    Operator = op,
                    Value = (op != QueryOperator.Exists) ? new JValue(value) : null
                };

                if (_expression[_currentIndex] == ')')
                {
                    if (parentExpression != null)
                    {
                        parentExpression.Expressions.Add(booleanExpression);
                        return rootExpression;
                    }
                    
                    return booleanExpression;
                }
                if (_expression[_currentIndex] == '&' && Match("&&"))
                {
                    if (parentExpression == null || parentExpression.Operator != QueryOperator.And)
                    {
                        CompositeExpression andExpression = new CompositeExpression { Operator = QueryOperator.And };

                        if (parentExpression != null)
                            parentExpression.Expressions.Add(andExpression);

                        parentExpression = andExpression;

                        if (rootExpression == null)
                            rootExpression = parentExpression;
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
                if (_expression[_currentIndex] == '|' && Match("||"))
                {
                    if (parentExpression == null || parentExpression.Operator != QueryOperator.Or)
                    {
                        CompositeExpression orExpression = new CompositeExpression { Operator = QueryOperator.Or };

                        if (parentExpression != null)
                            parentExpression.Expressions.Add(orExpression);

                        parentExpression = orExpression;

                        if (rootExpression == null)
                            rootExpression = parentExpression;
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
            }

            throw new JsonException("Path ended with open query.");
        }

        private object ParseValue()
        {
            char currentChar = _expression[_currentIndex];
            if (currentChar == '\'')
            {
                return ReadQuotedString();
            }
            else if (char.IsDigit(currentChar) || currentChar == '-')
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(currentChar);

                _currentIndex++;
                while (_currentIndex < _expression.Length)
                {
                    currentChar = _expression[_currentIndex];
                    if (currentChar == ' ' || currentChar == ')')
                    {
                        string numberText = sb.ToString();

                        if (numberText.IndexOfAny(new char[] { '.', 'E', 'e' }) != -1)
                        {
                            double d;
                            if (double.TryParse(numberText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out d))
                                return d;
                            else
                                throw new JsonException("Could not read query value.");
                        }
                        else
                        {
                            long l;
                            if (long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out l))
                                return l;
                            else
                                throw new JsonException("Could not read query value.");
                        }
                    }
                    else
                    {
                        sb.Append(currentChar);
                        _currentIndex++;
                    }
                }
            }
            else if (currentChar == 't')
            {
                if (Match("true"))
                    return true;
            }
            else if (currentChar == 'f')
            {
                if (Match("false"))
                    return false;
            }
            else if (currentChar == 'n')
            {
                if (Match("null"))
                    return null;
            }

            throw new JsonException("Could not read query value.");
        }

        private string ReadQuotedString()
        {
            StringBuilder sb = new StringBuilder();

            _currentIndex++;
            while (_currentIndex < _expression.Length)
            {
                char currentChar = _expression[_currentIndex];
                if (currentChar == '\\' && _currentIndex + 1 < _expression.Length)
                {
                    _currentIndex++;

                    if (_expression[_currentIndex] == '\'')
                        sb.Append('\'');
                    else if (_expression[_currentIndex] == '\\')
                        sb.Append('\\');
                    else
                        throw new JsonException(@"Unknown escape chracter: \" + _expression[_currentIndex]);

                    _currentIndex++;
                }
                else if (currentChar == '\'')
                {
                    _currentIndex++;
                    {
                        return sb.ToString();
                    }
                }
                else
                {
                    _currentIndex++;
                    sb.Append(currentChar);
                }
            }

            throw new JsonException("Path ended with an open string.");
        }

        private bool Match(string s)
        {
            int currentPosition = _currentIndex;
            foreach (char c in s)
            {
                if (currentPosition < _expression.Length && _expression[currentPosition] == c)
                    currentPosition++;
                else
                    return false;
            }

            _currentIndex = currentPosition;
            return true;
        }

        private QueryOperator ParseOperator()
        {
            if (_currentIndex + 1 >= _expression.Length)
                throw new JsonException("Path ended with open query.");

            if (Match("=="))
                return QueryOperator.Equals;
            if (Match("!=") || Match("<>"))
                return QueryOperator.NotEquals;
            if (Match("<="))
                return QueryOperator.LessThanOrEquals;
            if (Match("<"))
                return QueryOperator.LessThan;
            if (Match(">="))
                return QueryOperator.GreaterThanOrEquals;
            if (Match(">"))
                return QueryOperator.GreaterThan;

            throw new JsonException("Could not read query operator.");
        }

        private PathFilter ParseQuotedField(char indexerCloseChar)
        {
            List<string> fields = null;

            while (_currentIndex < _expression.Length)
            {
                string field = ReadQuotedString();

                EatWhitespace();
                EnsureLength("Path ended with open indexer.");

                if (_expression[_currentIndex] == indexerCloseChar)
                {
                    if (fields != null)
                    {
                        fields.Add(field);
                        return new FieldMultipleFilter { Names = fields };
                    }
                    else
                    {
                        return new FieldFilter { Name = field };
                    }
                }
                else if (_expression[_currentIndex] == ',')
                {
                    _currentIndex++;
                    EatWhitespace();

                    if (fields == null)
                        fields = new List<string>();

                    fields.Add(field);
                }
                else
                {
                    throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
                }
            }

            throw new JsonException("Path ended with open indexer.");
        }

        private void EnsureLength(string message)
        {
            if (_currentIndex >= _expression.Length)
                throw new JsonException(message);
        }

        internal IEnumerable<JToken> Evaluate(JToken t, bool errorWhenNoMatch)
        {
            return Evaluate(Filters, t, errorWhenNoMatch);
        }

        internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken t, bool errorWhenNoMatch)
        {
            IEnumerable<JToken> current = new[] { t };
            foreach (PathFilter filter in filters)
            {
                current = filter.ExecuteFilter(current, errorWhenNoMatch);
            }

            return current;
        }
    }
}