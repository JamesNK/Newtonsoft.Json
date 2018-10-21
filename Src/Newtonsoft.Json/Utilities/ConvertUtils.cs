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
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
#if !HAVE_GUID_TRY_PARSE
using System.Text;
using System.Text.RegularExpressions;
#endif
using Newtonsoft.Json.Serialization;
using System.Reflection;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
#if HAVE_ADO_NET
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
#if HAVE_DATE_TIME_OFFSET
                { typeof(DateTimeOffset), PrimitiveTypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), PrimitiveTypeCode.DateTimeOffsetNullable },
#endif
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(Guid), PrimitiveTypeCode.Guid },
                { typeof(Guid?), PrimitiveTypeCode.GuidNullable },
                { typeof(TimeSpan), PrimitiveTypeCode.TimeSpan },
                { typeof(TimeSpan?), PrimitiveTypeCode.TimeSpanNullable },
#if HAVE_BIG_INTEGER
                { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
                { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
#endif
                { typeof(Uri), PrimitiveTypeCode.Uri },
                { typeof(string), PrimitiveTypeCode.String },
                { typeof(byte[]), PrimitiveTypeCode.Bytes },
#if HAVE_ADO_NET
                { typeof(DBNull), PrimitiveTypeCode.DBNull }
#endif
            };

#if HAVE_ICONVERTIBLE
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
            return GetTypeCode(t, out _);
        }

        public static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
        {
            if (TypeCodeMap.TryGetValue(t, out PrimitiveTypeCode typeCode))
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

#if HAVE_ICONVERTIBLE
        public static TypeInformation GetTypeInformation(IConvertible convertable)
        {
            TypeInformation typeInformation = PrimitiveTypeCodes[(int)convertable.GetTypeCode()];
            return typeInformation;
        }
#endif

        public static bool IsConvertible(Type t)
        {
#if HAVE_ICONVERTIBLE
            return typeof(IConvertible).IsAssignableFrom(t);
#else
            return (
                t == typeof(bool) || t == typeof(byte) || t == typeof(char) || t == typeof(DateTime) || t == typeof(decimal) || t == typeof(double) || t == typeof(short) || t == typeof(int) ||
                t == typeof(long) || t == typeof(sbyte) || t == typeof(float) || t == typeof(string) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t.IsEnum());
#endif
        }

        public static TimeSpan ParseTimeSpan(string input)
        {
#if HAVE_TIME_SPAN_PARSE_WITH_CULTURE
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
#else
            return TimeSpan.Parse(input);
#endif
        }

        private static readonly ThreadSafeStore<StructMultiKey<Type, Type>, Func<object, object>> CastConverters =
            new ThreadSafeStore<StructMultiKey<Type, Type>, Func<object, object>>(CreateCastConverter);

        private static Func<object, object> CreateCastConverter(StructMultiKey<Type, Type> t)
        {
            Type initialType = t.Value1;
            Type targetType = t.Value2;
            MethodInfo castMethodInfo = targetType.GetMethod("op_Implicit", new[] { initialType })
                ?? targetType.GetMethod("op_Explicit", new[] { initialType });

            if (castMethodInfo == null)
            {
                return null;
            }

            MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(castMethodInfo);

            return o => call(null, o);
        }

#if HAVE_BIG_INTEGER
        internal static BigInteger ToBigInteger(object value)
        {
            if (value is BigInteger integer)
            {
                return integer;
            }

            if (value is string s)
            {
                return BigInteger.Parse(s, CultureInfo.InvariantCulture);
            }

            if (value is float f)
            {
                return new BigInteger(f);
            }
            if (value is double d)
            {
                return new BigInteger(d);
            }
            if (value is decimal @decimal)
            {
                return new BigInteger(@decimal);
            }
            if (value is int i)
            {
                return new BigInteger(i);
            }
            if (value is long l)
            {
                return new BigInteger(l);
            }
            if (value is uint u)
            {
                return new BigInteger(u);
            }
            if (value is ulong @ulong)
            {
                return new BigInteger(@ulong);
            }

            if (value is byte[] bytes)
            {
                return new BigInteger(bytes);
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
            switch (TryConvertInternal(initialValue, culture, targetType, out object value))
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
            if (IsConvertible(initialValue.GetType()) && IsConvertible(targetType))
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

#if HAVE_DATE_TIME_OFFSET
            if (initialValue is DateTime dt && targetType == typeof(DateTimeOffset))
            {
                value = new DateTimeOffset(dt);
                return ConvertResult.Success;
            }
#endif

            if (initialValue is byte[] bytes && targetType == typeof(Guid))
            {
                value = new Guid(bytes);
                return ConvertResult.Success;
            }

            if (initialValue is Guid guid && targetType == typeof(byte[]))
            {
                value = guid.ToByteArray();
                return ConvertResult.Success;
            }

            if (initialValue is string s)
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
                    if (VersionTryParse(s, out Version result))
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

#if HAVE_BIG_INTEGER
            if (targetType == typeof(BigInteger))
            {
                value = ToBigInteger(initialValue);
                return ConvertResult.Success;
            }
            if (initialValue is BigInteger integer)
            {
                value = FromBigInteger(integer, targetType);
                return ConvertResult.Success;
            }
#endif

#if HAVE_TYPE_DESCRIPTOR
            // see if source or target types have a TypeConverter that converts between the two
            TypeConverter toConverter = TypeDescriptor.GetConverter(initialType);

            if (toConverter != null && toConverter.CanConvertTo(targetType))
            {
                value = toConverter.ConvertTo(null, culture, initialValue, targetType);
                return ConvertResult.Success;
            }

            TypeConverter fromConverter = TypeDescriptor.GetConverter(targetType);

            if (fromConverter != null && fromConverter.CanConvertFrom(initialType))
            {
                value = fromConverter.ConvertFrom(null, culture, initialValue);
                return ConvertResult.Success;
            }
#endif
#if HAVE_ADO_NET
            // handle DBNull
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
            if (targetType == typeof(object))
            {
                return initialValue;
            }

            if (initialValue == null && ReflectionUtils.IsNullable(targetType))
            {
                return null;
            }

            if (TryConvert(initialValue, culture, targetType, out object convertedValue))
            {
                return convertedValue;
            }

            return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
        }
#endregion

        private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
        {
            Type valueType = value?.GetType();

            if (value != null)
            {
                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }

                Func<object, object> castConverter = CastConverters.Get(new StructMultiKey<Type, Type>(valueType, targetType));
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

            throw new ArgumentException("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialType?.ToString() ?? "{null}", targetType));
        }

        public static bool VersionTryParse(string input, out Version result)
        {
#if HAVE_VERSION_TRY_PARSE
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

#if HAS_CUSTOM_DOUBLE_PARSE
        private static class IEEE754
        {
            /// <summary>
            /// Exponents for both powers of 10 and 0.1
            /// </summary>
            private static readonly int[] MultExp64Power10 = new int[]
            {
                4, 7, 10, 14, 17, 20, 24, 27, 30, 34, 37, 40, 44, 47, 50
            };

            /// <summary>
            /// Normalized powers of 10
            /// </summary>
            private static readonly ulong[] MultVal64Power10 = new ulong[]
            {
                0xa000000000000000, 0xc800000000000000, 0xfa00000000000000,
                0x9c40000000000000, 0xc350000000000000, 0xf424000000000000,
                0x9896800000000000, 0xbebc200000000000, 0xee6b280000000000,
                0x9502f90000000000, 0xba43b74000000000, 0xe8d4a51000000000,
                0x9184e72a00000000, 0xb5e620f480000000, 0xe35fa931a0000000,
            };

            /// <summary>
            /// Normalized powers of 0.1
            /// </summary>
            private static readonly ulong[] MultVal64Power10Inv = new ulong[]
            {
                0xcccccccccccccccd, 0xa3d70a3d70a3d70b, 0x83126e978d4fdf3c,
                0xd1b71758e219652e, 0xa7c5ac471b478425, 0x8637bd05af6c69b7,
                0xd6bf94d5e57a42be, 0xabcc77118461ceff, 0x89705f4136b4a599,
                0xdbe6fecebdedd5c2, 0xafebff0bcb24ab02, 0x8cbccc096f5088cf,
                0xe12e13424bb40e18, 0xb424dc35095cd813, 0x901d7cf73ab0acdc,
            };

            /// <summary>
            /// Exponents for both powers of 10^16 and 0.1^16
            /// </summary>
            private static readonly int[] MultExp64Power10By16 = new int[]
            {
                54, 107, 160, 213, 266, 319, 373, 426, 479, 532, 585, 638,
                691, 745, 798, 851, 904, 957, 1010, 1064, 1117,
            };

            /// <summary>
            /// Normalized powers of 10^16
            /// </summary>
            private static readonly ulong[] MultVal64Power10By16 = new ulong[]
            {
                0x8e1bc9bf04000000, 0x9dc5ada82b70b59e, 0xaf298d050e4395d6,
                0xc2781f49ffcfa6d4, 0xd7e77a8f87daf7fa, 0xefb3ab16c59b14a0,
                0x850fadc09923329c, 0x93ba47c980e98cde, 0xa402b9c5a8d3a6e6,
                0xb616a12b7fe617a8, 0xca28a291859bbf90, 0xe070f78d39275566,
                0xf92e0c3537826140, 0x8a5296ffe33cc92c, 0x9991a6f3d6bf1762,
                0xaa7eebfb9df9de8a, 0xbd49d14aa79dbc7e, 0xd226fc195c6a2f88,
                0xe950df20247c83f8, 0x81842f29f2cce373, 0x8fcac257558ee4e2,
            };

            /// <summary>
            /// Normalized powers of 0.1^16
            /// </summary>
            private static readonly ulong[] MultVal64Power10By16Inv = new ulong[]
            {
                0xe69594bec44de160, 0xcfb11ead453994c3, 0xbb127c53b17ec165,
                0xa87fea27a539e9b3, 0x97c560ba6b0919b5, 0x88b402f7fd7553ab,
                0xf64335bcf065d3a0, 0xddd0467c64bce4c4, 0xc7caba6e7c5382ed,
                0xb3f4e093db73a0b7, 0xa21727db38cb0053, 0x91ff83775423cc29,
                0x8380dea93da4bc82, 0xece53cec4a314f00, 0xd5605fcdcf32e217,
                0xc0314325637a1978, 0xad1c8eab5ee43ba2, 0x9becce62836ac5b0,
                0x8c71dcd9ba0b495c, 0xfd00b89747823938, 0xe3e27a444d8d991a,
            };

            /// <summary>
            /// Packs <paramref name="val"/>*10^<paramref name="scale"/> as 64-bit floating point value according to IEEE 754 standard
            /// </summary>
            /// <param name="negative">Sign</param>
            /// <param name="val">Mantissa</param>
            /// <param name="scale">Exponent</param>
            /// <remarks>
            /// Adoption of native function NumberToDouble() from coreclr sources,
            /// see https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/number.cpp#L451
            /// </remarks>
            public static double PackDouble(bool negative, ulong val, int scale)
            {
                // handle zero value
                if (val == 0)
                {
                    return negative ? -0.0 : 0.0;
                }

                // normalize the mantissa
                int exp = 64;

                if ((val & 0xFFFFFFFF00000000) == 0)
                {
                    val <<= 32;
                    exp -= 32;
                }
                if ((val & 0xFFFF000000000000) == 0)
                {
                    val <<= 16;
                    exp -= 16;
                }
                if ((val & 0xFF00000000000000) == 0)
                {
                    val <<= 8;
                    exp -= 8;
                }
                if ((val & 0xF000000000000000) == 0)
                {
                    val <<= 4;
                    exp -= 4;
                }
                if ((val & 0xC000000000000000) == 0)
                {
                    val <<= 2;
                    exp -= 2;
                }
                if ((val & 0x8000000000000000) == 0)
                {
                    val <<= 1;
                    exp -= 1;
                }

                if (scale < 0)
                {
                    scale = -scale;

                    // check scale bounds
                    if (scale >= 22 * 16)
                    {
                        // underflow
                        return negative ? -0.0 : 0.0;
                    }

                    // perform scaling
                    int index = scale & 15;
                    if (index != 0)
                    {
                        exp -= MultExp64Power10[index - 1] - 1;
                        val = Mul64Lossy(val, MultVal64Power10Inv[index - 1], ref exp);
                    }

                    index = scale >> 4;
                    if (index != 0)
                    {
                        exp -= MultExp64Power10By16[index - 1] - 1;
                        val = Mul64Lossy(val, MultVal64Power10By16Inv[index - 1], ref exp);
                    }
                }
                else
                {
                    // check scale bounds
                    if (scale >= 22 * 16)
                    {
                        // overflow
                        return negative ? double.NegativeInfinity : double.PositiveInfinity;
                    }

                    // perform scaling
                    int index = scale & 15;
                    if (index != 0)
                    {
                        exp += MultExp64Power10[index - 1];
                        val = Mul64Lossy(val, MultVal64Power10[index - 1], ref exp);
                    }

                    index = scale >> 4;
                    if (index != 0)
                    {
                        exp += MultExp64Power10By16[index - 1];
                        val = Mul64Lossy(val, MultVal64Power10By16[index - 1], ref exp);
                    }
                }

                // round & scale down

                if ((val & (1 << 10)) != 0)
                {
                    // IEEE round to even
                    ulong tmp = val + ((1UL << 10) - 1 + ((val >> 11) & 1));
                    if (tmp < val)
                    {
                        // overflow
                        tmp = (tmp >> 1) | 0x8000000000000000;
                        exp++;
                    }
                    val = tmp;
                }

                // return the exponent to a biased state

                exp += 0x3FE;

                // handle overflow, underflow, "Epsilon - 1/2 Epsilon", denormalized, and the normal case

                if (exp <= 0)
                {
                    if (exp == -52 && (val >= 0x8000000000000058))
                    {
                        // round X where {Epsilon > X >= 2.470328229206232730000000E-324} up to Epsilon (instead of down to zero)
                        val = 0x0000000000000001;
                    }
                    else if (exp <= -52)
                    {
                        // underflow
                        val = 0;
                    }
                    else
                    {
                        // denormalized value
                        val >>= (-exp + 12);
                    }
                }
                else if (exp >= 0x7FF)
                {
                    // overflow
                    val = 0x7FF0000000000000;
                }
                else
                {
                    // normal positive exponent case
                    val = ((ulong)exp << 52) | ((val >> 11) & 0x000FFFFFFFFFFFFF);
                }

                // apply sign

                if (negative)
                {
                    val |= 0x8000000000000000;
                }

                return BitConverter.Int64BitsToDouble((long)val);
            }

            private static ulong Mul64Lossy(ulong a, ulong b, ref int exp)
            {
                ulong a_hi = (a >> 32);
                uint a_lo = (uint)a;
                ulong b_hi = (b >> 32);
                uint b_lo = (uint)b;

                ulong result = a_hi * b_hi;

                // save some multiplications if lo-parts aren't big enough to produce carry
                // (hi-parts will be always big enough, since a and b are normalized)

                if ((b_lo & 0xFFFF0000) != 0)
                {
                    result += (a_hi * b_lo) >> 32;
                }

                if ((a_lo & 0xFFFF0000) != 0)
                {
                    result += (a_lo * b_hi) >> 32;
                }

                // normalize
                if ((result & 0x8000000000000000) == 0)
                {
                    result <<= 1;
                    exp--;
                }

                return result;
            }
        }

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
                        if (i + 1 == end)
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
                        bool exponentNegative = false;
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
                        for (; i < end; i++)
                        {
                            c = chars[i];
                            if (c < '0' || c > '9')
                            {
                                return ParseResult.Invalid;
                            }

                            int newExponent = (10 * exponent) + (c - '0');
                            // stops updating exponent when overflowing
                            if (exponent < newExponent)
                            {
                                exponent = newExponent;
                            }
                        }

                        if (exponentNegative)
                        {
                            exponent = -exponent;
                        }
                        break;
                    default:
                        if (c < '0' || c > '9')
                        {
                            return ParseResult.Invalid;
                        }

                        if (i == start && c == '0')
                        {
                            i++;
                            if (i != end)
                            {
                                c = chars[i];
                                if (c == '.')
                                {
                                    goto case '.';
                                }
                                if (c == 'e' || c == 'E')
                                {
                                    goto case 'E';
                                }

                                return ParseResult.Invalid;
                            }
                        }

                        if (mantissaDigits < 19)
                        {
                            mantissa = (10 * mantissa) + (ulong)(c - '0');
                            if (mantissa > 0)
                            {
                                ++mantissaDigits;
                            }
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

            value = IEEE754.PackDouble(isNegative, mantissa, exponent);
            return double.IsInfinity(value) ? ParseResult.Overflow : ParseResult.Success;
        }
#endif

        public static ParseResult DecimalTryParse(char[] chars, int start, int length, out decimal value)
        {
            value = 0M;
            const decimal decimalMaxValueHi28 = 7922816251426433759354395033M;
            const ulong decimalMaxValueHi19 = 7922816251426433759UL;
            const ulong decimalMaxValueLo9 = 354395033UL;
            const char decimalMaxValueLo1 = '5';

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
            ulong hi19 = 0UL;
            ulong lo10 = 0UL;
            int mantissaDigits = 0;
            int exponentFromMantissa = 0;
            char? digit29 = null;
            bool? storeOnly28Digits = null;
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
                        if (i + 1 == end)
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
                        bool exponentNegative = false;
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
                        for (; i < end; i++)
                        {
                            c = chars[i];
                            if (c < '0' || c > '9')
                            {
                                return ParseResult.Invalid;
                            }

                            int newExponent = (10 * exponent) + (c - '0');
                            // stops updating exponent when overflowing
                            if (exponent < newExponent)
                            {
                                exponent = newExponent;
                            }
                        }

                        if (exponentNegative)
                        {
                            exponent = -exponent;
                        }
                        break;
                    default:
                        if (c < '0' || c > '9')
                        {
                            return ParseResult.Invalid;
                        }

                        if (i == start && c == '0')
                        {
                            i++;
                            if (i != end)
                            {
                                c = chars[i];
                                if (c == '.')
                                {
                                    goto case '.';
                                }
                                if (c == 'e' || c == 'E')
                                {
                                    goto case 'E';
                                }

                                return ParseResult.Invalid;
                            }
                        }

                        if (mantissaDigits < 29 && (mantissaDigits != 28 || !(storeOnly28Digits ?? (storeOnly28Digits = (hi19 > decimalMaxValueHi19 || (hi19 == decimalMaxValueHi19 && (lo10 > decimalMaxValueLo9 || (lo10 == decimalMaxValueLo9 && c > decimalMaxValueLo1))))).GetValueOrDefault())))
                        {
                            if (mantissaDigits < 19)
                            {
                                hi19 = (hi19 * 10UL) + (ulong)(c - '0');
                            }
                            else
                            {
                                lo10 = (lo10 * 10UL) + (ulong)(c - '0');
                            }
                            ++mantissaDigits;
                        }
                        else
                        {
                            if (!digit29.HasValue)
                            {
                                digit29 = c;
                            }
                            ++exponentFromMantissa;
                        }
                        break;
                }
            }

            exponent += exponentFromMantissa;

            // correct the decimal point
            exponent -= (numDecimalEnd - numDecimalStart);

            if (mantissaDigits <= 19)
            {
                value = hi19;
            }
            else
            {
                value = (hi19 / new decimal(1, 0, 0, false, (byte)(mantissaDigits - 19))) + lo10;
            }

            if (exponent > 0)
            {
                mantissaDigits += exponent;
                if (mantissaDigits > 29)
                {
                    return ParseResult.Overflow;
                }
                if (mantissaDigits == 29)
                {
                    if (exponent > 1)
                    {
                        value /= new decimal(1, 0, 0, false, (byte)(exponent - 1));
                        if (value > decimalMaxValueHi28)
                        {
                            return ParseResult.Overflow;
                        }
                    }
                    else if (value == decimalMaxValueHi28 && digit29 > decimalMaxValueLo1)
                    {
                        return ParseResult.Overflow;
                    }
                    value *= 10M;
                }
                else
                {
                    value /= new decimal(1, 0, 0, false, (byte)exponent);
                }
            }
            else
            {
                if (digit29 >= '5' && exponent >= -28)
                {
                    ++value;
                }
                if (exponent < 0)
                {
                    if (mantissaDigits + exponent + 28 <= 0)
                    {
                        value = isNegative ? -0M : 0M;
                        return ParseResult.Success;
                    }
                    if (exponent >= -28)
                    {
                        value *= new decimal(1, 0, 0, false, (byte)(-exponent));
                    }
                    else
                    {
                        value /= 1e28M;
                        value *= new decimal(1, 0, 0, false, (byte)(-exponent - 28));
                    }
                }
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
#if !HAVE_GUID_TRY_PARSE
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

        public static bool TryHexTextToInt(char[] text, int start, int end, out int value)
        {
            value = 0;

            for (int i = start; i < end; i++)
            {
                char ch = text[i];
                int chValue;

                if (ch <= 57 && ch >= 48)
                {
                    chValue = ch - 48;
                }
                else if (ch <= 70 && ch >= 65)
                {
                    chValue = ch - 55;
                }
                else if (ch <= 102 && ch >= 97)
                {
                    chValue = ch - 87;
                }
                else
                {
                    value = 0;
                    return false;
                }

                value += chValue << ((end - 1 - i) * 4);
            }

            return true;
        }
    }
}