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
#if !SILVERLIGHT
using System.Data.SqlTypes;
#endif

namespace Newtonsoft.Json.Utilities
{
  internal static class ConvertUtils
  {
    public static bool CanConvertType(Type initialType, Type targetType, bool allowTypeNameToString)
    {
      ValidationUtils.ArgumentNotNull(initialType, "initialType");
      ValidationUtils.ArgumentNotNull(targetType, "targetType");

      if (ReflectionUtils.IsNullableType(targetType))
        targetType = Nullable.GetUnderlyingType(targetType);

      if (targetType == initialType)
        return true;

      if (typeof(IConvertible).IsAssignableFrom(initialType) && typeof(IConvertible).IsAssignableFrom(targetType))
      {
        return true;
      }

      if (initialType == typeof(DateTime) && targetType == typeof(DateTimeOffset))
        return true;

      if (initialType == typeof(Guid) && (targetType == typeof(Guid) || targetType == typeof(string)))
        return true;

      if (initialType == typeof(Type) && targetType == typeof(string))
        return true;

#if !PocketPC
      // see if source or target types have a TypeConverter that converts between the two
      TypeConverter toConverter = GetConverter(initialType);

      if (toConverter != null && !IsComponentConverter(toConverter) && toConverter.CanConvertTo(targetType))
      {
        if (allowTypeNameToString || toConverter.GetType() != typeof(TypeConverter))
          return true;
      }

      TypeConverter fromConverter = GetConverter(targetType);

      if (fromConverter != null && !IsComponentConverter(fromConverter) && fromConverter.CanConvertFrom(initialType))
        return true;
#endif

      // handle DBNull and INullable
      if (initialType == typeof(DBNull))
      {
        if (ReflectionUtils.IsNullable(targetType))
          return true;
      }

      return false;
    }

    private static bool IsComponentConverter(TypeConverter converter)
    {
#if !SILVERLIGHT && !PocketPC
      return (converter is ComponentConverter);
#else
      return false;
#endif
    }

    #region Convert
    /// <summary>
    /// Converts the value to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <returns>The converted type.</returns>
    public static T Convert<T>(object initialValue)
    {
      return Convert<T>(initialValue, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Converts the value to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <returns>The converted type.</returns>
    public static T Convert<T>(object initialValue, CultureInfo culture)
    {
      return (T)Convert(initialValue, culture, typeof(T));
    }

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

      if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
        return new DateTimeOffset((DateTime)initialValue);

      if (initialValue is string)
      {
        if (targetType == typeof (Guid))
          return new Guid((string) initialValue);
        if (targetType == typeof (Uri))
          return new Uri((string) initialValue);
        if (targetType == typeof (TimeSpan))
          return TimeSpan.Parse((string) initialValue);
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
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvert<T>(object initialValue, out T convertedValue)
    {
      return TryConvert(initialValue, CultureInfo.CurrentCulture, out convertedValue);
    }

    /// <summary>
    /// Converts the value to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvert<T>(object initialValue, CultureInfo culture, out T convertedValue)
    {
      return MiscellaneousUtils.TryAction<T>(delegate
      {
        object tempConvertedValue;
        TryConvert(initialValue, CultureInfo.CurrentCulture, typeof(T), out tempConvertedValue);

        return (T)tempConvertedValue;
      }, out convertedValue);
    }

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
    /// <typeparam name="T">The type to convert or cast the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <returns>The converted type. If conversion was unsuccessful, the initial value is returned if assignable to the target type</returns>
    public static T ConvertOrCast<T>(object initialValue)
    {
      return ConvertOrCast<T>(initialValue, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Converts the value to the specified type. If the value is unable to be converted, the
    /// value is checked whether it assignable to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert or cast the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <returns>The converted type. If conversion was unsuccessful, the initial value is returned if assignable to the target type</returns>
    public static T ConvertOrCast<T>(object initialValue, CultureInfo culture)
    {
      return (T)ConvertOrCast(initialValue, culture, typeof(T));
    }

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
      if (TryConvert(initialValue, culture, targetType, out convertedValue))
        return convertedValue;

      return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
    }
    #endregion

    #region TryConvertOrCast
    /// <summary>
    /// Converts the value to the specified type. If the value is unable to be converted, the
    /// value is checked whether it assignable to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvertOrCast<T>(object initialValue, out T convertedValue)
    {
      return TryConvertOrCast<T>(initialValue, CultureInfo.CurrentCulture, out convertedValue);
    }

    /// <summary>
    /// Converts the value to the specified type. If the value is unable to be converted, the
    /// value is checked whether it assignable to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvertOrCast<T>(object initialValue, CultureInfo culture, out T convertedValue)
    {
      return MiscellaneousUtils.TryAction<T>(delegate
      {
        object tempConvertedValue;
        TryConvertOrCast(initialValue, CultureInfo.CurrentCulture, typeof(T), out tempConvertedValue);

        return (T)tempConvertedValue;
      }, out convertedValue);
    }

    /// <summary>
    /// Converts the value to the specified type. If the value is unable to be converted, the
    /// value is checked whether it assignable to the specified type.
    /// </summary>
    /// <param name="initialValue">The value to convert.</param>
    /// <param name="culture">The culture to use when converting.</param>
    /// <param name="targetType">The type to convert the value to.</param>
    /// <param name="convertedValue">The converted value if the conversion was successful or the default value of <c>T</c> if it failed.</param>
    /// <returns>
    /// 	<c>true</c> if <c>initialValue</c> was converted successfully or is assignable; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryConvertOrCast(object initialValue, CultureInfo culture, Type targetType, out object convertedValue)
    {
      return MiscellaneousUtils.TryAction<object>(delegate { return ConvertOrCast(initialValue, culture, targetType); }, out convertedValue);
    }
    #endregion

    private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
    {
      Type valueType = (value != null) ? value.GetType() : null;

      if (value != null && targetType.IsAssignableFrom(valueType))
        return value;
      else if (value == null && ReflectionUtils.IsNullable(targetType))
        return null;
      else
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
    private static TypeConverter GetConverter(Type t)
    {
      ValidationUtils.ArgumentNotNull(t, "t");

#if !SILVERLIGHT
      return TypeDescriptor.GetConverter(t);
#else
      object[] customAttributes = t.GetCustomAttributes(typeof(TypeConverterAttribute), true);
      if (customAttributes.Length != 1)
        return null;

      TypeConverterAttribute typeConverterAttribute = (TypeConverterAttribute)customAttributes[0];
      return (Activator.CreateInstance(Type.GetType(typeConverterAttribute.ConverterTypeName)) as TypeConverter);
#endif
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
