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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json.Utilities
{
  internal static class ValidationUtils
  {
    public const string EmailAddressRegex = @"^([a-zA-Z0-9_'+*$%\^&!\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9:]{2,4})+$";
    public const string CurrencyRegex = @"(^\$?(?!0,?\d)\d{1,3}(,?\d{3})*(\.\d\d)?)$";
    public const string DateRegex =
        @"^(((0?[1-9]|[12]\d|3[01])[\.\-\/](0?[13578]|1[02])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}|\d))|((0?[1-9]|[12]\d|30)[\.\-\/](0?[13456789]|1[012])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}|\d))|((0?[1-9]|1\d|2[0-8])[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}|\d))|(29[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00|[048])))$";
    public const string NumericRegex = @"\d*";

    public static void ArgumentNotNullOrEmpty(string value, string parameterName)
    {
      if (value == null)
        throw new ArgumentNullException(parameterName);

      if (value.Length == 0)
        throw new ArgumentException("'{0}' cannot be empty.".FormatWith(parameterName), parameterName);
    }

    public static void ArgumentNotNullOrEmptyOrWhitespace(string value, string parameterName)
    {
      ArgumentNotNullOrEmpty(value, parameterName);

      if (StringUtils.IsWhiteSpace(value))
        throw new ArgumentException("'{0}' cannot only be whitespace.".FormatWith(parameterName), parameterName);
    }

    public static void ArgumentTypeIsEnum(Type enumType, string parameterName)
    {
      ArgumentNotNull(enumType, "enumType");

      if (!enumType.IsEnum)
        throw new ArgumentException("Type {0} is not an Enum.".FormatWith(enumType), parameterName);
    }

    public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName)
    {
      ArgumentNotNullOrEmpty<T>(collection, parameterName, "Collection '{0}' cannot be empty.".FormatWith(parameterName));
    }

    public static void ArgumentNotNullOrEmpty<T>(ICollection<T> collection, string parameterName, string message)
    {
      if (collection == null)
        throw new ArgumentNullException(parameterName);

      if (collection.Count == 0)
        throw new ArgumentException(message, parameterName);
    }

    public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName)
    {
      ArgumentNotNullOrEmpty(collection, parameterName, "Collection '{0}' cannot be empty.".FormatWith(parameterName));
    }

    public static void ArgumentNotNullOrEmpty(ICollection collection, string parameterName, string message)
    {
      if (collection == null)
        throw new ArgumentNullException(parameterName);

      if (collection.Count == 0)
        throw new ArgumentException(message, parameterName);
    }

    public static void ArgumentNotNull(object value, string parameterName)
    {
      if (value == null)
        throw new ArgumentNullException(parameterName);
    }

    public static void ArgumentNotNegative(int value, string parameterName)
    {
      if (value <= 0)
        throw new ArgumentOutOfRangeException(parameterName, value, "Argument cannot be negative.");
    }

    public static void ArgumentNotNegative(int value, string parameterName, string message)
    {
      if (value <= 0)
        throw new ArgumentOutOfRangeException(parameterName, value, message);
    }

    public static void ArgumentNotZero(int value, string parameterName)
    {
      if (value == 0)
        throw new ArgumentOutOfRangeException(parameterName, value, "Argument cannot be zero.");
    }

    public static void ArgumentNotZero(int value, string parameterName, string message)
    {
      if (value == 0)
        throw new ArgumentOutOfRangeException(parameterName, value, message);
    }

    public static void ArgumentIsPositive<T>(T value, string parameterName) where T : struct, IComparable<T>
    {
      if (value.CompareTo(default(T)) != 1)
        throw new ArgumentOutOfRangeException(parameterName, value, "Positive number required.");
    }

    public static void ArgumentIsPositive(int value, string parameterName, string message)
    {
      if (value > 0)
        throw new ArgumentOutOfRangeException(parameterName, value, message);
    }

    public static void ObjectNotDisposed(bool disposed, Type objectType)
    {
      if (disposed)
        throw new ObjectDisposedException(objectType.Name);
    }

    public static void ArgumentConditionTrue(bool condition, string parameterName, string message)
    {
      if (!condition)
        throw new ArgumentException(message, parameterName);
    }
  }
}