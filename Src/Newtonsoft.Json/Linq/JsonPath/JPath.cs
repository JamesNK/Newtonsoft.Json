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
        private static readonly char[] FloatCharacters = new[] {'.', 'E', 'e'};

        private readonly string _expression;
        public List<PathFilter> Filters { get; }

        private int _currentIndex;

        public JPath(string expression)
        {
            ValidationUtils.ArgumentNotNull(expression, nameof(expression));
            _expression = expression;
            Filters = new List<PathFilter>();

            ParseMain();
        }

        private void ParseMain()
        {
            int currentPartStartIndex = _currentIndex;

            EatWhitespace();

            if (_expression.Length == _currentIndex)
            {
                return;
            }

            if (_expression[_currentIndex] == '$')
            {
                if (_expression.Length == 1)
                {
                    return;
                }

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
                {
                    throw new JsonException("Unexpected character while parsing path: " + _expression[lastCharacterIndex]);
                }
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
                            if (member == "*")
                            {
                                member = null;
                            }

                            filters.Add(CreatePathFilter(member, scan));
                            scan = false;
                        }

                        filters.Add(ParseIndexer(currentChar, scan));
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
                        if (_currentIndex < _expression.Length)
                        {
                            ended = true;
                        }
                        break;
                    case '.':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            string member = _expression.Substring(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            if (member == "*")
                            {
                                member = null;
                            }

                            filters.Add(CreatePathFilter(member, scan));
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
                            {
                                throw new JsonException("Unexpected character following indexer: " + currentChar);
                            }

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
                {
                    member = null;
                }
                filters.Add(CreatePathFilter(member, scan));
            }
            else
            {
                // no field name following dot in path and at end of base path/query
                if (followingDot && (atPathEnd || query))
                {
                    throw new JsonException("Unexpected end while parsing path.");
                }
            }

            return atPathEnd;
        }

        private static PathFilter CreatePathFilter(string member, bool scan)
        {
            PathFilter filter = (scan) ? (PathFilter)new ScanFilter {Name = member} : new FieldFilter {Name = member};
            return filter;
        }

        private PathFilter ParseIndexer(char indexerOpenChar, bool scan)
        {
            _currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            EnsureLength("Path ended with open indexer.");

            EatWhitespace();

            if (_expression[_currentIndex] == '\'')
            {
                return ParseQuotedField(indexerCloseChar, scan);
            }
            else if (_expression[_currentIndex] == '?')
            {
                return ParseQuery(indexerCloseChar, scan);
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
                        {
                            throw new JsonException("Array index expected.");
                        }

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
                            {
                                endIndex = index;
                            }
                            else
                            {
                                step = index;
                            }
                        }

                        return new ArraySliceFilter { Start = startIndex, End = endIndex, Step = step };
                    }
                    else
                    {
                        if (length == 0)
                        {
                            throw new JsonException("Array index expected.");
                        }

                        string indexer = _expression.Substring(start, length);
                        int index = Convert.ToInt32(indexer, CultureInfo.InvariantCulture);

                        return new ArrayIndexFilter { Index = index };
                    }
                }
                else if (currentCharacter == ',')
                {
                    int length = (end ?? _currentIndex) - start;

                    if (length == 0)
                    {
                        throw new JsonException("Array index expected.");
                    }

                    if (indexes == null)
                    {
                        indexes = new List<int>();
                    }

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
                    {
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                    }

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
                        {
                            startIndex = index;
                        }
                        else if (colonCount == 1)
                        {
                            endIndex = index;
                        }
                        else
                        {
                            step = index;
                        }
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
                    {
                        throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
                    }

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
                {
                    break;
                }

                _currentIndex++;
            }
        }

        private PathFilter ParseQuery(char indexerCloseChar, bool scan)
        {
            _currentIndex++;
            EnsureLength("Path ended with open indexer.");

            if (_expression[_currentIndex] != '(')
            {
                throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
            }

            _currentIndex++;

            QueryExpression expression = ParseExpression();

            _currentIndex++;
            EnsureLength("Path ended with open indexer.");
            EatWhitespace();

            if (_expression[_currentIndex] != indexerCloseChar)
            {
                throw new JsonException("Unexpected character while parsing path indexer: " + _expression[_currentIndex]);
            }

            if (!scan)
            {
                return new QueryFilter
                {
                    Expression = expression
                };
            }
            else
            {
                return new QueryScanFilter
                {
                    Expression = expression
                };
            }
        }

        private bool TryParseExpression(out List<PathFilter> expressionPath)
        {
            if (_expression[_currentIndex] == '$')
            {
                expressionPath = new List<PathFilter>();
                expressionPath.Add(RootFilter.Instance);
            }
            else if (_expression[_currentIndex] == '@')
            {
                expressionPath = new List<PathFilter>();
            }
            else
            {
                expressionPath = null;
                return false;
            }

            _currentIndex++;

            if (ParsePath(expressionPath, _currentIndex, true))
            {
                throw new JsonException("Path ended with open query.");
            }

            return true;
        }

        private JsonException CreateUnexpectedCharacterException()
        {
            return new JsonException("Unexpected character while parsing path query: " + _expression[_currentIndex]);
        }

        private object ParseSide()
        {
            EatWhitespace();

            if (TryParseExpression(out var expressionPath))
            {
                EatWhitespace();
                EnsureLength("Path ended with open query.");

                return expressionPath;
            }

            if (TryParseValue(out var value))
            {
                EatWhitespace();
                EnsureLength("Path ended with open query.");

                return new JValue(value);
            }

            throw CreateUnexpectedCharacterException();
        }

        private QueryExpression ParseExpression()
        {
            QueryExpression rootExpression = null;
            CompositeExpression parentExpression = null;

            while (_currentIndex < _expression.Length)
            {
                object left = ParseSide();
                object right = null;

                QueryOperator op;
                if (_expression[_currentIndex] == ')'
                    || _expression[_currentIndex] == '|'
                    || _expression[_currentIndex] == '&')
                {
                    op = QueryOperator.Exists;
                }
                else
                {
                    op = ParseOperator();

                    right = ParseSide();
                }

                BooleanQueryExpression booleanExpression = new BooleanQueryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
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
                if (_expression[_currentIndex] == '&')
                {
                    if (!Match("&&"))
                    {
                        throw CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.And)
                    {
                        CompositeExpression andExpression = new CompositeExpression { Operator = QueryOperator.And };

                        parentExpression?.Expressions.Add(andExpression);

                        parentExpression = andExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
                if (_expression[_currentIndex] == '|')
                {
                    if (!Match("||"))
                    {
                        throw CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.Or)
                    {
                        CompositeExpression orExpression = new CompositeExpression { Operator = QueryOperator.Or };

                        parentExpression?.Expressions.Add(orExpression);

                        parentExpression = orExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
            }

            throw new JsonException("Path ended with open query.");
        }

        private bool TryParseValue(out object value)
        {
            char currentChar = _expression[_currentIndex];
            if (currentChar == '\'')
            {
                value = ReadQuotedString();
                return true;
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

                        if (numberText.IndexOfAny(FloatCharacters) != -1)
                        {
                            bool result = double.TryParse(numberText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d);
                            value = d;
                            return result;
                        }
                        else
                        {
                            bool result = long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l);
                            value = l;
                            return result;
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
                {
                    value = true;
                    return true;
                }
            }
            else if (currentChar == 'f')
            {
                if (Match("false"))
                {
                    value = false;
                    return true;
                }
            }
            else if (currentChar == 'n')
            {
                if (Match("null"))
                {
                    value = null;
                    return true;
                }
            }
            else if (currentChar == '/')
            {
                value = ReadRegexString();
                return true;
            }

            value = null;
            return false;
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
                    currentChar = _expression[_currentIndex];

                    char resolvedChar;
                    switch (currentChar)
                    {
                        case 'b':
                            resolvedChar = '\b';
                            break;
                        case 't':
                            resolvedChar = '\t';
                            break;
                        case 'n':
                            resolvedChar = '\n';
                            break;
                        case 'f':
                            resolvedChar = '\f';
                            break;
                        case 'r':
                            resolvedChar = '\r';
                            break;
                        case '\\':
                        case '"':
                        case '\'':
                        case '/':
                            resolvedChar = currentChar;
                            break;
                        default:
                            throw new JsonException(@"Unknown escape character: \" + currentChar);
                    }

                    sb.Append(resolvedChar);

                    _currentIndex++;
                }
                else if (currentChar == '\'')
                {
                    _currentIndex++;
                    return sb.ToString();
                }
                else
                {
                    _currentIndex++;
                    sb.Append(currentChar);
                }
            }

            throw new JsonException("Path ended with an open string.");
        }

        private string ReadRegexString()
        {
            int startIndex = _currentIndex;

            _currentIndex++;
            while (_currentIndex < _expression.Length)
            {
                char currentChar = _expression[_currentIndex];

                // handle escaped / character
                if (currentChar == '\\' && _currentIndex + 1 < _expression.Length)
                {
                    _currentIndex += 2;
                }
                else if (currentChar == '/')
                {
                    _currentIndex++;

                    while (_currentIndex < _expression.Length)
                    {
                        currentChar = _expression[_currentIndex];

                        if (char.IsLetter(currentChar))
                        {
                            _currentIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return _expression.Substring(startIndex, _currentIndex - startIndex);
                }
                else
                {
                    _currentIndex++;
                }
            }

            throw new JsonException("Path ended with an open regex.");
        }

        private bool Match(string s)
        {
            int currentPosition = _currentIndex;
            foreach (char c in s)
            {
                if (currentPosition < _expression.Length && _expression[currentPosition] == c)
                {
                    currentPosition++;
                }
                else
                {
                    return false;
                }
            }

            _currentIndex = currentPosition;
            return true;
        }

        private QueryOperator ParseOperator()
        {
            if (_currentIndex + 1 >= _expression.Length)
            {
                throw new JsonException("Path ended with open query.");
            }

            if (Match("=="))
            {
                return QueryOperator.Equals;
            }

            if (Match("=~"))
            {
                return QueryOperator.RegexEquals;
            }

            if (Match("!=") || Match("<>"))
            {
                return QueryOperator.NotEquals;
            }
            if (Match("<="))
            {
                return QueryOperator.LessThanOrEquals;
            }
            if (Match("<"))
            {
                return QueryOperator.LessThan;
            }
            if (Match(">="))
            {
                return QueryOperator.GreaterThanOrEquals;
            }
            if (Match(">"))
            {
                return QueryOperator.GreaterThan;
            }

            throw new JsonException("Could not read query operator.");
        }

        private PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
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
                        return (scan)
                            ? (PathFilter)new ScanMultipleFilter { Names = fields }
                            : (PathFilter)new FieldMultipleFilter { Names = fields };
                    }
                    else
                    {
                        return CreatePathFilter(field, scan);
                    }
                }
                else if (_expression[_currentIndex] == ',')
                {
                    _currentIndex++;
                    EatWhitespace();

                    if (fields == null)
                    {
                        fields = new List<string>();
                    }

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
            {
                throw new JsonException(message);
            }
        }

        internal IEnumerable<JToken> Evaluate(JToken root, JToken t, bool errorWhenNoMatch)
        {
            return Evaluate(Filters, root, t, errorWhenNoMatch);
        }

        internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, bool errorWhenNoMatch)
        {
            IEnumerable<JToken> current = new[] { t };
            foreach (PathFilter filter in filters)
            {
                current = filter.ExecuteFilter(root, current, errorWhenNoMatch);
            }

            return current;
        }
    }
}