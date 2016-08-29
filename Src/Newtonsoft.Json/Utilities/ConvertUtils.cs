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
using System.ComponentModel;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;
using System.Reflection;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
using System.Data.SqlTypes;

#endif

namespace Newtonsoft.Json.Utilities
{
    internal enum PrimitiveTypeCode
    {
        Empty = 0,
        Object = 1,
        Char = 2,
        CharNullable = 3,
        Boolean = 4,
        BooleanNullable = 5,
        SByte = 6,
        SByteNullable = 7,
        Int16 = 8,
        Int16Nullable = 9,
        UInt16 = 10,
        UInt16Nullable = 11,
        Int32 = 12,
        Int32Nullable = 13,
        Byte = 14,
        ByteNullable = 15,
        UInt32 = 16,
        UInt32Nullable = 17,
        Int64 = 18,
        Int64Nullable = 19,
        UInt64 = 20,
        UInt64Nullable = 21,
        Single = 22,
        SingleNullable = 23,
        Double = 24,
        DoubleNullable = 25,
        DateTime = 26,
        DateTimeNullable = 27,
        DateTimeOffset = 28,
        DateTimeOffsetNullable = 29,
        Decimal = 30,
        DecimalNullable = 31,
        Guid = 32,
        GuidNullable = 33,
        TimeSpan = 34,
        TimeSpanNullable = 35,
        BigInteger = 36,
        BigIntegerNullable = 37,
        Uri = 38,
        String = 39,
        Bytes = 40,
        DBNull = 41
    }

    internal class TypeInformation
    {
        public Type Type { get; set; }
        public PrimitiveTypeCode TypeCode { get; set; }
    }

    internal enum ParseResult
    {
        None = 0,
        Success = 1,
        Overflow = 2,
        Invalid = 3
    }

    internal static class ConvertUtils
    {
        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap =
            new Dictionary<Type, PrimitiveTypeCode>
            {
                { typeof(char), PrimitiveTypeCode.Char },
                { typeof(char?), PrimitiveTypeCode.CharNullable },
                { typeof(bool), PrimitiveTypeCode.Boolean },
                { typeof(bool?), PrimitiveTypeCode.BooleanNullable },
                { typeof(sbyte), PrimitiveTypeCode.SByte },
                { typeof(sbyte?), PrimitiveTypeCode.SByteNullable },
                { typeof(short), PrimitiveTypeCode.Int16 },
                { typeof(short?), PrimitiveTypeCode.Int16Nullable },
                { typeof(ushort), PrimitiveTypeCode.UInt16 },
                { typeof(ushort?), PrimitiveTypeCode.UInt16Nullable },
                { typeof(int), PrimitiveTypeCode.Int32 },
                { typeof(int?), PrimitiveTypeCode.Int32Nullable },
                { typeof(byte), PrimitiveTypeCode.Byte },
                { typeof(byte?), PrimitiveTypeCode.ByteNullable },
                { typeof(uint), PrimitiveTypeCode.UInt32 },
                { typeof(uint?), PrimitiveTypeCode.UInt32Nullable },
                { typeof(long), PrimitiveTypeCode.Int64 },
                { typeof(long?), PrimitiveTypeCode.Int64Nullable },
                { typeof(ulong), PrimitiveTypeCode.UInt64 },
                { typeof(ulong?), PrimitiveTypeCode.UInt64Nullable },
                { typeof(float), PrimitiveTypeCode.Single },
                { typeof(float?), PrimitiveTypeCode.SingleNullable },
                { typeof(double), PrimitiveTypeCode.Double },
                { typeof(double?), PrimitiveTypeCode.DoubleNullable },
                { typeof(DateTime), PrimitiveTypeCode.DateTime },
                { typeof(DateTime?), PrimitiveTypeCode.DateTimeNullable },
#if !NET20
                { typeof(DateTimeOffset), PrimitiveTypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), PrimitiveTypeCode.DateTimeOffsetNullable },
#endif
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(Guid), PrimitiveTypeCode.Guid },
                { typeof(Guid?), PrimitiveTypeCode.GuidNullable },
                { typeof(TimeSpan), PrimitiveTypeCode.TimeSpan },
                { typeof(TimeSpan?), PrimitiveTypeCode.TimeSpanNullable },
#if !(PORTABLE || PORTABLE40 || NET35 || NET20)
                { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
                { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
#endif
                { typeof(Uri), PrimitiveTypeCode.Uri },
                { typeof(string), PrimitiveTypeCode.String },
                { typeof(byte[]), PrimitiveTypeCode.Bytes },
#if !(PORTABLE || PORTABLE40 || DOTNET)
                { typeof(DBNull), PrimitiveTypeCode.DBNull }
#endif
            };

#if !PORTABLE
        private static readonly TypeInformation[] PrimitiveTypeCodes =
        {
            // need all of these. lookup against the index with TypeCode value
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Empty },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Object },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.DBNull },
            new TypeInformation { Type = typeof(bool), TypeCode = PrimitiveTypeCode.Boolean },
            new TypeInformation { Type = typeof(char), TypeCode = PrimitiveTypeCode.Char },
            new TypeInformation { Type = typeof(sbyte), TypeCode = PrimitiveTypeCode.SByte },
            new TypeInformation { Type = typeof(byte), TypeCode = PrimitiveTypeCode.Byte },
            new TypeInformation { Type = typeof(short), TypeCode = PrimitiveTypeCode.Int16 },
            new TypeInformation { Type = typeof(ushort), TypeCode = PrimitiveTypeCode.UInt16 },
            new TypeInformation { Type = typeof(int), TypeCode = PrimitiveTypeCode.Int32 },
            new TypeInformation { Type = typeof(uint), TypeCode = PrimitiveTypeCode.UInt32 },
            new TypeInformation { Type = typeof(long), TypeCode = PrimitiveTypeCode.Int64 },
            new TypeInformation { Type = typeof(ulong), TypeCode = PrimitiveTypeCode.UInt64 },
            new TypeInformation { Type = typeof(float), TypeCode = PrimitiveTypeCode.Single },
            new TypeInformation { Type = typeof(double), TypeCode = PrimitiveTypeCode.Double },
            new TypeInformation { Type = typeof(decimal), TypeCode = PrimitiveTypeCode.Decimal },
            new TypeInformation { Type = typeof(DateTime), TypeCode = PrimitiveTypeCode.DateTime },
            new TypeInformation { Type = typeof(object), TypeCode = PrimitiveTypeCode.Empty }, // no 17 in TypeCode for some reason
            new TypeInformation { Type = typeof(string), TypeCode = PrimitiveTypeCode.String }
        };
#endif

        public static PrimitiveTypeCode GetTypeCode(Type t)
        {
            bool isEnum;
            return GetTypeCode(t, out isEnum);
        }

        public static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
        {
            PrimitiveTypeCode typeCode;
            if (TypeCodeMap.TryGetValue(t, out typeCode))
            {
                isEnum = false;
                return typeCode;
            }

            if (t.IsEnum())
            {
                isEnum = true;
                return GetTypeCode(Enum.GetUnderlyingType(t));
            }

            // performance?
            if (ReflectionUtils.IsNullableType(t))
            {
                Type nonNullable = Nullable.GetUnderlyingType(t);
                if (nonNullable.IsEnum())
                {
                    Type nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(nonNullable));
                    isEnum = true;
                    return GetTypeCode(nullableUnderlyingType);
                }
            }

            isEnum = false;
            return PrimitiveTypeCode.Object;
        }

#if !PORTABLE
        public static TypeInformation GetTypeInformation(IConvertible convertable)
        {
            TypeInformation typeInformation = PrimitiveTypeCodes[(int)convertable.GetTypeCode()];
            return typeInformation;
        }
#endif

        public static bool IsConvertible(Type t)
        {
#if !PORTABLE
            return typeof(IConvertible).IsAssignableFrom(t);
#else
            return (
                t == typeof(bool) || t == typeof(byte) || t == typeof(char) || t == typeof(DateTime) || t == typeof(decimal) || t == typeof(double) || t == typeof(short) || t == typeof(int) ||
                t == typeof(long) || t == typeof(sbyte) || t == typeof(float) || t == typeof(string) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t.IsEnum());
#endif
        }

        public static TimeSpan ParseTimeSpan(string input)
        {
#if !(NET35 || NET20)
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
#else
            return TimeSpan.Parse(input);
#endif
        }

        internal struct TypeConvertKey : IEquatable<TypeConvertKey>
        {
            private readonly Type _initialType;
            private readonly Type _targetType;

            public Type InitialType
            {
                get { return _initialType; }
            }

            public Type TargetType
            {
                get { return _targetType; }
            }

            public TypeConvertKey(Type initialType, Type targetType)
            {
                _initialType = initialType;
                _targetType = targetType;
            }

            public override int GetHashCode()
            {
                return _initialType.GetHashCode() ^ _targetType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeConvertKey))
                {
                    return false;
                }

                return Equals((TypeConvertKey)obj);
            }

            public bool Equals(TypeConvertKey other)
            {
                return (_initialType == other._initialType && _targetType == other._targetType);
            }
        }

        private static readonly ThreadSafeStore<TypeConvertKey, Func<object, object>> CastConverters =
            new ThreadSafeStore<TypeConvertKey, Func<object, object>>(CreateCastConverter);

        private static Func<object, object> CreateCastConverter(TypeConvertKey t)
        {
            MethodInfo castMethodInfo = t.TargetType.GetMethod("op_Implicit", new[] { t.InitialType });
            if (castMethodInfo == null)
            {
                castMethodInfo = t.TargetType.GetMethod("op_Explicit", new[] { t.InitialType });
            }

            if (castMethodInfo == null)
            {
                return null;
            }

            MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(castMethodInfo);

            return o => call(null, o);
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        internal static BigInteger ToBigInteger(object value)
        {
            if (value is BigInteger)
            {
                return (BigInteger)value;
            }
            if (value is string)
            {
                return BigInteger.Parse((string)value, CultureInfo.InvariantCulture);
            }
            if (value is float)
            {
                return new BigInteger((float)value);
            }
            if (value is double)
            {
                return new BigInteger((double)value);
            }
            if (value is decimal)
            {
                return new BigInteger((decimal)value);
            }
            if (value is int)
            {
                return new BigInteger((int)value);
            }
            if (value is long)
            {
                return new BigInteger((long)value);
            }
            if (value is uint)
            {
                return new BigInteger((uint)value);
            }
            if (value is ulong)
            {
                return new BigInteger((ulong)value);
            }
            if (value is byte[])
            {
                return new BigInteger((byte[])value);
            }

            throw new InvalidCastException("Cannot convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

        public static object FromBigInteger(BigInteger i, Type targetType)
        {
            if (targetType == typeof(decimal))
            {
                return (decimal)i;
            }
            if (targetType == typeof(double))
            {
                return (double)i;
            }
            if (targetType == typeof(float))
            {
                return (float)i;
            }
            if (targetType == typeof(ulong))
            {
                return (ulong)i;
            }
            if (targetType == typeof(bool))
            {
                return i != 0;
            }

            try
            {
                return System.Convert.ChangeType((long)i, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can not convert from BigInteger to {0}.".FormatWith(CultureInfo.InvariantCulture, targetType), ex);
            }
        }
#endif

        #region TryConvert
        internal enum ConvertResult
        {
            Success = 0,
            CannotConvertNull = 1,
            NotInstantiableType = 2,
            NoValidConversion = 3
        }

        public static object Convert(object initialValue, CultureInfo culture, Type targetType)
        {
            object value;
            switch (TryConvertInternal(initialValue, culture, targetType, out value))
            {
                case ConvertResult.Success:
                    return value;
                case ConvertResult.CannotConvertNull:
                    throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType));
                case ConvertResult.NotInstantiableType:
                    throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), nameof(targetType));
                case ConvertResult.NoValidConversion:
                    throw new InvalidOperationException("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType));
                default:
                    throw new InvalidOperationException("Unexpected conversion result.");
            }
        }

        private static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            try
            {
                if (TryConvertInternal(initialValue, culture, targetType, out value) == ConvertResult.Success)
                {
                    return true;
                }

                value = null;
                return false;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static ConvertResult TryConvertInternal(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            if (initialValue == null)
            {
                throw new ArgumentNullException(nameof(initialValue));
            }

            if (ReflectionUtils.IsNullableType(targetType))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            Type initialType = initialValue.GetType();

            if (targetType == initialType)
            {
                value = initialValue;
                return ConvertResult.Success;
            }

            // use Convert.ChangeType if both types are IConvertible
            if (ConvertUtils.IsConvertible(initialValue.GetType()) && ConvertUtils.IsConvertible(targetType))
            {
                if (targetType.IsEnum())
                {
                    if (initialValue is string)
                    {
                        value = Enum.Parse(targetType, initialValue.ToString(), true);
                        return ConvertResult.Success;
                    }
                    else if (IsInteger(initialValue))
                    {
                        value = Enum.ToObject(targetType, initialValue);
                        return ConvertResult.Success;
                    }
                }

                value = System.Convert.ChangeType(initialValue, targetType, culture);
                return ConvertResult.Success;
            }

#if !NET20
            if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
            {
                value = new DateTimeOffset((DateTime)initialValue);
                return ConvertResult.Success;
            }
#endif

            if (initialValue is byte[] && targetType == typeof(Guid))
            {
                value = new Guid((byte[])initialValue);
                return ConvertResult.Success;
            }

            if (initialValue is Guid && targetType == typeof(byte[]))
            {
                value = ((Guid)initialValue).ToByteArray();
                return ConvertResult.Success;
            }

            string s = initialValue as string;
            if (s != null)
            {
                if (targetType == typeof(Guid))
                {
                    value = new Guid(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(Uri))
                {
                    value = new Uri(s, UriKind.RelativeOrAbsolute);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(TimeSpan))
                {
                    value = ParseTimeSpan(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(byte[]))
                {
                    value = System.Convert.FromBase64String(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(Version))
                {
                    Version result;
                    if (VersionTryParse(s, out result))
                    {
                        value = result;
                        return ConvertResult.Success;
                    }
                    value = null;
                    return ConvertResult.NoValidConversion;
                }
                if (typeof(Type).IsAssignableFrom(targetType))
                {
                    value = Type.GetType(s, true);
                    return ConvertResult.Success;
                }
            }

#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
            if (targetType == typeof(BigInteger))
            {
                value = ToBigInteger(initialValue);
                return ConvertResult.Success;
            }
            if (initialValue is BigInteger)
            {
                value = FromBigInteger((BigInteger)initialValue, targetType);
                return ConvertResult.Success;
            }
#endif

#if !(PORTABLE40 || PORTABLE)
            // see if source or target types have a TypeConverter that converts between the two
            TypeConverter toConverter = GetConverter(initialType);

            if (toConverter != null && toConverter.CanConvertTo(targetType))
            {
                value = toConverter.ConvertTo(null, culture, initialValue, targetType);
                return ConvertResult.Success;
            }

            TypeConverter fromConverter = GetConverter(targetType);

            if (fromConverter != null && fromConverter.CanConvertFrom(initialType))
            {
                value = fromConverter.ConvertFrom(null, culture, initialValue);
                return ConvertResult.Success;
            }
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
            // handle DBNull and INullable
            if (initialValue == DBNull.Value)
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    value = EnsureTypeAssignable(null, initialType, targetType);
                    return ConvertResult.Success;
                }

                // cannot convert null to non-nullable
                value = null;
                return ConvertResult.CannotConvertNull;
            }
#endif
#if !(DOTNET || PORTABLE40 || PORTABLE)
            if (initialValue is INullable)
            {
                value = EnsureTypeAssignable(ToValue((INullable)initialValue), initialType, targetType);
                return ConvertResult.Success;
            }
#endif

            if (targetType.IsInterface() || targetType.IsGenericTypeDefinition() || targetType.IsAbstract())
            {
                value = null;
                return ConvertResult.NotInstantiableType;
            }

            value = null;
            return ConvertResult.NoValidConversion;
        }
        #endregion

        #region ConvertOrCast
        /// <summary>
        /// Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.
        /// </summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <param name="culture">The culture to use when converting.</param>
        /// <param name="targetType">The type to convert or cast the value to.</param>
        /// <returns>
        /// The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.
        /// </returns>
        public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
        {
            object convertedValue;

            if (targetType == typeof(object))
            {
                return initialValue;
            }

            if (initialValue == null && ReflectionUtils.IsNullable(targetType))
            {
                return null;
            }

            if (TryConvert(initialValue, culture, targetType, out convertedValue))
            {
                return convertedValue;
            }

            return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
        }
        #endregion

        private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
        {
            Type valueType = (value != null) ? value.GetType() : null;

            if (value != null)
            {
                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }

                Func<object, object> castConverter = CastConverters.Get(new TypeConvertKey(valueType, targetType));
                if (castConverter != null)
                {
                    return castConverter(value);
                }
            }
            else
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    return null;
                }
            }

            throw new ArgumentException("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, (initialType != null) ? initialType.ToString() : "{null}", targetType));
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
        public static object ToValue(INullable nullableValue)
        {
            if (nullableValue == null)
            {
                return null;
            }
            else if (nullableValue is SqlInt32)
            {
                return ToValue((SqlInt32)nullableValue);
            }
            else if (nullableValue is SqlInt64)
            {
                return ToValue((SqlInt64)nullableValue);
            }
            else if (nullableValue is SqlBoolean)
            {
                return ToValue((SqlBoolean)nullableValue);
            }
            else if (nullableValue is SqlString)
            {
                return ToValue((SqlString)nullableValue);
            }
            else if (nullableValue is SqlDateTime)
            {
                return ToValue((SqlDateTime)nullableValue);
            }

            throw new ArgumentException("Unsupported INullable type: {0}".FormatWith(CultureInfo.InvariantCulture, nullableValue.GetType()));
        }
#endif

#if !(PORTABLE40 || PORTABLE)
        internal static TypeConverter GetConverter(Type t)
        {
            return JsonTypeReflector.GetTypeConverter(t);
        }
#endif

        public static bool VersionTryParse(string input, out Version result)
        {
#if !(NET20 || NET35)
            return Version.TryParse(input, out result);
#else
    // improve failure performance with regex?
            try
            {
                result = new Version(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
#endif
        }

        public static bool IsInteger(object value)
        {
            switch (GetTypeCode(value.GetType()))
            {
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static ParseResult Int32TryParse(char[] chars, int start, int length, out int value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int32.MaxValue and MinValue are 10 chars
            // Or is 10 chars and start is greater than two
            // Need to improve this!
            if (length > 10 || (length == 10 && chars[start] - '0' > 2))
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                int newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == int.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }

        public static ParseResult Int64TryParse(char[] chars, int start, int length, out long value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int64.MaxValue and MinValue are 19 chars
            if (length > 19)
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                long newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == long.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }

        private const int MaxExponent = 308;
        private static readonly double[] DoubleExponents =
        {
            1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20,
            1e21, 1e22, 1e23, 1e24, 1e25, 1e26, 1e27, 1e28, 1e29, 1e30, 1e31, 1e32, 1e33, 1e34, 1e35, 1e36, 1e37, 1e38, 1e39, 1e40,
            1e41, 1e42, 1e43, 1e44, 1e45, 1e46, 1e47, 1e48, 1e49, 1e50, 1e51, 1e52, 1e53, 1e54, 1e55, 1e56, 1e57, 1e58, 1e59, 1e60,
            1e61, 1e62, 1e63, 1e64, 1e65, 1e66, 1e67, 1e68, 1e69, 1e70, 1e71, 1e72, 1e73, 1e74, 1e75, 1e76, 1e77, 1e78, 1e79, 1e80,
            1e81, 1e82, 1e83, 1e84, 1e85, 1e86, 1e87, 1e88, 1e89, 1e90, 1e91, 1e92, 1e93, 1e94, 1e95, 1e96, 1e97, 1e98, 1e99, 1e100,
            1e101, 1e102, 1e103, 1e104, 1e105, 1e106, 1e107, 1e108, 1e109, 1e110, 1e111, 1e112, 1e113, 1e114, 1e115, 1e116, 1e117, 1e118, 1e119, 1e120,
            1e121, 1e122, 1e123, 1e124, 1e125, 1e126, 1e127, 1e128, 1e129, 1e130, 1e131, 1e132, 1e133, 1e134, 1e135, 1e136, 1e137, 1e138, 1e139, 1e140,
            1e141, 1e142, 1e143, 1e144, 1e145, 1e146, 1e147, 1e148, 1e149, 1e150, 1e151, 1e152, 1e153, 1e154, 1e155, 1e156, 1e157, 1e158, 1e159, 1e160,
            1e161, 1e162, 1e163, 1e164, 1e165, 1e166, 1e167, 1e168, 1e169, 1e170, 1e171, 1e172, 1e173, 1e174, 1e175, 1e176, 1e177, 1e178, 1e179, 1e180,
            1e181, 1e182, 1e183, 1e184, 1e185, 1e186, 1e187, 1e188, 1e189, 1e190, 1e191, 1e192, 1e193, 1e194, 1e195, 1e196, 1e197, 1e198, 1e199, 1e200,
            1e201, 1e202, 1e203, 1e204, 1e205, 1e206, 1e207, 1e208, 1e209, 1e210, 1e211, 1e212, 1e213, 1e214, 1e215, 1e216, 1e217, 1e218, 1e219, 1e220,
            1e221, 1e222, 1e223, 1e224, 1e225, 1e226, 1e227, 1e228, 1e229, 1e230, 1e231, 1e232, 1e233, 1e234, 1e235, 1e236, 1e237, 1e238, 1e239, 1e240,
            1e241, 1e242, 1e243, 1e244, 1e245, 1e246, 1e247, 1e248, 1e249, 1e250, 1e251, 1e252, 1e253, 1e254, 1e255, 1e256, 1e257, 1e258, 1e259, 1e260,
            1e261, 1e262, 1e263, 1e264, 1e265, 1e266, 1e267, 1e268, 1e269, 1e270, 1e271, 1e272, 1e273, 1e274, 1e275, 1e276, 1e277, 1e278, 1e279, 1e280,
            1e281, 1e282, 1e283, 1e284, 1e285, 1e286, 1e287, 1e288, 1e289, 1e290, 1e291, 1e292, 1e293, 1e294, 1e295, 1e296, 1e297, 1e298, 1e299, 1e300,
            1e301, 1e302, 1e303, 1e304, 1e305, 1e306, 1e307, 1e308
        };
        private static readonly double[] DoubleNegativeExponents =
        {
            1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8, 1e-9, 1e-10, 1e-11, 1e-12, 1e-13, 1e-14, 1e-15, 1e-16, 1e-17, 1e-18, 1e-19, 1e-20,
            1e-21, 1e-22, 1e-23, 1e-24, 1e-25, 1e-26, 1e-27, 1e-28, 1e-29, 1e-30, 1e-31, 1e-32, 1e-33, 1e-34, 1e-35, 1e-36, 1e-37, 1e-38, 1e-39, 1e-40,
            1e-41, 1e-42, 1e-43, 1e-44, 1e-45, 1e-46, 1e-47, 1e-48, 1e-49, 1e-50, 1e-51, 1e-52, 1e-53, 1e-54, 1e-55, 1e-56, 1e-57, 1e-58, 1e-59, 1e-60,
            1e-61, 1e-62, 1e-63, 1e-64, 1e-65, 1e-66, 1e-67, 1e-68, 1e-69, 1e-70, 1e-71, 1e-72, 1e-73, 1e-74, 1e-75, 1e-76, 1e-77, 1e-78, 1e-79, 1e-80,
            1e-81, 1e-82, 1e-83, 1e-84, 1e-85, 1e-86, 1e-87, 1e-88, 1e-89, 1e-90, 1e-91, 1e-92, 1e-93, 1e-94, 1e-95, 1e-96, 1e-97, 1e-98, 1e-99, 1e-100,
            1e-101, 1e-102, 1e-103, 1e-104, 1e-105, 1e-106, 1e-107, 1e-108, 1e-109, 1e-110, 1e-111, 1e-112, 1e-113, 1e-114, 1e-115, 1e-116, 1e-117, 1e-118, 1e-119, 1e-120,
            1e-121, 1e-122, 1e-123, 1e-124, 1e-125, 1e-126, 1e-127, 1e-128, 1e-129, 1e-130, 1e-131, 1e-132, 1e-133, 1e-134, 1e-135, 1e-136, 1e-137, 1e-138, 1e-139, 1e-140,
            1e-141, 1e-142, 1e-143, 1e-144, 1e-145, 1e-146, 1e-147, 1e-148, 1e-149, 1e-150, 1e-151, 1e-152, 1e-153, 1e-154, 1e-155, 1e-156, 1e-157, 1e-158, 1e-159, 1e-160,
            1e-161, 1e-162, 1e-163, 1e-164, 1e-165, 1e-166, 1e-167, 1e-168, 1e-169, 1e-170, 1e-171, 1e-172, 1e-173, 1e-174, 1e-175, 1e-176, 1e-177, 1e-178, 1e-179, 1e-180,
            1e-181, 1e-182, 1e-183, 1e-184, 1e-185, 1e-186, 1e-187, 1e-188, 1e-189, 1e-190, 1e-191, 1e-192, 1e-193, 1e-194, 1e-195, 1e-196, 1e-197, 1e-198, 1e-199, 1e-200,
            1e-201, 1e-202, 1e-203, 1e-204, 1e-205, 1e-206, 1e-207, 1e-208, 1e-209, 1e-210, 1e-211, 1e-212, 1e-213, 1e-214, 1e-215, 1e-216, 1e-217, 1e-218, 1e-219, 1e-220,
            1e-221, 1e-222, 1e-223, 1e-224, 1e-225, 1e-226, 1e-227, 1e-228, 1e-229, 1e-230, 1e-231, 1e-232, 1e-233, 1e-234, 1e-235, 1e-236, 1e-237, 1e-238, 1e-239, 1e-240,
            1e-241, 1e-242, 1e-243, 1e-244, 1e-245, 1e-246, 1e-247, 1e-248, 1e-249, 1e-250, 1e-251, 1e-252, 1e-253, 1e-254, 1e-255, 1e-256, 1e-257, 1e-258, 1e-259, 1e-260,
            1e-261, 1e-262, 1e-263, 1e-264, 1e-265, 1e-266, 1e-267, 1e-268, 1e-269, 1e-270, 1e-271, 1e-272, 1e-273, 1e-274, 1e-275, 1e-276, 1e-277, 1e-278, 1e-279, 1e-280,
            1e-281, 1e-282, 1e-283, 1e-284, 1e-285, 1e-286, 1e-287, 1e-288, 1e-289, 1e-290, 1e-291, 1e-292, 1e-293, 1e-294, 1e-295, 1e-296, 1e-297, 1e-298, 1e-299, 1e-300,
            1e-301, 1e-302, 1e-303, 1e-304, 1e-305, 1e-306, 1e-307, 1e-308
        };

        public static ParseResult DoubleTryParse(char[] chars, int start, int length, out double value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');
            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int i = start;
            int end = start + length;
            int numDecimalStart = end;
            int numDecimalEnd = end;
            int exponent = 0;
            bool exponentNegative = false;
            ulong mantissa = 0UL;
            int mantissaDigits = 0;
            int exponentFromMantissa = 0;
            for (; i < end; i++)
            {
                char c = chars[i];
                switch (c)
                {
                    case '.':
                        if (i == start)
                        {
                            return ParseResult.Invalid;
                        }

                        if (numDecimalStart != end)
                        {
                            // multiple decimal points
                            return ParseResult.Invalid;
                        }

                        numDecimalStart = i + 1;
                        break;
                    case 'e':
                    case 'E':
                        if (i == start)
                        {
                            return ParseResult.Invalid;
                        }
                        if (i == numDecimalStart)
                        {
                            // E follows decimal point
                            return ParseResult.Invalid;
                        }
                        i++;
                        if (i == end)
                        {
                            return ParseResult.Invalid;
                        }

                        if (numDecimalStart < end)
                        {
                            numDecimalEnd = i - 1;
                        }

                        c = chars[i];
                        switch (c)
                        {
                            case '-':
                                exponentNegative = true;
                                i++;
                                break;
                            case '+':
                                i++;
                                break;
                        }

                        // parse 3 digit 
                        for (;i < end;i++)
                        {
                            c = chars[i];
                            if (c < '0' || c > '9')
                            {
                                return ParseResult.Invalid;
                            }

                            exponent = (10 * exponent) + (c - '0');
                        }

                        if (exponentNegative)
                        {
                            exponent = -exponent;
                        }
                        else
                        {
                            if (exponent > MaxExponent)
                            {
                                return ParseResult.Overflow;
                            }
                        }
                        break;
                    default:
                        if (c < '0' || c > '9')
                        {
                            return ParseResult.Invalid;
                        }

                        if (mantissaDigits < 19)
                        {
                            mantissa = (10 * mantissa) + (ulong)(c - '0');
                            ++mantissaDigits;
                        }
                        else
                        {
                            ++exponentFromMantissa;
                        }
                        break;
                }
            }

            exponent += exponentFromMantissa;

            // correct the decimal point
            exponent -= (numDecimalEnd - numDecimalStart);

            if (exponent > 0)
            {
                value = mantissa * DoubleExponents[exponent - 1];
            }
            else if (exponent < 0)
            {
                exponent = -exponent;
                if (exponent > MaxExponent)
                {
                    // handle very small numbers, e.g. 4.94065645841247E-324
                    value = mantissa * DoubleNegativeExponents[154 - 1];
                    exponent -= 154;
                }
                else
                {
                    value = mantissa;
                }

                if (DoubleNegativeExponents.Length <= exponent - 1)
                {
                    value = 0;
                    return ParseResult.Success;
                }
                
                value = exponent < 23 ? value / DoubleExponents[exponent - 1] : value * DoubleNegativeExponents[exponent - 1];
            }
            else
            {
                value = mantissa;
            }

            if (double.IsPositiveInfinity(value))
            {
                return ParseResult.Overflow;
            }

            if (isNegative)
            {
                value = -value;
            }

            return ParseResult.Success;
        }

        public static bool TryConvertGuid(string s, out Guid g)
        {
            // GUID has to have format 00000000-0000-0000-0000-000000000000
#if NET20 || NET35
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            Regex format = new Regex("^[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}$");
            Match match = format.Match(s);
            if (match.Success)
            {
                g = new Guid(s);
                return true;
            }

            g = Guid.Empty;
            return false;
#else
            return Guid.TryParseExact(s, "D", out g);
#endif
        }

        public static int HexTextToInt(char[] text, int start, int end)
        {
            int value = 0;
            for (int i = start; i < end; i++)
            {
                value += HexCharToInt(text[i]) << ((end - 1 - i) * 4);
            }
            return value;
        }

        private static int HexCharToInt(char ch)
        {
            if (ch <= 57 && ch >= 48)
            {
                return ch - 48;
            }

            if (ch <= 70 && ch >= 65)
            {
                return ch - 55;
            }

            if (ch <= 102 && ch >= 97)
            {
                return ch - 87;
            }

            throw new FormatException("Invalid hex character: " + ch);
        }
    }
}