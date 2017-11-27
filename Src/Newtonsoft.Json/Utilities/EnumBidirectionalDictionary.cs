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
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Utilities
{
    internal class EnumBidirectionalDictionary
    {
        private readonly IDictionary<string, string> _firstToSecond;
        private readonly IDictionary<string, string> _secondToFirst;
        private readonly string _duplicateFirstErrorMessage;
        private readonly string _duplicateSecondErrorMessage;

        public EnumBidirectionalDictionary()
            : this(
                "Duplicate item already exists for '{0}'.",
                "Duplicate item already exists for '{0}'.")
        {
        }

        public EnumBidirectionalDictionary(string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
        {
            // By default, our dictionaries are case sensitive.
            _firstToSecond = new Dictionary<string, string>(StringComparer.Ordinal);
            _secondToFirst = new Dictionary<string, string>(StringComparer.Ordinal);

            _duplicateFirstErrorMessage = duplicateFirstErrorMessage;
            _duplicateSecondErrorMessage = duplicateSecondErrorMessage;
        }

        public void Set(string first, string second)
        {
            if (_firstToSecond.TryGetValue(first, out string existingSecond))
            {
                if (!existingSecond.Equals(second))
                {
                    throw new ArgumentException(_duplicateFirstErrorMessage.FormatWith(CultureInfo.InvariantCulture, first));
                }
            }

            if (_secondToFirst.TryGetValue(second, out string existingFirst))
            {
                if (!existingFirst.Equals(first))
                {
                    throw new ArgumentException(_duplicateSecondErrorMessage.FormatWith(CultureInfo.InvariantCulture, second));
                }
            }

            _firstToSecond.Add(first, second);
            _secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(string first, out string second, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return _firstToSecond.TryGetValue(first, out second);
            }
            else
            {
                var matchingElements = _firstToSecond.Where(kvp => kvp.Key.Equals(first, StringComparison.OrdinalIgnoreCase));
                second = matchingElements.Any() ? matchingElements.First().Value : null;
                return second != null;
            }
        }

        public bool TryGetBySecond(string second, out string first, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return _secondToFirst.TryGetValue(second, out first);
            }
            else
            {
                var matchingElements = _secondToFirst.Where(kvp => kvp.Key.Equals(second, StringComparison.OrdinalIgnoreCase));
                first = matchingElements.Any() ? matchingElements.First().Value : null;
                return first != null;
            }
        }
    }
}