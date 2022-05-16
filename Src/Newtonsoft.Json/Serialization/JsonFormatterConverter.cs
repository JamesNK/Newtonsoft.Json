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

#if HAVE_BINARY_SERIALIZATION
using System;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Serialization
{
    internal class JsonFormatterConverter : IFormatterConverter
    {
        private readonly JsonSerializerInternalReader _reader;
        private readonly JsonISerializableContract _contract;
        private readonly JsonProperty? _member;

        public JsonFormatterConverter(JsonSerializerInternalReader reader, JsonISerializableContract contract, JsonProperty? member)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(contract, nameof(contract));

            _reader = reader;
            _contract = contract;
            _member = member;
        }

        private T GetTokenValue<T>(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            JValue v = (JValue)value;
            return (T)System.Convert.ChangeType(v.Value, typeof(T), CultureInfo.InvariantCulture)!;
        }

        public object Convert(object value, Type type)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (!(value is JToken token))
            {
                throw new ArgumentException("Value is not a JToken.", nameof(value));
            }

            return _reader.CreateISerializableItem(token, type, _contract, _member)!;
        }

        public object Convert(object value, TypeCode typeCode)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            object? resolvedValue = (value is JValue v) ? v.Value : value;

            return System.Convert.ChangeType(resolvedValue, typeCode, CultureInfo.InvariantCulture)!;
        }

        public bool ToBoolean(object value)
        {
            return GetTokenValue<bool>(value);
        }

        public byte ToByte(object value)
        {
            return GetTokenValue<byte>(value);
        }

        public char ToChar(object value)
        {
            return GetTokenValue<char>(value);
        }

        public DateTime ToDateTime(object value)
        {
            return GetTokenValue<DateTime>(value);
        }

        public decimal ToDecimal(object value)
        {
            return GetTokenValue<decimal>(value);
        }

        public double ToDouble(object value)
        {
            return GetTokenValue<double>(value);
        }

        public short ToInt16(object value)
        {
            return GetTokenValue<short>(value);
        }

        public int ToInt32(object value)
        {
            return GetTokenValue<int>(value);
        }

        public long ToInt64(object value)
        {
            return GetTokenValue<long>(value);
        }

        public sbyte ToSByte(object value)
        {
            return GetTokenValue<sbyte>(value);
        }

        public float ToSingle(object value)
        {
            return GetTokenValue<float>(value);
        }

        public string ToString(object value)
        {
            return GetTokenValue<string>(value);
        }

        public ushort ToUInt16(object value)
        {
            return GetTokenValue<ushort>(value);
        }

        public uint ToUInt32(object value)
        {
            return GetTokenValue<uint>(value);
        }

        public ulong ToUInt64(object value)
        {
            return GetTokenValue<ulong>(value);
        }
    }
}

#endif