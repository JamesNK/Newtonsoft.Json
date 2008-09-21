using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal delegate T Creator<T>();

  internal static class MiscellaneousUtils
  {
    public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
    {
      string newMessage = message + Environment.NewLine + @"Actual value was {0}.".FormatWith(CultureInfo.InvariantCulture, actualValue);

      return new ArgumentOutOfRangeException(paramName, newMessage);
    }

    public static bool TryAction<T>(Creator<T> creator, out T output)
    {
      ValidationUtils.ArgumentNotNull(creator, "creator");

      try
      {
        output = creator();
        return true;
      }
      catch
      {
        output = default(T);
        return false;
      }
    }

    public static string ToString(object value)
    {
      if (value == null)
        return "{null}";

      return (value is string) ? @"""" + value.ToString() + @"""" : value.ToString();
    }
  }
}
