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
using System.Text;

namespace Newtonsoft.Json.Utilities
{
  internal static class MathUtils
  {
    public static int IntLength(ulong i)
    {
      if (i < 10000000000)
      {
        if (i < 10) return 1;
        if (i < 100) return 2;
        if (i < 1000) return 3;
        if (i < 10000) return 4;
        if (i < 100000) return 5;
        if (i < 1000000) return 6;
        if (i < 10000000) return 7;
        if (i < 100000000) return 8;
        if (i < 1000000000) return 9;

        return 10;
      }
      else
      {
        if (i < 100000000000) return 11;
        if (i < 1000000000000) return 12;
        if (i < 10000000000000) return 13;
        if (i < 100000000000000) return 14;
        if (i < 1000000000000000) return 15;
        if (i < 10000000000000000) return 16;
        if (i < 100000000000000000) return 17;
        if (i < 1000000000000000000) return 18;
        if (i < 10000000000000000000) return 19;

        return 20;
      }
    }

    public static char IntToHex(int n)
    {
      if (n <= 9)
        return (char)(n + 48);

      return (char)((n - 10) + 97);
    }

    public static int? Min(int? val1, int? val2)
    {
      if (val1 == null)
        return val2;
      if (val2 == null)
        return val1;

      return Math.Min(val1.Value, val2.Value);
    }

    public static int? Max(int? val1, int? val2)
    {
      if (val1 == null)
        return val2;
      if (val2 == null)
        return val1;

      return Math.Max(val1.Value, val2.Value);
    }

    public static double? Max(double? val1, double? val2)
    {
      if (val1 == null)
        return val2;
      if (val2 == null)
        return val1;

      return Math.Max(val1.Value, val2.Value);
    }

    public static bool ApproxEquals(double d1, double d2)
    {
      const double epsilon = 2.2204460492503131E-16;

      if (d1 == d2)
        return true;

      double tolerance = ((Math.Abs(d1) + Math.Abs(d2)) + 10.0) * epsilon;
      double difference = d1 - d2;

      return (-tolerance < difference && tolerance > difference);
    }
  }
}