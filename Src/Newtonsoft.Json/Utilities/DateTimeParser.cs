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

namespace Newtonsoft.Json.Utilities
{
    internal enum ParserTimeZone
    {
        Unspecified = 0,
        Utc = 1,
        LocalWestOfUtc = 2,
        LocalEastOfUtc = 3
    }

    internal struct DateTimeParser
    {
        static DateTimeParser()
        {
            Power10 = new[] { -1, 10, 100, 1000, 10000, 100000, 1000000 };

            Lzyyyy = "yyyy".Length;
            Lzyyyy_ = "yyyy-".Length;
            Lzyyyy_MM = "yyyy-MM".Length;
            Lzyyyy_MM_ = "yyyy-MM-".Length;
            Lzyyyy_MM_dd = "yyyy-MM-dd".Length;
            Lzyyyy_MM_ddT = "yyyy-MM-ddT".Length;
            LzHH = "HH".Length;
            LzHH_ = "HH:".Length;
            LzHH_mm = "HH:mm".Length;
            LzHH_mm_ = "HH:mm:".Length;
            LzHH_mm_ss = "HH:mm:ss".Length;
            Lz_ = "-".Length;
            Lz_zz = "-zz".Length;
        }

        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public int Fraction;
        public int ZoneHour;
        public int ZoneMinute;
        public ParserTimeZone Zone;

        private string _text;
        private int _length;

        private static readonly int[] Power10;

        private static readonly int Lzyyyy;
        private static readonly int Lzyyyy_;
        private static readonly int Lzyyyy_MM;
        private static readonly int Lzyyyy_MM_;
        private static readonly int Lzyyyy_MM_dd;
        private static readonly int Lzyyyy_MM_ddT;
        private static readonly int LzHH;
        private static readonly int LzHH_;
        private static readonly int LzHH_mm;
        private static readonly int LzHH_mm_;
        private static readonly int LzHH_mm_ss;
        private static readonly int Lz_;
        private static readonly int Lz_zz;

        private const short MaxFractionDigits = 7;

        public bool Parse(string text)
        {
            _text = text;
            _length = text.Length;

            if (ParseDate(0) && ParseChar(Lzyyyy_MM_dd, 'T') && ParseTimeAndZoneAndWhitespace(Lzyyyy_MM_ddT))
                return true;

            return false;
        }

        private bool ParseDate(int start)
        {
            return (Parse4Digit(start, out Year)
                    && 1 <= Year
                    && ParseChar(start + Lzyyyy, '-')
                    && Parse2Digit(start + Lzyyyy_, out Month)
                    && 1 <= Month
                    && Month <= 12
                    && ParseChar(start + Lzyyyy_MM, '-')
                    && Parse2Digit(start + Lzyyyy_MM_, out Day)
                    && 1 <= Day
                    && Day <= DateTime.DaysInMonth(Year, Month));
        }

        private bool ParseTimeAndZoneAndWhitespace(int start)
        {
            return (ParseTime(ref start) && ParseZone(start));
        }

        private bool ParseTime(ref int start)
        {
            if (!(Parse2Digit(start, out Hour)
                  && Hour < 24
                  && ParseChar(start + LzHH, ':')
                  && Parse2Digit(start + LzHH_, out Minute)
                  && Minute < 60
                  && ParseChar(start + LzHH_mm, ':')
                  && Parse2Digit(start + LzHH_mm_, out Second)
                  && Second < 60))
            {
                return false;
            }

            start += LzHH_mm_ss;
            if (ParseChar(start, '.'))
            {
                Fraction = 0;
                int numberOfDigits = 0;

                while (++start < _length && numberOfDigits < MaxFractionDigits)
                {
                    int digit = _text[start] - '0';
                    if (digit < 0 || digit > 9)
                        break;

                    Fraction = (Fraction * 10) + digit;

                    numberOfDigits++;
                }

                if (numberOfDigits < MaxFractionDigits)
                {
                    if (numberOfDigits == 0)
                        return false;

                    Fraction *= Power10[MaxFractionDigits - numberOfDigits];
                }
            }
            return true;
        }

        private bool ParseZone(int start)
        {
            if (start < _length)
            {
                char ch = _text[start];
                if (ch == 'Z' || ch == 'z')
                {
                    Zone = ParserTimeZone.Utc;
                    start++;
                }
                else
                {
                    if (start + 2 < _length
                        && Parse2Digit(start + Lz_, out ZoneHour)
                        && ZoneHour <= 99)
                    {
                        switch (ch)
                        {
                            case '-':
                                Zone = ParserTimeZone.LocalWestOfUtc;
                                start += Lz_zz;
                                break;

                            case '+':
                                Zone = ParserTimeZone.LocalEastOfUtc;
                                start += Lz_zz;
                                break;
                        }
                    }

                    if (start < _length)
                    {
                        if (ParseChar(start, ':'))
                        {
                            start += 1;

                            if (start + 1 < _length
                                && Parse2Digit(start, out ZoneMinute)
                                && ZoneMinute <= 99)
                            {
                                start += 2;
                            }
                        }
                        else
                        {
                            if (start + 1 < _length
                                && Parse2Digit(start, out ZoneMinute)
                                && ZoneMinute <= 99)
                            {
                                start += 2;
                            }
                        }
                    }
                }
            }

            return (start == _length);
        }

        private bool Parse4Digit(int start, out int num)
        {
            if (start + 3 < _length)
            {
                int digit1 = _text[start] - '0';
                int digit2 = _text[start + 1] - '0';
                int digit3 = _text[start + 2] - '0';
                int digit4 = _text[start + 3] - '0';
                if (0 <= digit1 && digit1 < 10
                    && 0 <= digit2 && digit2 < 10
                    && 0 <= digit3 && digit3 < 10
                    && 0 <= digit4 && digit4 < 10)
                {
                    num = (((((digit1 * 10) + digit2) * 10) + digit3) * 10) + digit4;
                    return true;
                }
            }
            num = 0;
            return false;
        }

        private bool Parse2Digit(int start, out int num)
        {
            if (start + 1 < _length)
            {
                int digit1 = _text[start] - '0';
                int digit2 = _text[start + 1] - '0';
                if (0 <= digit1 && digit1 < 10
                    && 0 <= digit2 && digit2 < 10)
                {
                    num = (digit1 * 10) + digit2;
                    return true;
                }
            }
            num = 0;
            return false;
        }

        private bool ParseChar(int start, char ch)
        {
            return (start < _length && _text[start] == ch);
        }
    }
}