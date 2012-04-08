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
  /// Builds a string. Unlike StringBuilder this class lets you reuse it's internal buffer.
  /// </summary>
  internal class StringBuffer
  {
    private char[] _buffer;
    private int _position;

    private static readonly char[] EmptyBuffer = new char[0];

    public int Position
    {
      get { return _position; }
      set { _position = value; }
    }

    public StringBuffer()
    {
      _buffer = EmptyBuffer;
    }

    public StringBuffer(int initalSize)
    {
      _buffer = new char[initalSize];
    }

    public void Append(char value)
    {
      // test if the buffer array is large enough to take the value
      if (_position == _buffer.Length)
        EnsureSize(1);

      // set value and increment poisition
      _buffer[_position++] = value;
    }

    public void Append(char[] buffer, int startIndex, int count)
    {
      if (_position + count >= _buffer.Length)
        EnsureSize(count);

      Array.Copy(buffer, startIndex, _buffer, _position, count);

      _position += count;
    }

    public void Clear()
    {
      _buffer = EmptyBuffer;
      _position = 0;
    }

    private void EnsureSize(int appendLength)
    {
      char[] newBuffer = new char[(_position + appendLength) * 2];

      Array.Copy(_buffer, newBuffer, _position);

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

    public char[] GetInternalBuffer()
    {
      return _buffer;
    }
  }
}