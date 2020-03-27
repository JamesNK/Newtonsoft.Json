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
        private DateParseHandling _dateParseHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLoadSettings"/> class.
        /// </summary>
        public JsonLoadSettings()
        {
            _lineInfoHandling = LineInfoHandling.Load;
            _commentHandling = CommentHandling.Ignore;
            _duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
#if HAVE_DATE_TIME_OFFSET
            _dateParseHandling = DateParseHandling.DateTimeOffset;
#else
            _dateParseHandling = DateParseHandling.DateTime;
#endif
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

        /// <summary>
        /// Gets or sets how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON.
        /// </summary>
        public DateParseHandling DateParseHandling
        {
            get => _dateParseHandling;
            set
            {
                if (value < DateParseHandling.None ||
#if HAVE_DATE_TIME_OFFSET
                    value > DateParseHandling.DateTimeOffset
#else
                    value > DateParseHandling.DateTime
#endif
                    )
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _dateParseHandling = value;
            }
        }
    }
}
