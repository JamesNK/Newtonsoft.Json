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
using System.Globalization;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(NETFX_CORE || DNXCORE50)
    public struct Ratio : IConvertible, IFormattable, ISerializable
    {
        private readonly int _numerator;
        private readonly int _denominator;

        public Ratio(int numerator, int denominator)
        {
            _numerator = numerator;
            _denominator = denominator;
        }

        #region Properties
        public int Numerator
        {
            get { return _numerator; }
        }

        public int Denominator
        {
            get { return _denominator; }
        }

        public bool IsNan
        {
            get { return _denominator == 0; }
        }
        #endregion

        #region Serialization operations
        public Ratio(SerializationInfo info, StreamingContext context)
        {
            _numerator = info.GetInt32("n");
            _denominator = info.GetInt32("d");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("n", _numerator);
            info.AddValue("d", _denominator);
        }
        #endregion

        #region IConvertible Members
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return _numerator == 0;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return (byte)(_numerator / _denominator);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_numerator / _denominator);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_numerator / _denominator);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return (decimal)_numerator / _denominator;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return _denominator == 0
                ? double.NaN
                : (double)_numerator / _denominator;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return (short)(_numerator / _denominator);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _numerator / _denominator;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return _numerator / _denominator;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)(_numerator / _denominator);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return _denominator == 0
                ? float.NaN
                : (float)_numerator / _denominator;
        }

        public string ToString(IFormatProvider provider)
        {
            return _denominator == 1
                ? _numerator.ToString(provider)
                : _numerator.ToString(provider) + "/" + _denominator.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(ToDouble(provider), conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)(_numerator / _denominator);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)(_numerator / _denominator);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)(_numerator / _denominator);
        }
        #endregion

        #region String operations
        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        public static Ratio Parse(string input)
        {
            return Parse(input, CultureInfo.InvariantCulture);
        }

        public static Ratio Parse(string input, IFormatProvider formatProvider)
        {
            Ratio result;
            if (!TryParse(input, formatProvider, out result))
            {
                throw new FormatException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Text '{0}' is invalid text representation of ratio",
                        input));
            }
            return result;
        }

        public static bool TryParse(string input, out Ratio result)
        {
            return TryParse(input, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryParse(string input, IFormatProvider formatProvider, out Ratio result)
        {
            if (input != null)
            {
                var fractionIndex = input.IndexOf('/');

                int numerator;
                if (fractionIndex < 0)
                {
                    if (int.TryParse(input, NumberStyles.Integer, formatProvider, out numerator))
                    {
                        result = new Ratio(numerator, 1);
                        return true;
                    }
                }
                else
                {
                    int denominator;
                    if (int.TryParse(input.Substring(0, fractionIndex), NumberStyles.Integer, formatProvider, out numerator) &&
                        int.TryParse(input.Substring(fractionIndex + 1), NumberStyles.Integer, formatProvider, out denominator))
                    {
                        result = new Ratio(numerator, denominator);
                        return true;
                    }
                }
            }

            result = default(Ratio);
            return false;
        }
        #endregion
    }
#endif
}