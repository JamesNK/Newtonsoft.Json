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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities
{
    internal static partial class DateTimeUtils
    {
        internal static Task WriteDateTimeStringAsync(TextWriter writer, DateTime value, DateFormatHandling format, string formatString, CultureInfo culture, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                char[] chars = new char[64];
                int pos = WriteDateTimeString(chars, 0, value, null, value.Kind, format);
                return writer.WriteAsync(chars, 0, pos, cancellationToken);
            }

            return writer.WriteAsync(value.ToString(formatString, culture), cancellationToken);
        }

        internal static Task WriteDateTimeOffsetStringAsync(TextWriter writer, DateTimeOffset value, DateFormatHandling format, string formatString, CultureInfo culture, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                char[] chars = new char[64];
                int pos = WriteDateTimeString(chars, 0, (format == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, format);
                return writer.WriteAsync(chars, 0, pos, cancellationToken);
            }

            return writer.WriteAsync(value.ToString(formatString, culture), cancellationToken);
        }
    }
}

#endif