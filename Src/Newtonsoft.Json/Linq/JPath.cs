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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
  internal class JPath
  {
    private readonly string _expression;
    public List<object> Parts { get; private set; }

    private int _currentIndex;

    public JPath(string expression)
    {
      ValidationUtils.ArgumentNotNull(expression, "expression");
      _expression = expression;
      Parts = new List<object>();

      ParseMain();
    }

    private void ParseMain()
    {
      int currentPartStartIndex = _currentIndex;
      bool followingIndexer = false;

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
              Parts.Add(member);
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
              Parts.Add(member);
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
        Parts.Add(member);
      }
    }

    private void ParseIndexer(char indexerOpenChar)
    {
      _currentIndex++;

      char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';
      int indexerStart = _currentIndex;
      int indexerLength = 0;
      bool indexerClosed = false;

      while (_currentIndex < _expression.Length)
      {
        char currentCharacter = _expression[_currentIndex];
        if (char.IsDigit(currentCharacter))
        {
          indexerLength++;
        }
        else if (currentCharacter == indexerCloseChar)
        {
          indexerClosed = true;
          break;
        }
        else
        {
          throw new JsonException("Unexpected character while parsing path indexer: " + currentCharacter);
        }

        _currentIndex++;
      }

      if (!indexerClosed)
        throw new JsonException("Path ended with open indexer. Expected " + indexerCloseChar);

      if (indexerLength == 0)
        throw new JsonException("Empty path indexer.");

      string indexer = _expression.Substring(indexerStart, indexerLength);
      Parts.Add(Convert.ToInt32(indexer, CultureInfo.InvariantCulture));
    }

    internal JToken Evaluate(JToken root, bool errorWhenNoMatch)
    {
      JToken current = root;

      foreach (object part in Parts)
      {
        string propertyName = part as string;
        if (propertyName != null)
        {
          JObject o = current as JObject;
          if (o != null)
          {
            current = o[propertyName];

            if (current == null && errorWhenNoMatch)
              throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, propertyName));
          }
          else
          {
            if (errorWhenNoMatch)
              throw new JsonException("Property '{0}' not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, propertyName, current.GetType().Name));

            return null;
          }
        }
        else
        {
          int index = (int) part;

          JArray a = current as JArray;
          JConstructor c = current as JConstructor;

          if (a != null)
          {
            if (a.Count <= index)
            {
              if (errorWhenNoMatch)
                throw new JsonException("Index {0} outside the bounds of JArray.".FormatWith(CultureInfo.InvariantCulture, index));
              
              return null;
            }

            current = a[index];
          }
          else if (c != null)
          {
            if (c.Count <= index)
            {
              if (errorWhenNoMatch)
                throw new JsonException("Index {0} outside the bounds of JConstructor.".FormatWith(CultureInfo.InvariantCulture, index));

              return null;
            }

            current = c[index];
          }
          else
          {
            if (errorWhenNoMatch)
              throw new JsonException("Index {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, index, current.GetType().Name));

            return null;
          }
        }
      }

      return current;
    }
  }
}