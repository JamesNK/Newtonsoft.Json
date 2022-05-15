#region License
// Copyright (c) 2022 James Newton-King
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

namespace Newtonsoft.Json.Utilities
{
    internal static class BoxedPrimitives
    {
        internal static object Get(bool value) => value ? BooleanTrue : BooleanFalse;

        internal static readonly object BooleanTrue = true;
        internal static readonly object BooleanFalse = false;

        internal static object Get(int value) => value switch
        {
            -1 => Int32_M1,
            0 => Int32_0,
            1 => Int32_1,
            2 => Int32_2,
            3 => Int32_3,
            4 => Int32_4,
            5 => Int32_5,
            6 => Int32_6,
            7 => Int32_7,
            8 => Int32_8,
            _ => value,
        };

        // integers tend to be weighted towards a handful of low numbers; we could argue
        // for days over the "correct" range to have special handling, but I'm arbitrarily
        // mirroring the same decision as the IL opcodes, which has M1 thru 8
        internal static readonly object Int32_M1 = -1;
        internal static readonly object Int32_0 = 0;
        internal static readonly object Int32_1 = 1;
        internal static readonly object Int32_2 = 2;
        internal static readonly object Int32_3 = 3;
        internal static readonly object Int32_4 = 4;
        internal static readonly object Int32_5 = 5;
        internal static readonly object Int32_6 = 6;
        internal static readonly object Int32_7 = 7;
        internal static readonly object Int32_8 = 8;

        internal static object Get(long value) => value switch
        {
            -1 => Int64_M1,
            0 => Int64_0,
            1 => Int64_1,
            2 => Int64_2,
            3 => Int64_3,
            4 => Int64_4,
            5 => Int64_5,
            6 => Int64_6,
            7 => Int64_7,
            8 => Int64_8,
            _ => value,
        };

        internal static readonly object Int64_M1 = -1L;
        internal static readonly object Int64_0 = 0L;
        internal static readonly object Int64_1 = 1L;
        internal static readonly object Int64_2 = 2L;
        internal static readonly object Int64_3 = 3L;
        internal static readonly object Int64_4 = 4L;
        internal static readonly object Int64_5 = 5L;
        internal static readonly object Int64_6 = 6L;
        internal static readonly object Int64_7 = 7L;
        internal static readonly object Int64_8 = 8L;

        internal static object Get(decimal value) => value == decimal.Zero ? DecimalZero : value;

        private static readonly object DecimalZero = decimal.Zero;

        internal static object Get(double value)
        {
            if (value == 0.0d)
            {
                return DoubleZero;
            }
            if (double.IsInfinity(value))
            {
                return double.IsPositiveInfinity(value) ? DoublePositiveInfinity : DoubleNegativeInfinity;
            }
            if (double.IsNaN(value))
            {
                return DoubleNaN;
            }
            return value;
        }

        internal static readonly object DoubleNaN = double.NaN;
        internal static readonly object DoublePositiveInfinity = double.PositiveInfinity;
        internal static readonly object DoubleNegativeInfinity = double.NegativeInfinity;
        internal static readonly object DoubleZero = (double)0;
    }
}
