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

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when selecting JSON.
    /// </summary>
    public class JsonSelectSettings
    {
#if HAVE_REGEX_TIMEOUTS
        /// <summary>
        /// Gets or sets a timeout that will be used when executing regular expressions.
        /// </summary>
        /// <value>The timeout that will be used when executing regular expressions.</value>
        public TimeSpan? RegexMatchTimeout { get; set; }
#endif

        /// <summary>
        /// Gets or sets a flag that indicates whether an error should be thrown if
        /// no tokens are found when evaluating part of the expression.
        /// </summary>
        /// <value>
        /// A flag that indicates whether an error should be thrown if
        /// no tokens are found when evaluating part of the expression.
        /// </value>
        public bool ErrorWhenNoMatch { get; set; }
    }
}
