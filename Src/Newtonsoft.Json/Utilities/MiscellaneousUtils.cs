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
    public static bool ValueEquals(object objA, object objB)
    {
      if (objA == null && objB == null)
        return true;
      if (objA != null && objB == null)
        return false;
      if (objA == null && objB != null)
        return false;

      // comparing an Int32 and Int64 both of the same value returns false
      // make types the same then compare
      if (objA.GetType() != objB.GetType())
      {
        if (ConvertUtils.IsInteger(objA) && ConvertUtils.IsInteger(objB))
          return Convert.ToDecimal(objA, CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, CultureInfo.CurrentCulture));
        else if ((objA is double || objA is float || objA is decimal) && (objB is double || objB is float || objB is decimal))
          return MathUtils.ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
        else
          return false;
      }

      return objA.Equals(objB);
    }

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

    public static byte[] HexToBytes(string hex)
    {
      string fixedHex = hex.Replace("-", string.Empty);

      // array to put the result in
      byte[] bytes = new byte[fixedHex.Length / 2];
      // variable to determine shift of high/low nibble
      int shift = 4;
      // offset of the current byte in the array
      int offset = 0;
      // loop the characters in the string
      foreach (char c in fixedHex)
      {
        // get character code in range 0-9, 17-22
        // the % 32 handles lower case characters
        int b = (c - '0') % 32;
        // correction for a-f
        if (b > 9) b -= 7;
        // store nibble (4 bits) in byte array
        bytes[offset] |= (byte)(b << shift);
        // toggle the shift variable between 0 and 4
        shift ^= 4;
        // move to next byte
        if (shift != 0) offset++;
      }
      return bytes;
    }

    public static string BytesToHex(byte[] bytes)
    {
      return BytesToHex(bytes, false);
    }

    public static string BytesToHex(byte[] bytes, bool removeDashes)
    {
      string hex = BitConverter.ToString(bytes);
      if (removeDashes)
        hex = hex.Replace("-", "");

      return hex;
    }

    public static int ByteArrayCompare(byte[] a1, byte[] a2)
    {
      int lengthCompare = a1.Length.CompareTo(a2.Length);
      if (lengthCompare != 0)
        return lengthCompare;

      for (int i = 0; i < a1.Length; i++)
      {
        int valueCompare = a1[i].CompareTo(a2[i]);
        if (valueCompare != 0)
          return valueCompare;
      }

      return 0;
    }

    public static string GetPrefix(string qualifiedName)
    {
      string prefix;
      string localName;
      GetQualifiedNameParts(qualifiedName, out prefix, out localName);

      return prefix;
    }

    public static string GetLocalName(string qualifiedName)
    {
      string prefix;
      string localName;
      GetQualifiedNameParts(qualifiedName, out prefix, out localName);

      return localName;
    }

    public static void GetQualifiedNameParts(string qualifiedName, out string prefix, out string localName)
    {
      int colonPosition = qualifiedName.IndexOf(':');

      if ((colonPosition == -1 || colonPosition == 0) || (qualifiedName.Length - 1) == colonPosition)
      {
        prefix = null;
        localName = qualifiedName;
      }
      else
      {
        prefix = qualifiedName.Substring(0, colonPosition);
        localName = qualifiedName.Substring(colonPosition + 1);
      }
    }
  }
}