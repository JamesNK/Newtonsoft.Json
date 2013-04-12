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
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal static class ValidationUtils
  {
    public static void ArgumentNotNullOrEmpty(string value, string parameterName)
    {
      if (value == null)
        throw new ArgumentNullException(parameterName);

      if (value.Length == 0)
        throw new ArgumentException("'{0}' cannot be empty.".FormatWith(CultureInfo.InvariantCulture, parameterName), parameterName);
    }

    public static void ArgumentTypeIsEnum(Type enumType, string parameterName)
    {
      ArgumentNotNull(enumType, "enumType");

      if (!enumType.IsEnum())
        throw new ArgumentException("Type {0} is not an Enum.".FormatWith(CultureInfo.InvariantCulture, enumType), parameterName);
    }

    public static void ArgumentNotNull(object value, string parameterName)
    {
      if (value == null)
        throw new ArgumentNullException(parameterName);
    }
  }
}