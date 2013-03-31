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
#if !(PORTABLE || NET35 || NET20 || WINDOWS_PHONE || SILVERLIGHT)
using System.Numerics;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonPrimitiveContract : JsonContract
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
#if !(PORTABLE || NET35 || NET20 || WINDOWS_PHONE || SILVERLIGHT)
          { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
          { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
#endif
          { typeof(Uri), PrimitiveTypeCode.Uri },
          { typeof(string), PrimitiveTypeCode.String },
          { typeof(byte[]), PrimitiveTypeCode.Bytes },
#if !(PORTABLE || NETFX_CORE)
          { typeof(DBNull), PrimitiveTypeCode.DBNull }
#endif
        };

    internal PrimitiveTypeCode TypeCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPrimitiveContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonPrimitiveContract(Type underlyingType)
      : base(underlyingType)
    {
      ContractType = JsonContractType.Primitive;

      // get the underlying enum value
      Type t;
      if (ReflectionUtils.IsNullableType(underlyingType))
        t = Nullable.GetUnderlyingType(underlyingType).IsEnum() ? ReflectionUtils.MakeGenericType(typeof(Nullable<>), Enum.GetUnderlyingType(Nullable.GetUnderlyingType(underlyingType))) : underlyingType;
      else
        t = underlyingType.IsEnum() ? Enum.GetUnderlyingType(underlyingType) : underlyingType;

      PrimitiveTypeCode typeCode;
      TypeCodeMap.TryGetValue(t, out typeCode);
      TypeCode = typeCode;
    }

    internal void WriteValue(JsonWriter writer, object value)
    {
      switch (TypeCode)
      {
        case PrimitiveTypeCode.Char:
          writer.WriteValue((char)value);
          break;
        case PrimitiveTypeCode.CharNullable:
          if (value != null)
            writer.WriteValue((char)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Boolean:
          writer.WriteValue((bool)value);
          break;
        case PrimitiveTypeCode.BooleanNullable:
          if (value != null)
            writer.WriteValue((bool)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.SByte:
          writer.WriteValue((sbyte)value);
          break;
        case PrimitiveTypeCode.SByteNullable:
          if (value != null)
            writer.WriteValue((sbyte)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Int16:
          writer.WriteValue((short)value);
          break;
        case PrimitiveTypeCode.Int16Nullable:
          if (value != null)
            writer.WriteValue((short)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.UInt16:
          writer.WriteValue((ushort)value);
          break;
        case PrimitiveTypeCode.UInt16Nullable:
          if (value != null)
            writer.WriteValue((ushort)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Int32:
          writer.WriteValue((int)value);
          break;
        case PrimitiveTypeCode.Int32Nullable:
          if (value != null)
            writer.WriteValue((int)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Byte:
          writer.WriteValue((byte)value);
          break;
        case PrimitiveTypeCode.ByteNullable:
          if (value != null)
            writer.WriteValue((byte)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.UInt32:
          writer.WriteValue((uint)value);
          break;
        case PrimitiveTypeCode.UInt32Nullable:
          if (value != null)
            writer.WriteValue((uint)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Int64:
          writer.WriteValue((long)value);
          break;
        case PrimitiveTypeCode.Int64Nullable:
          if (value != null)
            writer.WriteValue((long)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.UInt64:
          writer.WriteValue((ulong)value);
          break;
        case PrimitiveTypeCode.UInt64Nullable:
          if (value != null)
            writer.WriteValue((ulong)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Single:
          writer.WriteValue((float)value);
          break;
        case PrimitiveTypeCode.SingleNullable:
          // use nullable WriteValue to handle NaN, Infinity with FloatFormatHandling
          if (value != null)
            writer.WriteValue((float?)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Double:
          writer.WriteValue((double)value);
          break;
        case PrimitiveTypeCode.DoubleNullable:
          // use nullable WriteValue to handle NaN, Infinity with FloatFormatHandling
          if (value != null)
            writer.WriteValue((double?)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.DateTime:
          writer.WriteValue((DateTime)value);
          break;
        case PrimitiveTypeCode.DateTimeNullable:
          if (value != null)
            writer.WriteValue((DateTime)value);
          else
            writer.WriteNull();
          break;
#if !NET20
        case PrimitiveTypeCode.DateTimeOffset:
          writer.WriteValue((DateTimeOffset)value);
          break;
        case PrimitiveTypeCode.DateTimeOffsetNullable:
          if (value != null)
            writer.WriteValue((DateTimeOffset)value);
          else
            writer.WriteNull();
          break;
#endif
        case PrimitiveTypeCode.Decimal:
          writer.WriteValue((decimal)value);
          break;
        case PrimitiveTypeCode.DecimalNullable:
          if (value != null)
            writer.WriteValue((decimal)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.Guid:
          writer.WriteValue((Guid)value);
          break;
        case PrimitiveTypeCode.GuidNullable:
          if (value != null)
            writer.WriteValue((Guid)value);
          else
            writer.WriteNull();
          break;
        case PrimitiveTypeCode.TimeSpan:
          writer.WriteValue((TimeSpan)value);
          break;
        case PrimitiveTypeCode.TimeSpanNullable:
          if (value != null)
            writer.WriteValue((TimeSpan)value);
          else
            writer.WriteNull();
          break;
#if !(PORTABLE || NET35 || NET20 || WINDOWS_PHONE || SILVERLIGHT)
        case PrimitiveTypeCode.BigInteger:
          writer.WriteValue((BigInteger)value);
          break;
        case PrimitiveTypeCode.BigIntegerNullable:
          if (value != null)
            writer.WriteValue((BigInteger)value);
          else
            writer.WriteNull();
          break;
#endif
        case PrimitiveTypeCode.Uri:
          writer.WriteValue((Uri)value);
          break;
        case PrimitiveTypeCode.String:
          writer.WriteValue((string)value);
          break;
        case PrimitiveTypeCode.Bytes:
          writer.WriteValue((byte[])value);
          break;
#if !(PORTABLE || NETFX_CORE)
        case PrimitiveTypeCode.DBNull:
          writer.WriteNull();
          break;
#endif
        default:
          throw new ArgumentOutOfRangeException("Could not write type: " + UnderlyingType);
      }
    }
  }

  internal enum PrimitiveTypeCode
  {
    Unknown,
    Char,
    CharNullable,
    Boolean,
    BooleanNullable,
    SByte,
    SByteNullable,
    Int16,
    Int16Nullable,
    UInt16,
    UInt16Nullable,
    Int32,
    Int32Nullable,
    Byte,
    ByteNullable,
    UInt32,
    UInt32Nullable,
    Int64,
    Int64Nullable,
    UInt64,
    UInt64Nullable,
    Single,
    SingleNullable,
    Double,
    DoubleNullable,
    DateTime,
    DateTimeNullable,
#if !NET20
    DateTimeOffset,
    DateTimeOffsetNullable,
#endif
    Decimal,
    DecimalNullable,
    Guid,
    GuidNullable,
    TimeSpan,
    TimeSpanNullable,
#if !(PORTABLE || NET35 || NET20 || WINDOWS_PHONE || SILVERLIGHT)
    BigInteger,
    BigIntegerNullable,
#endif
    Uri,
    String,
    Bytes,
    DBNull
  }
}