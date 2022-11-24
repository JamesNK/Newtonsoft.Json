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
    /// Specifies the settings used when loading JSON.
    /// </summary>
    public class JsonLoadSettings
    {
        private CommentHandling _commentHandling;
        private LineInfoHandling _lineInfoHandling;
        private DuplicatePropertyNameHandling _duplicatePropertyNameHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLoadSettings"/> class.
        /// </summary>
        public JsonLoadSettings()
        {
            _lineInfoHandling = LineInfoHandling.Load;
            _commentHandling = CommentHandling.Ignore;
            _duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
        }

        /// <summary>
        /// Gets or sets how JSON comments are handled when loading JSON.
        /// The default value is <see cref="CommentHandling.Ignore" />.
        /// </summary>
        /// <value>The JSON comment handling.</value>
        public CommentHandling CommentHandling
        {
            get => _commentHandling;
            set
            {
                if (value < CommentHandling.Ignore || value > CommentHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _commentHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how JSON line info is handled when loading JSON.
        /// The default value is <see cref="LineInfoHandling.Load" />.
        /// </summary>
        /// <value>The JSON line info handling.</value>
        public LineInfoHandling LineInfoHandling
        {
            get => _lineInfoHandling;
            set
            {
                if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _lineInfoHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how duplicate property names in JSON objects are handled when loading JSON.
        /// The default value is <see cref="DuplicatePropertyNameHandling.Replace" />.
        /// </summary>
        /// <value>The JSON duplicate property name handling.</value>
        public DuplicatePropertyNameHandling DuplicatePropertyNameHandling
        {
            get => _duplicatePropertyNameHandling;
            set
            {
                if (value < DuplicatePropertyNameHandling.Replace || value > DuplicatePropertyNameHandling.Error)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _duplicatePropertyNameHandling = value;
            }
        }
    }
}
