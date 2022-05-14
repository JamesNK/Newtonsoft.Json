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

#if HAVE_TIME_ONLY
using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="TimeOnly"/> to and from JSON.
    /// </summary>
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private string? _timeFormat;
        private const string DefaultTimeFormat = "HH':'mm':'ss'.'FFFFFFF";

        /// <summary>
        /// Gets or sets the time format used when converting a time to and from JSON.
        /// </summary>
        /// <value>The time format used when converting a time to and from JSON.</value>
        public string? TimeFormat
        {
            get => _timeFormat ?? string.Empty;
            set => _timeFormat = (StringUtils.IsNullOrEmpty(value)) ? null : value;
        }

        public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            TimeSpan timeSpan;
            if(!StringUtils.IsNullOrEmpty(_timeFormat)) {
                timeSpan = TimeSpan.ParseExact((string)reader.Value, _timeFormat, CultureInfo.InvariantCulture);
            } else {
                timeSpan = TimeSpan.Parse((string)reader.Value, CultureInfo.InvariantCulture);
            }

            return TimeOnly.FromTimeSpan(timeSpan);
        }

        public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(_timeFormat ?? DefaultTimeFormat, CultureInfo.InvariantCulture));
        }
    }
}

#endif