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
using System.Linq;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;
using System.Reflection;

#if !SILVERLIGHT
using System.Data.SqlTypes;
#endif

namespace Newtonsoft.Json.Utilities
{
  internal static class ConvertUtils
  {
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
          return false;

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
        castMethodInfo = t.TargetType.GetMethod("op_Explicit", new[] { t.InitialType });

      if (castMethodInfo == null)
        return null;

      MethodCall<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(castMethodInfo);

      return o => call(null, o);
    }

    #region Convert
    /// <summary>
    /// Converts the value to the specified type.
    /// </summary>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <returns>The converted type.</returns>
    public static object Convert(object initialValue, CultureInfo culture, Type targetType)
    {
      if (initialValue == null)
        throw new ArgumentNullException("initialValue");

      if (ReflectionUtils.IsNullableType(targetType))
        targetType = Nullable.GetUnderlyingType(targetType);

      Type initialType = initialValue.GetType();

      if (targetType == initialType)
        return initialValue;

      if (initialValue is string && typeof(Type).IsAssignableFrom(targetType))
        return Type.GetType((string) initialValue, true);

      if (targetType.IsInterface || targetType.IsGenericTypeDefinition || targetType.IsAbstract)
        throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), "targetType");

      // use Convert.ChangeType if both types are IConvertible
      if (initialValue is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
      {
        if (targetType.IsEnum)
        {
          if (initialValue is string)
            return Enum.Parse(targetType, initialValue.ToString(), true);
          else if (IsInteger(initialValue))
            return Enum.ToObject(targetType, initialValue);
        }
        
        return System.Convert.ChangeType(initialValue, targetType, culture);
      }

#if !PocketPC && !NET20
      if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
        return new DateTimeOffset((DateTime)initialValue);
#endif

      if (initialValue is string)
      {
        if (targetType == typeof (Guid))
          return new Guid((string) initialValue);
        if (targetType == typeof (Uri))
          return new Uri((string) initialValue);
        if (targetType == typeof (TimeSpan))
#if !(NET35 || NET20 || SILVERLIGHT)
          return TimeSpan.Parse((string) initialValue, CultureInfo.InvariantCulture);
#else
          return TimeSpan.Parse((string)initialValue);
#endif
      }

#if !PocketPC
      // see if source or target types have a TypeConverter that converts between the two
      TypeConverter toConverter = GetConverter(initialType);

      if (toConverter != null && toConverter.CanConvertTo(targetType))
      {
#if !SILVERLIGHT
        return toConverter.ConvertTo(null, culture, initialValue, targetType);
#else
        return toConverter.ConvertTo(initialValue, targetType);
#endif
      }

      TypeConverter fromConverter = GetConverter(targetType);

      if (fromConverter != null && fromConverter.CanConvertFrom(initialType))
      {
#if !SILVERLIGHT
        return fromConverter.ConvertFrom(null, culture, initialValue);
#else
        return fromConverter.ConvertFrom(initialValue);
#endif
      }
#endif

      // handle DBNull and INullable
      if (initialValue == DBNull.Value)
      {
        if (ReflectionUtils.IsNullable(targetType))
          return EnsureTypeAssignable(null, initialType, targetType);
        
        throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, initialType, targetType));
      }
#if !SILVERLIGHT
      if (initialValue is INullable)
        return EnsureTypeAssignable(ToValue((INullable)initialValue), initialType, targetType);
#endif

      throw new Exception("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialType, targetType));
    }
    #endregion

    #region TryConvert
    /// <summary>
    /// Converts the value to the specified type.
    /// </summary>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
    {
      return MiscellaneousUtils.TryAction<object>(delegate { return Convert(initialValue, culture, targetType); }, out convertedValue);
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
        return initialValue;

      if (initialValue == null && ReflectionUtils.IsNullable(targetType))
        return null;

      if (TryConvert(initialValue, culture, targetType, out convertedValue))
        return convertedValue;

      return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
    }
    #endregion

    private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
    {
      Type valueType = (value != null) ? value.GetType() : null;

      if (value != null)
      {
        if (targetType.IsAssignableFrom(valueType))
          return value;

        Func<object, object> castConverter = CastConverters.Get(new TypeConvertKey(valueType, targetType));
        if (castConverter != null)
          return castConverter(value);
      }
      else
      {
        if (ReflectionUtils.IsNullable(targetType))
          return null;
      }

      throw new Exception("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, (initialType != null) ? initialType.ToString() : "{null}", targetType));
    }

#if !SILVERLIGHT
    public static object ToValue(INullable nullableValue)
    {
      if (nullableValue == null)
        return null;
      else if (nullableValue is SqlInt32)
        return ToValue((SqlInt32)nullableValue);
      else if (nullableValue is SqlInt64)
        return ToValue((SqlInt64)nullableValue);
      else if (nullableValue is SqlBoolean)
        return ToValue((SqlBoolean)nullableValue);
      else if (nullableValue is SqlString)
        return ToValue((SqlString)nullableValue);
      else if (nullableValue is SqlDateTime)
        return ToValue((SqlDateTime)nullableValue);

      throw new Exception("Unsupported INullable type: {0}".FormatWith(CultureInfo.InvariantCulture, nullableValue.GetType()));
    }
#endif

#if !PocketPC
    internal static TypeConverter GetConverter(Type t)
    {
      return JsonTypeReflector.GetTypeConverter(t);
    }
#endif

    public static bool IsInteger(object value)
    {
      switch (System.Convert.GetTypeCode(value))
      {
        case TypeCode.SByte:
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return true;
        default:
          return false;
      }
    }
  }
}
