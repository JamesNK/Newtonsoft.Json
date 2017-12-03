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

namespace Newtonsoft.Json.Utilities
{
    internal class EnumBidirectionalDictionary
    {
        private readonly BidirectionalDictionary<string, string> _caseSensitive;
        private readonly BidirectionalDictionary<string, string> _caseInsensitive;

        public EnumBidirectionalDictionary()
        {
            _caseSensitive = new BidirectionalDictionary<string, string>(StringComparer.Ordinal, StringComparer.Ordinal);
            _caseInsensitive = new BidirectionalDictionary<string, string>(StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase);
        }

        public void Set(string first, string second)
        {
            _caseSensitive.Set(first, second);
            if (!_caseInsensitive.TryGetByFirst(first, out _) && !_caseInsensitive.TryGetBySecond(second, out _))
            {
                _caseInsensitive.Set(first, second);
            }
        }

        public bool TryGetByFirst(string first, out string second, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return _caseSensitive.TryGetByFirst(first, out second);
            }
            else
            {
                return _caseInsensitive.TryGetByFirst(first, out second);
            }
        }

        public bool TryGetBySecond(string second, out string first, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return _caseSensitive.TryGetBySecond(second, out first);
            }
            else
            {
                return _caseInsensitive.TryGetBySecond(second, out first);
            }
        }
    }
}