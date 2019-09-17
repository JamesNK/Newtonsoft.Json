using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when reading JSON.
    /// </summary>
    public class JsonTextReaderSettings
    {
        private DateTimeZoneHandling _dateTimeZoneHandling;
        private DateParseHandling _dateParseHandling;
        private FloatParseHandling _floatParseHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTextReaderSettings"/> class.
        /// </summary>
        public JsonTextReaderSettings()
        {
            _dateTimeZoneHandling = DateTimeZoneHandling.Local;
            _floatParseHandling = FloatParseHandling.Double;
            _dateParseHandling = DateParseHandling.None;
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> time zones are handled when reading JSON.
        /// </summary>
        public DateTimeZoneHandling DateTimeZoneHandling
        {
            get => _dateTimeZoneHandling;
            set
            {
                if (value < DateTimeZoneHandling.Local || value > DateTimeZoneHandling.RoundtripKind)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _dateTimeZoneHandling = value;
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

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public FloatParseHandling FloatParseHandling
        {
            get => _floatParseHandling;
            set
            {
                if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _floatParseHandling = value;
            }
        }
    }
}
