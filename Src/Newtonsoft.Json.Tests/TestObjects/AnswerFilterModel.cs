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
using System.Collections;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(PORTABLE || DNXCORE50) || NETSTANDARD1_3
    [Serializable]
    public class AnswerFilterModel
    {
        [NonSerialized]
        private readonly IList answerValues;

        /// <summary>
        /// Initializes a new instance of the  class.
        /// </summary>
        public AnswerFilterModel()
        {
            answerValues = (from answer in Enum.GetNames(typeof(Antworten))
                select new SelectListItem { Text = answer, Value = answer, Selected = false })
                .ToList();
        }

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ja.
        /// nach bisherigen Antworten.
        /// </summary>
        public bool Ja { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether handlungsbedarf.
        /// </summary>
        public bool Handlungsbedarf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether beratungsbedarf.
        /// </summary>
        public bool Beratungsbedarf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unzutreffend.
        /// </summary>
        public bool Unzutreffend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unbeantwortet.
        /// </summary>
        public bool Unbeantwortet { get; set; }

        /// <summary>
        /// Gets the answer values.
        /// </summary>
        public IEnumerable AnswerValues
        {
            get { return answerValues; }
        }
    }
#endif
}