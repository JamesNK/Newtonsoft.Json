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
    internal struct StringReference
    {
        private readonly char[] _chars;
        private readonly int _startIndex;
        private readonly int _length;

        public char this[int i]
        {
            get { return _chars[i]; }
        }

        public char[] Chars
        {
            get { return _chars; }
        }

        public int StartIndex
        {
            get { return _startIndex; }
        }

        public int Length
        {
            get { return _length; }
        }

        public StringReference(char[] chars, int startIndex, int length)
        {
            _chars = chars;
            _startIndex = startIndex;
            _length = length;
        }

        public override string ToString()
        {
            return new string(_chars, _startIndex, _length);
        }
    }

    internal static class StringReferenceExtensions
    {
        public static int IndexOf(this StringReference s, char c, int startIndex, int length)
        {
            int index = Array.IndexOf(s.Chars, c, s.StartIndex + startIndex, length);
            if (index == -1)
            {
                return -1;
            }

            return index - s.StartIndex;
        }

        public static bool StartsWith(this StringReference s, string text)
        {
            if (text.Length > s.Length)
            {
                return false;
            }

            char[] chars = s.Chars;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != chars[i + s.StartIndex])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EndsWith(this StringReference s, string text)
        {
            if (text.Length > s.Length)
            {
                return false;
            }

            char[] chars = s.Chars;

            int start = s.StartIndex + s.Length - text.Length;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != chars[i + start])
                {
                    return false;
                }
            }

            return true;
        }
    }
}