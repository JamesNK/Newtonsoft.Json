﻿#region License
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
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Represents a writer that provides a fast, non-cached, forward-only way of generating Json data.
  /// </summary>
  public class JsonTextWriter : JsonWriter
  {
    private readonly TextWriter _writer;
    private Base64Encoder _base64Encoder;
    private char _indentChar;
    private int _indentation;
    private char _quoteChar;
    private QuoteNameHandling _quoteNameHandling;
    private bool[] _charEscapeFlags;

    private Base64Encoder Base64Encoder
    {
      get
      {
        if (_base64Encoder == null)
          _base64Encoder = new Base64Encoder(_writer);

        return _base64Encoder;
      }
    }

    /// <summary>
    /// Gets or sets how many IndentChars to write for each level in the hierarchy when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
    /// </summary>
    public int Indentation
    {
      get { return _indentation; }
      set
      {
        if (value < 0)
          throw new ArgumentException("Indentation value must be greater than 0.");

        _indentation = value;
      }
    }

    /// <summary>
    /// Gets or sets which character to use to quote attribute values.
    /// </summary>
    public char QuoteChar
    {
      get { return _quoteChar; }
      set
      {
        if (value != '"' && value != '\'')
          throw new ArgumentException(@"Invalid JavaScript string quote character. Valid quote characters are ' and "".");

        _quoteChar = value;
        UpdateCharEscapeFlags();
      }
    }

    /// <summary>
    /// Gets or sets which character to use for indenting when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
    /// </summary>
    public char IndentChar
    {
      get { return _indentChar; }
      set { _indentChar = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether object names will be surrounded with quotes.
    /// </summary>
    [Obsolete("This property is obsolete and has been replaced by QuoteNameHandling property.")]
    public bool QuoteName
    {
      get { return _quoteNameHandling != QuoteNameHandling.Unquoted; }
      set { _quoteNameHandling = value ? QuoteNameHandling.Quoted : QuoteNameHandling.Unquoted; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether object names will be surrounded with quotes.
    /// </summary>
    public QuoteNameHandling QuoteNameHandling
    {
      get { return _quoteNameHandling; }
      set { _quoteNameHandling = value; }
    }

    /// <summary>
    /// Creates an instance of the <c>JsonWriter</c> class using the specified <see cref="TextWriter"/>. 
    /// </summary>
    /// <param name="textWriter">The <c>TextWriter</c> to write to.</param>
    public JsonTextWriter(TextWriter textWriter)
    {
      if (textWriter == null)
        throw new ArgumentNullException("textWriter");

      _writer = textWriter;
      _quoteChar = '"';
      _quoteNameHandling = QuoteNameHandling.Quoted;
      _indentChar = ' ';
      _indentation = 2;

      UpdateCharEscapeFlags();
    }

    /// <summary>
    /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
    /// </summary>
    public override void Flush()
    {
      _writer.Flush();
    }

    /// <summary>
    /// Closes this stream and the underlying stream.
    /// </summary>
    public override void Close()
    {
      base.Close();

      if (CloseOutput && _writer != null)
#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
        _writer.Close();
#else
        _writer.Dispose();
#endif
    }

    /// <summary>
    /// Writes the beginning of a Json object.
    /// </summary>
    public override void WriteStartObject()
    {
      InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

      _writer.Write("{");
    }

    /// <summary>
    /// Writes the beginning of a Json array.
    /// </summary>
    public override void WriteStartArray()
    {
      InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

      _writer.Write("[");
    }

    /// <summary>
    /// Writes the start of a constructor with the given name.
    /// </summary>
    /// <param name="name">The name of the constructor.</param>
    public override void WriteStartConstructor(string name)
    {
      InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);

      _writer.Write("new ");
      _writer.Write(name);
      _writer.Write("(");
    }

    /// <summary>
    /// Writes the specified end token.
    /// </summary>
    /// <param name="token">The end token to write.</param>
    protected override void WriteEnd(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.EndObject:
          _writer.Write("}");
          break;
        case JsonToken.EndArray:
          _writer.Write("]");
          break;
        case JsonToken.EndConstructor:
          _writer.Write(")");
          break;
        default:
          throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
      }
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a Json object.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public override void WritePropertyName(string name)
    {
      InternalWritePropertyName(name);

      JavaScriptUtils.WriteEscapedJavaScriptString(_writer, name, _quoteChar, _quoteNameHandling, _charEscapeFlags, StringEscapeHandling);

      _writer.Write(':');
    }

    /// <summary>
    /// Writes the property name of a name/value pair on a JSON object.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    public override void WritePropertyName(string name, bool escape)
    {
      InternalWritePropertyName(name);

      if (escape)
      {
        JavaScriptUtils.WriteEscapedJavaScriptString(_writer, name, _quoteChar, _quoteNameHandling, _charEscapeFlags, StringEscapeHandling);
      }
      else
      {
        bool quoteName = _quoteNameHandling == QuoteNameHandling.Quoted
          || _quoteNameHandling == QuoteNameHandling.Auto && !JavaScriptUtils.IsValidIdentifier(name);
        if (quoteName)
          _writer.Write(_quoteChar);

        _writer.Write(name);

        if (quoteName)
          _writer.Write(_quoteChar);
      }

      _writer.Write(':');
    }

    internal override void OnStringEscapeHandlingChanged()
    {
      UpdateCharEscapeFlags();
    }

    private void UpdateCharEscapeFlags()
    {
      if (StringEscapeHandling == StringEscapeHandling.EscapeHtml)
        _charEscapeFlags = JavaScriptUtils.HtmlCharEscapeFlags;
      else if (_quoteChar == '"')
        _charEscapeFlags = JavaScriptUtils.DoubleQuoteCharEscapeFlags;
      else
        _charEscapeFlags = JavaScriptUtils.SingleQuoteCharEscapeFlags;
    }

    /// <summary>
    /// Writes indent characters.
    /// </summary>
    protected override void WriteIndent()
    {
      _writer.Write(Environment.NewLine);

      // levels of indentation multiplied by the indent count
      int currentIndentCount = Top*_indentation;

      while (currentIndentCount > 0)
      {
        // write up to a max of 10 characters at once to avoid creating too many new strings
        int writeCount = Math.Min(currentIndentCount, 10);

        _writer.Write(new string(_indentChar, writeCount));

        currentIndentCount -= writeCount;
      }
    }

    /// <summary>
    /// Writes the JSON value delimiter.
    /// </summary>
    protected override void WriteValueDelimiter()
    {
      _writer.Write(',');
    }

    /// <summary>
    /// Writes an indent space.
    /// </summary>
    protected override void WriteIndentSpace()
    {
      _writer.Write(' ');
    }

    private void WriteValueInternal(string value, JsonToken token)
    {
      _writer.Write(value);
    }

    #region WriteValue methods
    /// <summary>
    /// Writes a null value.
    /// </summary>
    public override void WriteNull()
    {
      InternalWriteNull();
      WriteValueInternal(JsonConvert.Null, JsonToken.Null);
    }

    /// <summary>
    /// Writes an undefined value.
    /// </summary>
    public override void WriteUndefined()
    {
      InternalWriteUndefined();
      WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
    }

    /// <summary>
    /// Writes raw JSON.
    /// </summary>
    /// <param name="json">The raw JSON to write.</param>
    public override void WriteRaw(string json)
    {
      InternalWriteRaw();

      _writer.Write(json);
    }

    /// <summary>
    /// Writes a <see cref="String"/> value.
    /// </summary>
    /// <param name="value">The <see cref="String"/> value to write.</param>
    public override void WriteValue(string value)
    {
      InternalWriteValue(JsonToken.String);

      if (value == null)
        WriteValueInternal(JsonConvert.Null, JsonToken.Null);
      else
        JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, QuoteNameHandling.Quoted, _charEscapeFlags, StringEscapeHandling);
    }

    /// <summary>
    /// Writes a <see cref="Int32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int32"/> value to write.</param>
    public override void WriteValue(int value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt32"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt32"/> value to write.</param>
    [CLSCompliant(false)]
    public override void WriteValue(uint value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int64"/> value to write.</param>
    public override void WriteValue(long value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt64"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt64"/> value to write.</param>
    [CLSCompliant(false)]
    public override void WriteValue(ulong value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Single"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Single"/> value to write.</param>
    public override void WriteValue(float value)
    {
      InternalWriteValue(JsonToken.Float);
      WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Single}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Single}"/> value to write.</param>
    public override void WriteValue(float? value)
    {
      if (value == null)
      {
        WriteNull();
      }
      else
      {
        InternalWriteValue(JsonToken.Float);
        WriteValueInternal(JsonConvert.ToString(value.Value, FloatFormatHandling, QuoteChar, true), JsonToken.Float);
      }
    }

    /// <summary>
    /// Writes a <see cref="Double"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Double"/> value to write.</param>
    public override void WriteValue(double value)
    {
      InternalWriteValue(JsonToken.Float);
      WriteValueInternal(JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="Nullable{Double}"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{Double}"/> value to write.</param>
    public override void WriteValue(double? value)
    {
      if (value == null)
      {
        WriteNull();
      }
      else
      {
        InternalWriteValue(JsonToken.Float);
        WriteValueInternal(JsonConvert.ToString(value.Value, FloatFormatHandling, QuoteChar, true), JsonToken.Float);
      }
    }

    /// <summary>
    /// Writes a <see cref="Boolean"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Boolean"/> value to write.</param>
    public override void WriteValue(bool value)
    {
      InternalWriteValue(JsonToken.Boolean);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
    }

    /// <summary>
    /// Writes a <see cref="Int16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Int16"/> value to write.</param>
    public override void WriteValue(short value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="UInt16"/> value.
    /// </summary>
    /// <param name="value">The <see cref="UInt16"/> value to write.</param>
    [CLSCompliant(false)]
    public override void WriteValue(ushort value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Char"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Char"/> value to write.</param>
    public override void WriteValue(char value)
    {
      InternalWriteValue(JsonToken.String);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Byte"/> value to write.</param>
    public override void WriteValue(byte value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="SByte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="SByte"/> value to write.</param>
    [CLSCompliant(false)]
    public override void WriteValue(sbyte value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Integer);
    }

    /// <summary>
    /// Writes a <see cref="Decimal"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Decimal"/> value to write.</param>
    public override void WriteValue(decimal value)
    {
      InternalWriteValue(JsonToken.Float);
      WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    public override void WriteValue(DateTime value)
    {
      InternalWriteValue(JsonToken.Date);
      value = JsonConvert.EnsureDateTime(value, DateTimeZoneHandling);
      _writer.Write(_quoteChar);
      JsonConvert.WriteDateTimeString(_writer, value, DateFormatHandling, DateFormatString, Culture);
      _writer.Write(_quoteChar);
    }

    /// <summary>
    /// Writes a <see cref="T:Byte[]"/> value.
    /// </summary>
    /// <param name="value">The <see cref="T:Byte[]"/> value to write.</param>
    public override void WriteValue(byte[] value)
    {
      if (value == null)
      {
        WriteNull();
      }
      else
      {
        InternalWriteValue(JsonToken.Bytes);
        _writer.Write(_quoteChar);
        Base64Encoder.Encode(value, 0, value.Length);
        Base64Encoder.Flush();
        _writer.Write(_quoteChar);
      }
    }

#if !NET20
    /// <summary>
    /// Writes a <see cref="DateTimeOffset"/> value.
    /// </summary>
    /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
    public override void WriteValue(DateTimeOffset value)
    {
      InternalWriteValue(JsonToken.Date);
      _writer.Write(_quoteChar);
      JsonConvert.WriteDateTimeOffsetString(_writer, value, DateFormatHandling, DateFormatString, Culture);
      _writer.Write(_quoteChar);
    }
#endif

    /// <summary>
    /// Writes a <see cref="Guid"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> value to write.</param>
    public override void WriteValue(Guid value)
    {
      InternalWriteValue(JsonToken.String);
      WriteValueInternal(JsonConvert.ToString(value, _quoteChar), JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="TimeSpan"/> value.
    /// </summary>
    /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
    public override void WriteValue(TimeSpan value)
    {
      InternalWriteValue(JsonToken.String);
      WriteValueInternal(JsonConvert.ToString(value, _quoteChar), JsonToken.String);
    }

    /// <summary>
    /// Writes a <see cref="Uri"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Uri"/> value to write.</param>
    public override void WriteValue(Uri value)
    {
      if (value == null)
      {
        WriteNull();
      }
      else
      {
        InternalWriteValue(JsonToken.String);
        WriteValueInternal(JsonConvert.ToString(value, _quoteChar), JsonToken.String);
      }
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40 || PORTABLE)
    /// <summary>
    /// Writes a <see cref="BigInteger"/> value.
    /// </summary>
    /// <param name="value">The <see cref="BigInteger"/> value to write.</param>
    public override void WriteValue(BigInteger value)
    {
      InternalWriteValue(JsonToken.Integer);
      WriteValueInternal(value.ToString(CultureInfo.InvariantCulture), JsonToken.String);
    }
#endif
    #endregion

    /// <summary>
    /// Writes out a comment <code>/*...*/</code> containing the specified text. 
    /// </summary>
    /// <param name="text">Text to place inside the comment.</param>
    public override void WriteComment(string text)
    {
      InternalWriteComment();

      _writer.Write("/*");
      _writer.Write(text);
      _writer.Write("*/");
    }

    /// <summary>
    /// Writes out the given white space.
    /// </summary>
    /// <param name="ws">The string of white space characters.</param>
    public override void WriteWhitespace(string ws)
    {
      InternalWriteWhitespace(ws);

      _writer.Write(ws);
    }
  }
}