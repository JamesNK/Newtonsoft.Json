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
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;

namespace Newtonsoft.Json.Utilities
{
  internal static class EnumUtils
  {
    public static IList<T> GetFlagsValues<T>(T value) where T : struct
    {
      Type enumType = typeof(T);

      if (!enumType.IsDefined(typeof(FlagsAttribute), false))
        throw new ArgumentException("Enum type {0} is not a set of flags.".FormatWith(CultureInfo.InvariantCulture, enumType));

      Type underlyingType = Enum.GetUnderlyingType(value.GetType());

      ulong num = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
      EnumValues<ulong> enumNameValues = GetNamesAndValues<T>();
      IList<T> selectedFlagsValues = new List<T>();

      foreach (EnumValue<ulong> enumNameValue in enumNameValues)
      {
        if ((num & enumNameValue.Value) == enumNameValue.Value && enumNameValue.Value != 0)
          selectedFlagsValues.Add((T)Convert.ChangeType(enumNameValue.Value, underlyingType, CultureInfo.CurrentCulture));
      }

      if (selectedFlagsValues.Count == 0 && enumNameValues.SingleOrDefault(v => v.Value == 0) != null)
        selectedFlagsValues.Add(default(T));

      return selectedFlagsValues;
    }

    /// <summary>
    /// Gets a dictionary of the names and values of an Enum type.
    /// </summary>
    /// <returns></returns>
    public static EnumValues<ulong> GetNamesAndValues<T>() where T : struct
    {
      return GetNamesAndValues<ulong>(typeof(T));
    }

    /// <summary>
    /// Gets a dictionary of the names and values of an Enum type.
    /// </summary>
    /// <param name="enumType">The enum type to get names and values for.</param>
    /// <returns></returns>
    public static EnumValues<TUnderlyingType> GetNamesAndValues<TUnderlyingType>(Type enumType) where TUnderlyingType : struct
    {
      if (enumType == null)
        throw new ArgumentNullException("enumType");

      ValidationUtils.ArgumentTypeIsEnum(enumType, "enumType");

      IList<object> enumValues = GetValues(enumType);
      IList<string> enumNames = GetNames(enumType);

      EnumValues<TUnderlyingType> nameValues = new EnumValues<TUnderlyingType>();

      for (int i = 0; i < enumValues.Count; i++)
      {
        try
        {
          nameValues.Add(new EnumValue<TUnderlyingType>(enumNames[i], (TUnderlyingType)Convert.ChangeType(enumValues[i], typeof(TUnderlyingType), CultureInfo.CurrentCulture)));
        }
        catch (OverflowException e)
        {
          throw new InvalidOperationException(
            string.Format(CultureInfo.InvariantCulture, "Value from enum with the underlying type of {0} cannot be added to dictionary with a value type of {1}. Value was too large: {2}",
              Enum.GetUnderlyingType(enumType), typeof(TUnderlyingType), Convert.ToUInt64(enumValues[i], CultureInfo.InvariantCulture)), e);
        }
      }

      return nameValues;
    }

    public static IList<object> GetValues(Type enumType)
    {
      if (!enumType.IsEnum())
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

      List<object> values = new List<object>();

      var fields = from field in enumType.GetFields()
                   where field.IsLiteral
                   select field;

      foreach (FieldInfo field in fields)
      {
        object value = field.GetValue(enumType);
        values.Add(value);
      }

      return values;
    }

    public static IList<string> GetNames(Type enumType)
    {
      if (!enumType.IsEnum())
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum.");

      List<string> values = new List<string>();

      var fields = from field in enumType.GetFields()
                   where field.IsLiteral
                   select field;

      foreach (FieldInfo field in fields)
      {
        values.Add(field.Name);
      }

      return values;
    }
  }
}