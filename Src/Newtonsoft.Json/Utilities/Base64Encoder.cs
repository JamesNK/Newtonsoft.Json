using System;
using System.IO;

namespace Newtonsoft.Json.Utilities
{
  internal class Base64Encoder
  {
    private const int Base64LineSize = 76;
    private const int LineSizeInBytes = 57;

    private readonly char[] _charsLine = new char[Base64LineSize];
    private readonly TextWriter _writer;

    private byte[] _leftOverBytes;
    private int _leftOverBytesCount;

    public Base64Encoder(TextWriter writer)
    {
      ValidationUtils.ArgumentNotNull(writer, "writer");
      _writer = writer;
    }

    public void Encode(byte[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      if (index < 0)
        throw new ArgumentOutOfRangeException("index");

      if (count < 0)
        throw new ArgumentOutOfRangeException("count");

      if (count > (buffer.Length - index))
        throw new ArgumentOutOfRangeException("count");

      if (_leftOverBytesCount > 0)
      {
        int leftOverBytesCount = _leftOverBytesCount;
        while (leftOverBytesCount < 3 && count > 0)
        {
          _leftOverBytes[leftOverBytesCount++] = buffer[index++];
          count--;
        }
        if (count == 0 && leftOverBytesCount < 3)
        {
          _leftOverBytesCount = leftOverBytesCount;
          return;
        }
        int num2 = Convert.ToBase64CharArray(_leftOverBytes, 0, 3, _charsLine, 0);
        WriteChars(_charsLine, 0, num2);
      }
      _leftOverBytesCount = count % 3;
      if (_leftOverBytesCount > 0)
      {
        count -= _leftOverBytesCount;
        if (_leftOverBytes == null)
        {
          _leftOverBytes = new byte[3];
        }
        for (int i = 0; i < _leftOverBytesCount; i++)
        {
          _leftOverBytes[i] = buffer[(index + count) + i];
        }
      }
      int num4 = index + count;
      int length = LineSizeInBytes;
      while (index < num4)
      {
        if ((index + length) > num4)
        {
          length = num4 - index;
        }
        int num6 = Convert.ToBase64CharArray(buffer, index, length, _charsLine, 0);
        WriteChars(_charsLine, 0, num6);
        index += length;
      }
    }

    public void Flush()
    {
      if (_leftOverBytesCount > 0)
      {
        int count = Convert.ToBase64CharArray(_leftOverBytes, 0, _leftOverBytesCount, _charsLine, 0);
        WriteChars(_charsLine, 0, count);
        _leftOverBytesCount = 0;
      }
    }

    private void WriteChars(char[] chars, int index, int count)
    {
      _writer.Write(chars, index, count);
    }
  }
}