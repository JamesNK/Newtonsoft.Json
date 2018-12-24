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
    /// <summary>
    /// Builds a string. Unlike <see cref="System.Text.StringBuilder"/> this class lets you reuse its internal buffer.
    /// </summary>
    internal struct StringBuffer
    {
        private char[]? _buffer;
        private int _position;

        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsEmpty => _buffer == null;

        public StringBuffer(IArrayPool<char>? bufferPool, int initalSize) : this(BufferUtils.RentBuffer(bufferPool, initalSize))
        {
        }

        private StringBuffer(char[] buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public void Append(IArrayPool<char>? bufferPool, char value)
        {
            // test if the buffer array is large enough to take the value
            if (_position == _buffer!.Length)
            {
                EnsureSize(bufferPool, 1);
            }

            // set value and increment poisition
            _buffer![_position++] = value;
        }

        public void Append(IArrayPool<char>? bufferPool, char[] buffer, int startIndex, int count)
        {
            if (_position + count >= _buffer!.Length)
            {
                EnsureSize(bufferPool, count);
            }

            Array.Copy(buffer, startIndex, _buffer, _position, count);

            _position += count;
        }

        public void Clear(IArrayPool<char>? bufferPool)
        {
            if (_buffer != null)
            {
                BufferUtils.ReturnBuffer(bufferPool, _buffer);
                _buffer = null;
            }
            _position = 0;
        }

        private void EnsureSize(IArrayPool<char>? bufferPool, int appendLength)
        {
            char[] newBuffer = BufferUtils.RentBuffer(bufferPool, (_position + appendLength) * 2);

            if (_buffer != null)
            {
                Array.Copy(_buffer, newBuffer, _position);
                BufferUtils.ReturnBuffer(bufferPool, _buffer);
            }

            _buffer = newBuffer;
        }

        public override string ToString()
        {
            return ToString(0, _position);
        }

        public string ToString(int start, int length)
        {
            // TODO: validation
            return new string(_buffer, start, length);
        }

        public char[]? InternalBuffer => _buffer;
    }
}