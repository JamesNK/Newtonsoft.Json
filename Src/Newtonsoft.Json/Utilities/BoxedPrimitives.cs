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
        internal static readonly object BooleanTrue = true, BooleanFalse = false;

        internal static object Get(int value)
        {
            switch (value)
            {
                case -1:
                case 0:
                case 1:
                case 2:
                case 3:
                    return Int32Small[value + 1];
                default:
                    return value;
            }
        }
        private static readonly object[] Int32Small = new object[] { -1, 0, 1, 2, 3 };

        internal static object Get(long value)
        {
            switch (value)
            {
                case -1:
                case 0:
                case 1:
                case 2:
                case 3:
                    return Int64Small[(int)value + 1];
                default:
                    return value;
            }
        }
        private static readonly object[] Int64Small = new object[] { -1L, 0L, 1L, 2L, 3L };

        internal static object Get(decimal value) => value == decimal.Zero ? DecimalZero : value;
        private static readonly object DecimalZero = decimal.Zero;

        internal static object Get(double value)
        {
            if (value == 0.0d) return DoubleZero;
            if (double.IsInfinity(value)) return double.IsPositiveInfinity(value) ? DoublePositiveInfinity : DoubleNegativeInfinity;
            if (double.IsNaN(value)) return DoubleNaN;
            return value;
        }
        internal static readonly object DoubleNaN = double.NaN, DoublePositiveInfinity = double.PositiveInfinity, DoubleNegativeInfinity = double.NegativeInfinity, DoubleZero = (double)0;

#if HAVE_BIG_INTEGER
        internal static object Get(System.Numerics.BigInteger value) => value == System.Numerics.BigInteger.Zero ? BigIntegerZero : value;
        private static readonly object BigIntegerZero = System.Numerics.BigInteger.Zero;
#endif
    }
}
