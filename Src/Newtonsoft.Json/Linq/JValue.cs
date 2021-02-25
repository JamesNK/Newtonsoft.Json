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
using System.Diagnostics;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
#if HAVE_DYNAMIC
using System.Dynamic;
using System.Linq.Expressions;
#endif
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a value in JSON (string, integer, date, etc).
    /// </summary>
    public partial class JValue : JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>
#if HAVE_ICONVERTIBLE
        , IConvertible
#endif
    {
        private JTokenType _valueType;
        private object? _value;

        internal JValue(object? value, JTokenType type)
        {
            _value = value;
            _valueType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class from another <see cref="JValue"/> object.
        /// </summary>
        /// <param name="other">A <see cref="JValue"/> object to copy from.</param>
        public JValue(JValue other)
            : this(other.Value, other.Type)
        {
            CopyAnnotations(this, other);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(long value)
            : this(value, JTokenType.Integer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(decimal value)
            : this(value, JTokenType.Float)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(char value)
            : this(value, JTokenType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        [CLSCompliant(false)]
        public JValue(ulong value)
            : this(value, JTokenType.Integer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(double value)
            : this(value, JTokenType.Float)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(float value)
            : this(value, JTokenType.Float)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(DateTime value)
            : this(value, JTokenType.Date)
        {
        }

#if HAVE_DATE_TIME_OFFSET
        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(DateTimeOffset value)
            : this(value, JTokenType.Date)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(bool value)
            : this(value, JTokenType.Boolean)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(string? value)
            : this(value, JTokenType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(Guid value)
            : this(value, JTokenType.Guid)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(Uri? value)
            : this(value, (value != null) ? JTokenType.Uri : JTokenType.Null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(TimeSpan value)
            : this(value, JTokenType.TimeSpan)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public JValue(object? value)
            : this(value, GetValueType(null, value))
        {
        }

        internal override bool DeepEquals(JToken node)
        {
            if (!(node is JValue other))
            {
                return false;
            }
            if (other == this)
            {
                return true;
            }

            return ValuesEquals(this, other);
        }

        /// <summary>
        /// Gets a value indicating whether this token has child tokens.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
        /// </value>
        public override bool HasValues => false;

#if HAVE_BIG_INTEGER
        private static int CompareBigInteger(BigInteger i1, object i2)
        {
            int result = i1.CompareTo(ConvertUtils.ToBigInteger(i2));

            if (result != 0)
            {
                return result;
            }

            // converting a fractional number to a BigInteger will lose the fraction
            // check for fraction if result is two numbers are equal
            if (i2 is decimal d1)
            {
                return (0m).CompareTo(Math.Abs(d1 - Math.Truncate(d1)));
            }
            else if (i2 is double || i2 is float)
            {
                double d = Convert.ToDouble(i2, CultureInfo.InvariantCulture);
                return (0d).CompareTo(Math.Abs(d - Math.Truncate(d)));
            }

            return result;
        }
#endif

        internal static int Compare(JTokenType valueType, object? objA, object? objB)
        {
            if (objA == objB)
            {
                return 0;
            }
            if (objB == null)
            {
                return 1;
            }
            if (objA == null)
            {
                return -1;
            }

            switch (valueType)
            {
                case JTokenType.Integer:
                {
#if HAVE_BIG_INTEGER
                    if (objA is BigInteger integerA)
                    {
                        return CompareBigInteger(integerA, objB);
                    }
                    if (objB is BigInteger integerB)
                    {
                            return -CompareBigInteger(integerB, objA);
                        }
#endif
                    if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                    {
                        return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
                    }
                    else if (objA is float || objB is float || objA is double || objB is double)
                    {
                        return CompareFloat(objA, objB);
                    }
                    else
                    {
                        return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
                    }
                }
                case JTokenType.Float:
                {
#if HAVE_BIG_INTEGER
                    if (objA is BigInteger integerA)
                    {
                        return CompareBigInteger(integerA, objB);
                    }
                    if (objB is BigInteger integerB)
                    {
                        return -CompareBigInteger(integerB, objA);
                    }
#endif
                    if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                    {
                        return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
                    }
                    return CompareFloat(objA, objB);
                    }
                case JTokenType.Comment:
                case JTokenType.String:
                case JTokenType.Raw:
                    string s1 = Convert.ToString(objA, CultureInfo.InvariantCulture);
                    string s2 = Convert.ToString(objB, CultureInfo.InvariantCulture);

                    return string.CompareOrdinal(s1, s2);
                case JTokenType.Boolean:
                    bool b1 = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
                    bool b2 = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);

                    return b1.CompareTo(b2);
                case JTokenType.Date:
#if HAVE_DATE_TIME_OFFSET
                    if (objA is DateTime dateA)
                    {
#else
                        DateTime dateA = (DateTime)objA;
#endif
                        DateTime dateB;

#if HAVE_DATE_TIME_OFFSET
                        if (objB is DateTimeOffset offsetB)
                        {
                            dateB = offsetB.DateTime;
                        }
                        else
#endif
                        {
                            dateB = Convert.ToDateTime(objB, CultureInfo.InvariantCulture);
                        }

                        return dateA.CompareTo(dateB);
#if HAVE_DATE_TIME_OFFSET
                    }
                    else
                    {
                        DateTimeOffset offsetA = (DateTimeOffset)objA;
                        if (!(objB is DateTimeOffset offsetB))
                        {
                            offsetB = new DateTimeOffset(Convert.ToDateTime(objB, CultureInfo.InvariantCulture));
                        }

                        return offsetA.CompareTo(offsetB);
                    }
#endif
                case JTokenType.Bytes:
                    if (!(objB is byte[] bytesB))
                    {
                        throw new ArgumentException("Object must be of type byte[].");
                    }

                    byte[]? bytesA = objA as byte[];
                    MiscellaneousUtils.Assert(bytesA != null);

                    return MiscellaneousUtils.ByteArrayCompare(bytesA!, bytesB);
                case JTokenType.Guid:
                    if (!(objB is Guid))
                    {
                        throw new ArgumentException("Object must be of type Guid.");
                    }

                    Guid guid1 = (Guid)objA;
                    Guid guid2 = (Guid)objB;

                    return guid1.CompareTo(guid2);
                case JTokenType.Uri:
                    Uri? uri2 = objB as Uri;
                    if (uri2 == null)
                    {
                        throw new ArgumentException("Object must be of type Uri.");
                    }

                    Uri uri1 = (Uri)objA;

                    return Comparer<string>.Default.Compare(uri1.ToString(), uri2.ToString());
                case JTokenType.TimeSpan:
                    if (!(objB is TimeSpan))
                    {
                        throw new ArgumentException("Object must be of type TimeSpan.");
                    }

                    TimeSpan ts1 = (TimeSpan)objA;
                    TimeSpan ts2 = (TimeSpan)objB;

                    return ts1.CompareTo(ts2);
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(valueType), valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
            }
        }

        private static int CompareFloat(object objA, object objB)
        {
            double d1 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
            double d2 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);

            // take into account possible floating point errors
            if (MathUtils.ApproxEquals(d1, d2))
            {
                return 0;
            }

            return d1.CompareTo(d2);
        }

#if HAVE_EXPRESSIONS
        private static bool Operation(ExpressionType operation, object? objA, object? objB, out object? result)
        {
            if (objA is string || objB is string)
            {
                if (operation == ExpressionType.Add || operation == ExpressionType.AddAssign)
                {
                    result = objA?.ToString() + objB?.ToString();
                    return true;
                }
            }

#if HAVE_BIG_INTEGER
            if (objA is BigInteger || objB is BigInteger)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                // not that this will lose the fraction
                // BigInteger doesn't have operators with non-integer types
                BigInteger i1 = ConvertUtils.ToBigInteger(objA);
                BigInteger i2 = ConvertUtils.ToBigInteger(objB);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = i1 + i2;
                        return true;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = i1 - i2;
                        return true;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = i1 * i2;
                        return true;
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = i1 / i2;
                        return true;
                }
            }
            else
#endif
                if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                {
                    if (objA == null || objB == null)
                    {
                        result = null;
                        return true;
                    }

                    decimal d1 = Convert.ToDecimal(objA, CultureInfo.InvariantCulture);
                    decimal d2 = Convert.ToDecimal(objB, CultureInfo.InvariantCulture);

                    switch (operation)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddAssign:
                            result = d1 + d2;
                            return true;
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractAssign:
                            result = d1 - d2;
                            return true;
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyAssign:
                            result = d1 * d2;
                            return true;
                        case ExpressionType.Divide:
                        case ExpressionType.DivideAssign:
                            result = d1 / d2;
                            return true;
                    }
                }
                else if (objA is float || objB is float || objA is double || objB is double)
                {
                    if (objA == null || objB == null)
                    {
                        result = null;
                        return true;
                    }

                    double d1 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
                    double d2 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);

                    switch (operation)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddAssign:
                            result = d1 + d2;
                            return true;
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractAssign:
                            result = d1 - d2;
                            return true;
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyAssign:
                            result = d1 * d2;
                            return true;
                        case ExpressionType.Divide:
                        case ExpressionType.DivideAssign:
                            result = d1 / d2;
                            return true;
                    }
                }
                else if (objA is int || objA is uint || objA is long || objA is short || objA is ushort || objA is sbyte || objA is byte ||
                         objB is int || objB is uint || objB is long || objB is short || objB is ushort || objB is sbyte || objB is byte)
                {
                    if (objA == null || objB == null)
                    {
                        result = null;
                        return true;
                    }

                    long l1 = Convert.ToInt64(objA, CultureInfo.InvariantCulture);
                    long l2 = Convert.ToInt64(objB, CultureInfo.InvariantCulture);

                    switch (operation)
                    {
                        case ExpressionType.Add:
                        case ExpressionType.AddAssign:
                            result = l1 + l2;
                            return true;
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractAssign:
                            result = l1 - l2;
                            return true;
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyAssign:
                            result = l1 * l2;
                            return true;
                        case ExpressionType.Divide:
                        case ExpressionType.DivideAssign:
                            result = l1 / l2;
                            return true;
                    }
                }

            result = null;
            return false;
        }
#endif

        internal override JToken CloneToken()
        {
            return new JValue(this);
        }

        /// <summary>
        /// Creates a <see cref="JValue"/> comment with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="JValue"/> comment with the given value.</returns>
        public static JValue CreateComment(string? value)
        {
            return new JValue(value, JTokenType.Comment);
        }

        /// <summary>
        /// Creates a <see cref="JValue"/> string with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="JValue"/> string with the given value.</returns>
        public static JValue CreateString(string? value)
        {
            return new JValue(value, JTokenType.String);
        }

        /// <summary>
        /// Creates a <see cref="JValue"/> null value.
        /// </summary>
        /// <returns>A <see cref="JValue"/> null value.</returns>
        public static JValue CreateNull()
        {
            return new JValue(null, JTokenType.Null);
        }

        /// <summary>
        /// Creates a <see cref="JValue"/> undefined value.
        /// </summary>
        /// <returns>A <see cref="JValue"/> undefined value.</returns>
        public static JValue CreateUndefined()
        {
            return new JValue(null, JTokenType.Undefined);
        }

        private static JTokenType GetValueType(JTokenType? current, object? value)
        {
            if (value == null)
            {
                return JTokenType.Null;
            }
#if HAVE_ADO_NET
            else if (value == DBNull.Value)
            {
                return JTokenType.Null;
            }
#endif
            else if (value is string)
            {
                return GetStringValueType(current);
            }
            else if (value is long || value is int || value is short || value is sbyte
                     || value is ulong || value is uint || value is ushort || value is byte)
            {
                return JTokenType.Integer;
            }
            else if (value is Enum)
            {
                return JTokenType.Integer;
            }
#if HAVE_BIG_INTEGER
            else if (value is BigInteger)
            {
                return JTokenType.Integer;
            }
#endif
            else if (value is double || value is float || value is decimal)
            {
                return JTokenType.Float;
            }
            else if (value is DateTime)
            {
                return JTokenType.Date;
            }
#if HAVE_DATE_TIME_OFFSET
            else if (value is DateTimeOffset)
            {
                return JTokenType.Date;
            }
#endif
            else if (value is byte[])
            {
                return JTokenType.Bytes;
            }
            else if (value is bool)
            {
                return JTokenType.Boolean;
            }
            else if (value is Guid)
            {
                return JTokenType.Guid;
            }
            else if (value is Uri)
            {
                return JTokenType.Uri;
            }
            else if (value is TimeSpan)
            {
                return JTokenType.TimeSpan;
            }

            throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

        private static JTokenType GetStringValueType(JTokenType? current)
        {
            if (current == null)
            {
                return JTokenType.String;
            }

            switch (current.GetValueOrDefault())
            {
                case JTokenType.Comment:
                case JTokenType.String:
                case JTokenType.Raw:
                    return current.GetValueOrDefault();
                default:
                    return JTokenType.String;
            }
        }

        /// <summary>
        /// Gets the node type for this <see cref="JToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override JTokenType Type => _valueType;

        /// <summary>
        /// Gets or sets the underlying token value.
        /// </summary>
        /// <value>The underlying token value.</value>
        public object? Value
        {
            get => _value;
            set
            {
                Type? currentType = _value?.GetType();
                Type? newType = value?.GetType();

                if (currentType != newType)
                {
                    _valueType = GetValueType(_valueType, value);
                }

                _value = value;
            }
        }

        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/>s which will be used when writing the token.</param>
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            if (converters != null && converters.Length > 0 && _value != null)
            {
                JsonConverter? matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value.GetType());
                if (matchingConverter != null && matchingConverter.CanWrite)
                {
                    matchingConverter.WriteJson(writer, _value, JsonSerializer.CreateDefault());
                    return;
                }
            }

            switch (_valueType)
            {
                case JTokenType.Comment:
                    writer.WriteComment(_value?.ToString());
                    return;
                case JTokenType.Raw:
                    writer.WriteRawValue(_value?.ToString());
                    return;
                case JTokenType.Null:
                    writer.WriteNull();
                    return;
                case JTokenType.Undefined:
                    writer.WriteUndefined();
                    return;
                case JTokenType.Integer:
                    if (_value is int i)
                    {
                        writer.WriteValue(i);
                    }
                    else if (_value is long l)
                    {
                        writer.WriteValue(l);
                    }
                    else if (_value is ulong ul)
                    {
                        writer.WriteValue(ul);
                    }
#if HAVE_BIG_INTEGER
                    else if (_value is BigInteger integer)
                    {
                        writer.WriteValue(integer);
                    }
#endif
                    else
                    {
                        writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
                    }
                    return;
                case JTokenType.Float:
                    if (_value is decimal dec)
                    {
                        writer.WriteValue(dec);
                    }
                    else if (_value is double d)
                    {
                        writer.WriteValue(d);
                    }
                    else if (_value is float f)
                    {
                        writer.WriteValue(f);
                    }
                    else
                    {
                        writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
                    }
                    return;
                case JTokenType.String:
                    writer.WriteValue(_value?.ToString());
                    return;
                case JTokenType.Boolean:
                    writer.WriteValue(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
                    return;
                case JTokenType.Date:
#if HAVE_DATE_TIME_OFFSET
                    if (_value is DateTimeOffset offset)
                    {
                        writer.WriteValue(offset);
                    }
                    else
#endif
                    {
                        writer.WriteValue(Convert.ToDateTime(_value, CultureInfo.InvariantCulture));
                    }
                    return;
                case JTokenType.Bytes:
                    writer.WriteValue((byte[]?)_value);
                    return;
                case JTokenType.Guid:
                    writer.WriteValue((_value != null) ? (Guid?)_value : null);
                    return;
                case JTokenType.TimeSpan:
                    writer.WriteValue((_value != null) ? (TimeSpan?)_value : null);
                    return;
                case JTokenType.Uri:
                    writer.WriteValue((Uri?)_value);
                    return;
            }

            throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), _valueType, "Unexpected token type.");
        }

        internal override int GetDeepHashCode()
        {
            int valueHashCode = (_value != null) ? _value.GetHashCode() : 0;

            // GetHashCode on an enum boxes so cast to int
            return ((int)_valueType).GetHashCode() ^ valueHashCode;
        }

        private static bool ValuesEquals(JValue v1, JValue v2)
        {
            return (v1 == v2 || (v1._valueType == v2._valueType && Compare(v1._valueType, v1._value, v2._value) == 0));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(JValue? other)
        {
            if (other == null)
            {
                return false;
            }

            return ValuesEquals(this, other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Object"/> is equal to the current <see cref="Object"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is JValue v)
            {
                return Equals(v);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            if (_value == null)
            {
                return 0;
            }

            return _value.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <remarks>
        /// <c>ToString()</c> returns a non-JSON string value for tokens with a type of <see cref="JTokenType.String"/>.
        /// If you want the JSON for all token types then you should use <see cref="WriteTo(JsonWriter, JsonConverter[])"/>.
        /// </remarks>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_value == null)
            {
                return string.Empty;
            }

            return _value.ToString();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(string? format, IFormatProvider formatProvider)
        {
            if (_value == null)
            {
                return string.Empty;
            }

            if (_value is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }
            else
            {
                return _value.ToString();
            }
        }

#if HAVE_DYNAMIC
        /// <summary>
        /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>
        /// The <see cref="DynamicMetaObject"/> to bind this object.
        /// </returns>
        protected override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JValue>(parameter, this, new JValueDynamicProxy());
        }

        private class JValueDynamicProxy : DynamicProxy<JValue>
        {
            public override bool TryConvert(JValue instance, ConvertBinder binder, [NotNullWhen(true)]out object? result)
            {
                if (binder.Type == typeof(JValue) || binder.Type == typeof(JToken))
                {
                    result = instance;
                    return true;
                }

                object? value = instance.Value;

                if (value == null)
                {
                    result = null;
                    return ReflectionUtils.IsNullable(binder.Type);
                }

                result = ConvertUtils.Convert(value, CultureInfo.InvariantCulture, binder.Type);
                return true;
            }

            public override bool TryBinaryOperation(JValue instance, BinaryOperationBinder binder, object arg, [NotNullWhen(true)]out object? result)
            {
                object? compareValue = arg is JValue value ? value.Value : arg;

                switch (binder.Operation)
                {
                    case ExpressionType.Equal:
                        result = (Compare(instance.Type, instance.Value, compareValue) == 0);
                        return true;
                    case ExpressionType.NotEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) != 0);
                        return true;
                    case ExpressionType.GreaterThan:
                        result = (Compare(instance.Type, instance.Value, compareValue) > 0);
                        return true;
                    case ExpressionType.GreaterThanOrEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) >= 0);
                        return true;
                    case ExpressionType.LessThan:
                        result = (Compare(instance.Type, instance.Value, compareValue) < 0);
                        return true;
                    case ExpressionType.LessThanOrEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) <= 0);
                        return true;
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        if (Operation(binder.Operation, instance.Value, compareValue, out result))
                        {
                            result = new JValue(result);
                            return true;
                        }
                        break;
                }

                result = null;
                return false;
            }
        }
#endif

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            JTokenType comparisonType;
            object? otherValue;
            if (obj is JValue value)
            {
                otherValue = value.Value;
                comparisonType = (_valueType == JTokenType.String && _valueType != value._valueType)
                    ? value._valueType
                    : _valueType;
            }
            else
            {
                otherValue = obj;
                comparisonType = _valueType;
            }

            return Compare(comparisonType, _value, otherValue);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This instance is less than <paramref name="obj"/>.
        /// Zero
        /// This instance is equal to <paramref name="obj"/>.
        /// Greater than zero
        /// This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 	<paramref name="obj"/> is not of the same type as this instance.
        /// </exception>
        public int CompareTo(JValue obj)
        {
            if (obj == null)
            {
                return 1;
            }

            JTokenType comparisonType = (_valueType == JTokenType.String && _valueType != obj._valueType)
                ? obj._valueType
                : _valueType;

            return Compare(comparisonType, _value, obj._value);
        }

#if HAVE_ICONVERTIBLE
        TypeCode IConvertible.GetTypeCode()
        {
            if (_value == null)
            {
                return TypeCode.Empty;
            }

            if (_value is IConvertible convertable)
            {
                return convertable.GetTypeCode();
            }

            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return (bool)this;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return (char)this;
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return (sbyte)this;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return (byte)this;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return (short)this;
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return (ushort)this;
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return (int)this;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return (uint)this;
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return (long)this;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return (ulong)this;
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (float)this;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return (double)this;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return (decimal)this;
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return (DateTime)this;
        }

        object? IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return ToObject(conversionType);
        }
#endif
    }
}
