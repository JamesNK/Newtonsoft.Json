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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
#if !PORTABLE || NETSTANDARD1_1
using System.Numerics;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Serialization
{
    internal partial class TraceJsonWriter
    {
        public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }
            return t;
        }

        public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(byte[] value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value != null)
            {
                base.WriteValue(value);
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
#if !PORTABLE || NETSTANDARD1_1
            if (value is BigInteger)
            {
                InternalWriteValue(JsonToken.Integer);
            }
            else
#endif
            {
                if (value != null)
                {
                    base.WriteValue(value);
                }
                else
                {
                    base.WriteNull();
                }
            }

            return t;
        }

        public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }
            return t;
        }

        public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value != null)
            {
                base.WriteValue(value);
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(Uri value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value != null)
            {
                base.WriteValue(value);
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            base.WriteValue(value);
            return t;
        }

        public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteValueAsync(value, cancellationToken);
            _textWriter.WriteValue(value);
            if (value.HasValue)
            {
                base.WriteValue(value.GetValueOrDefault());
            }
            else
            {
                base.WriteNull();
            }

            return t;
        }

        public override Task WriteUndefinedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteUndefinedAsync(cancellationToken);
            _textWriter.WriteUndefined();
            base.WriteUndefined();
            return t;
        }

        public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteWhitespaceAsync(ws, cancellationToken);
            _textWriter.WriteWhitespace(ws);
            base.WriteWhitespace(ws);
            return t;
        }

        public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.CloseAsync(cancellationToken);
            _textWriter.Close();
            base.Close();
            return t;
        }

        public override Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.FlushAsync(cancellationToken);
            _textWriter.Flush();
            return t;
        }

        public override Task WriteRawAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteRawAsync(json, cancellationToken);
            _textWriter.WriteRaw(json);
            base.WriteRaw(json);
            return t;
        }

        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteEndArrayAsync(cancellationToken);
            _textWriter.WriteEndArray();
            base.WriteEndArray();
            return t;
        }

        public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteEndConstructorAsync(cancellationToken);
            _textWriter.WriteEndConstructor();
            base.WriteEndConstructor();
            return t;
        }

        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteEndObjectAsync(cancellationToken);
            _textWriter.WriteEndObject();
            base.WriteEndObject();
            return t;
        }

        public override Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteNullAsync(cancellationToken);
            _textWriter.WriteNull();
            base.WriteUndefined();
            return t;
        }

        public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WritePropertyNameAsync(name, cancellationToken);
            _textWriter.WritePropertyName(name);
            base.WritePropertyName(name);
            return t;
        }

        public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WritePropertyNameAsync(name, escape, cancellationToken);
            _textWriter.WritePropertyName(name, escape);

            // method with escape will error
            base.WritePropertyName(name);
            return t;
        }

        public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteStartArrayAsync(cancellationToken);
            _textWriter.WriteStartArray();
            base.WriteStartArray();
            return t;
        }

        public override Task WriteCommentAsync(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteCommentAsync(text, cancellationToken);
            _textWriter.WriteComment(text);
            base.WriteComment(text);
            return t;
        }

        public override Task WriteRawValueAsync(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteRawValueAsync(json, cancellationToken);
            _textWriter.WriteRawValue(json);

            // calling base method will write json twice
            InternalWriteValue(JsonToken.Undefined);
            return t;
        }

        public override Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteStartConstructorAsync(name, cancellationToken);
            _textWriter.WriteStartConstructor(name);
            base.WriteStartConstructor(name);
            return t;
        }

        public override Task WriteStartObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task t = _innerWriter.WriteStartObjectAsync(cancellationToken);
            _textWriter.WriteStartObject();
            base.WriteStartObject();
            return t;
        }
    }
}

#endif