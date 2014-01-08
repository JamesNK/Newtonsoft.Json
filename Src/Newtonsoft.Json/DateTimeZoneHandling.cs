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

namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies how to treat the time value when converting between string and <see cref="DateTime"/>.
    /// </summary>
    public enum DateTimeZoneHandling
    {
        /// <summary>
        /// Treat as local time. If the <see cref="DateTime"/> object represents a Coordinated Universal Time (UTC), it is converted to the local time.
        /// </summary>
        Local,

        /// <summary>
        /// Treat as a UTC. If the <see cref="DateTime"/> object represents a local time, it is converted to a UTC.
        /// </summary>
        Utc,

        /// <summary>
        /// Treat as a local time if a <see cref="DateTime"/> is being converted to a string.
        /// If a string is being converted to <see cref="DateTime"/>, convert to a local time if a time zone is specified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Time zone information should be preserved when converting.
        /// </summary>
        RoundtripKind
    }
}