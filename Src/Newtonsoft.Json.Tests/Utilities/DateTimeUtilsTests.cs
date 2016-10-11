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
using System.IO;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Utilities
{
    [TestFixture]
    public class DateTimeUtilsTests : TestFixtureBase
    {
        [Test]
        public void RoundTripDateTimeMinAndMax()
        {
            RoundtripDateIso(DateTime.MinValue);
            RoundtripDateIso(DateTime.MaxValue);
        }

        private static StringReference CreateStringReference(string s)
        {
            return new StringReference(s.ToCharArray(), 0, s.Length);
        }

        private static void RoundtripDateIso(DateTime value)
        {
            StringWriter sw = new StringWriter();
            DateTimeUtils.WriteDateTimeString(sw, value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
            string minDateText = sw.ToString();

            DateTime parsedDt;
            DateTimeUtils.TryParseDateTimeIso(CreateStringReference(minDateText), DateTimeZoneHandling.RoundtripKind, out parsedDt);

            Assert.AreEqual(value, parsedDt);
        }

        [Test]
        public void Parse24HourDateTime()
        {
            DateTime dt;
            Assert.IsTrue(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:00Z"), DateTimeZoneHandling.RoundtripKind, out dt));
            Assert.AreEqual(new DateTime(2000, 12, 16, 0, 0, 0, DateTimeKind.Utc), dt);

            Assert.IsFalse(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:01:00Z"), DateTimeZoneHandling.RoundtripKind, out dt));
            Assert.IsFalse(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:01Z"), DateTimeZoneHandling.RoundtripKind, out dt));
            Assert.IsFalse(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:00.0000001Z"), DateTimeZoneHandling.RoundtripKind, out dt));
        }

#if !NET20
        [Test]
        public void Parse24HourDateTimeOffset()
        {
            DateTimeOffset dt;
            Assert.IsTrue(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00Z"), out dt));
            Assert.AreEqual(new DateTimeOffset(2000, 12, 16, 0, 0, 0, TimeSpan.Zero), dt);

            Assert.IsFalse(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:01:00Z"), out dt));
            Assert.IsFalse(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:01Z"), out dt));
            Assert.IsFalse(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00.0000001Z"), out dt));
        }
#endif

        [Test]
        public void NewDateTimeParse()
        {
            AssertNewDateTimeParseEqual("999x-12-31T23:59:59");
            AssertNewDateTimeParseEqual("9999x12-31T23:59:59");
            AssertNewDateTimeParseEqual("9999-1x-31T23:59:59");
            AssertNewDateTimeParseEqual("9999-12x31T23:59:59");
            AssertNewDateTimeParseEqual("9999-12-3xT23:59:59");
            AssertNewDateTimeParseEqual("9999-12-31x23:59:59");
            AssertNewDateTimeParseEqual("9999-12-31T2x:59:59");
            AssertNewDateTimeParseEqual("9999-12-31T23x59:59");
            AssertNewDateTimeParseEqual("9999-12-31T23:5x:59");
            AssertNewDateTimeParseEqual("9999-12-31T23:59x59");
            AssertNewDateTimeParseEqual("9999-12-31T23:59:5x");
            AssertNewDateTimeParseEqual("9999-12-31T23:59:5");
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.x");
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.99999999");
            //AssertNewDateTimeParseEqual("9999-12-31T23:59:59.", null); // DateTime.TryParse is bugged and should return null

            AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055Z");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055+00:00");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055+11:30");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055-11:30");

            AssertNewDateTimeParseEqual("2000-12-15T22:11:03Z");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03+00:00");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03+11:30");
            AssertNewDateTimeParseEqual("2000-12-15T22:11:03-11:30");

            AssertNewDateTimeParseEqual("0001-01-01T00:00:00Z");
            AssertNewDateTimeParseEqual("0001-01-01T00:00:00"); // this is DateTime.MinDate
            //AssertNewDateTimeParseEqual("0001-01-01T00:00:00+00:00"); // when the timezone is negative then this breaks
            //AssertNewDateTimeParseEqual("0001-01-01T00:00:00+11:30"); // when the timezone is negative then this breaks
            AssertNewDateTimeParseEqual("0001-01-01T00:00:00-12:00");

            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999Z");
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999"); // this is DateTime.MaxDate
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999+00:00", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999+11:30", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
            AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999-11:30", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
        }

        private void AssertNewDateTimeParseEqual(string text, object oldDate)
        {
            object oldDt;
            if (TryParseDateIso(text, DateParseHandling.DateTime, DateTimeZoneHandling.RoundtripKind, out oldDt))
            {
                oldDate = oldDt;
            }

            object newDt = null;
            DateTime temp;
            if (DateTimeUtils.TryParseDateTimeIso(CreateStringReference(text), DateTimeZoneHandling.RoundtripKind, out temp))
            {
                newDt = temp;
            }

            if (!Equals(oldDate, newDt))
            {
                Assert.AreEqual(oldDate, newDt, "DateTime parse not equal. Text: '{0}' Old ticks: {1} New ticks: {2}".FormatWith(
                    CultureInfo.InvariantCulture,
                    text,
                    oldDate != null ? ((DateTime)oldDate).Ticks : (long?)null,
                    newDt != null ? ((DateTime)newDt).Ticks : (long?)null
                    ));
            }
        }

        private void AssertNewDateTimeParseEqual(string text)
        {
            //Console.WriteLine("Parsing date text: " + text);

            object oldDt;
            TryParseDateIso(text, DateParseHandling.DateTime, DateTimeZoneHandling.RoundtripKind, out oldDt);

            AssertNewDateTimeParseEqual(text, oldDt);
        }

#if !NET20
        [Test]
        public void ReadOffsetMSDateTimeOffset()
        {
            char[] c = @"12345/Date(1418924498000+0800)/12345".ToCharArray();
            StringReference reference = new StringReference(c, 5, c.Length - 10);

            DateTimeOffset d;
            DateTimeUtils.TryParseDateTimeOffset(reference, null, CultureInfo.InvariantCulture, out d);

            long initialTicks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(d.DateTime, d.Offset);

            Assert.AreEqual(1418924498000, initialTicks);
            Assert.AreEqual(8, d.Offset.Hours);
        }

        [Test]
        public void NewDateTimeOffsetParse()
        {
            AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00");

            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055Z");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055+00:00");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055+13:30");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055-13:30");

            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03Z");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03+00:00");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03+13:30");
            AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03-13:30");

            AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00Z");
            AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00+00:00");
            AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00+13:30");
            AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00-13:30");

            AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999Z");
            AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999");
            AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999+00:00");
            AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999+13:30");
            AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999-13:30");
        }

        private void AssertNewDateTimeOffsetParseEqual(string text)
        {
            object oldDt;
            object newDt = null;

            TryParseDateIso(text, DateParseHandling.DateTimeOffset, DateTimeZoneHandling.Unspecified, out oldDt);

            DateTimeOffset temp;
            if (DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference(text), out temp))
            {
                newDt = temp;
            }

            if (!Equals(oldDt, newDt))
            {
                long? oldTicks = oldDt != null ? (long?)((DateTime)oldDt).Ticks : null;
                long? newTicks = newDt != null ? (long?)((DateTime)newDt).Ticks : null;

                Assert.AreEqual(oldDt, newDt, "DateTimeOffset parse not equal. Text: '{0}' Old ticks: {1} New ticks: {2}".FormatWith(
                    CultureInfo.InvariantCulture,
                    text,
                    oldTicks,
                    newTicks));
            }
        }
#endif

        internal static bool TryParseDateIso(string text, DateParseHandling dateParseHandling, DateTimeZoneHandling dateTimeZoneHandling, out object dt)
        {
            const string isoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

#if !NET20
            if (dateParseHandling == DateParseHandling.DateTimeOffset)
            {
                DateTimeOffset dateTimeOffset;
                if (DateTimeOffset.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTimeOffset))
                {
                    dt = dateTimeOffset;
                    return true;
                }
            }
            else
#endif
            {
                DateTime dateTime;
                if (DateTime.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime))
                {
                    dateTime = DateTimeUtils.EnsureDateTime(dateTime, dateTimeZoneHandling);

                    dt = dateTime;
                    return true;
                }
            }

            dt = null;
            return false;
        }
    }
}