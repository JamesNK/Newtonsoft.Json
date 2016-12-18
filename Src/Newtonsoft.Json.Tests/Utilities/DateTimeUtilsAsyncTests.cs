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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    public class DateTimeUtilsAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task RoundTripDateTimeMinAndMax()
        {
            await Task.WhenAll(RoundtripDateIsoAsync(DateTime.MinValue), RoundtripDateIsoAsync(DateTime.MaxValue));
        }

        [Test]
        public async Task RoundTripDateTimeOffset()
        {
            await Task.WhenAll(
                RoundtripDateIsoAsync(DateTimeOffset.MinValue),
                RoundtripDateIsoAsync(DateTimeOffset.MaxValue),
                RoundtripDateIsoAsync(new DateTimeOffset(2000, 2, 29, 12, 39, 40, 123, TimeSpan.Zero)),
                RoundtripDateIsoAsync(new DateTimeOffset(2000, 2, 29, 12, 39, 40, 123, new TimeSpan(1, 30, 0))),
                RoundtripDateIsoAsync(new DateTimeOffset(2000, 2, 29, 12, 39, 40, 123, -new TimeSpan(8, 30, 0))));
        }

        [Test]
        public void WriteDateTimeAsyncAlreadyCancelled()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;
            source.Cancel();
            var task = DateTimeUtils.WriteDateTimeStringAsync(TextWriter.Null, DateTime.MaxValue, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture, token);
            Assert.IsTrue(task.IsCanceled);
            task = DateTimeUtils.WriteDateTimeStringAsync(TextWriter.Null, DateTime.MaxValue, DateFormatHandling.IsoDateFormat, "yyyy-MM-dd", CultureInfo.InvariantCulture, token);
            Assert.IsTrue(task.IsCanceled);
        }

        private static StringReference CreateStringReference(string s)
        {
            return new StringReference(s.ToCharArray(), 0, s.Length);
        }

        private static async Task RoundtripDateIsoAsync(DateTime value)
        {
            StringWriter sw = new StringWriter();
            await DateTimeUtils.WriteDateTimeStringAsync(sw, value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture, CancellationToken.None);
            string minDateText = sw.ToString();

            DateTime parsedDt;
            DateTimeUtils.TryParseDateTimeIso(CreateStringReference(minDateText), DateTimeZoneHandling.RoundtripKind, out parsedDt);

            Assert.AreEqual(value, parsedDt);
        }

        private static async Task RoundtripDateIsoAsync(DateTimeOffset value)
        {
            StringWriter sw = new StringWriter();
            await DateTimeUtils.WriteDateTimeOffsetStringAsync(sw, value, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture, CancellationToken.None);
            string minDateText = sw.ToString();

            DateTimeOffset parsedDt;
            DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference(minDateText), out parsedDt);

            Assert.AreEqual(value, parsedDt);
        }

        [Test]
        public void WriteDateTimeOffsetAsyncAlreadyCancelled()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;
            source.Cancel();
            var task = DateTimeUtils.WriteDateTimeOffsetStringAsync(TextWriter.Null, DateTime.MaxValue, DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture, token);
            Assert.IsTrue(task.IsCanceled);
            task = DateTimeUtils.WriteDateTimeOffsetStringAsync(TextWriter.Null, DateTime.MaxValue, DateFormatHandling.IsoDateFormat, "yyyy-MM-dd", CultureInfo.InvariantCulture, token);
            Assert.IsTrue(task.IsCanceled);
        }
    }
}

#endif
