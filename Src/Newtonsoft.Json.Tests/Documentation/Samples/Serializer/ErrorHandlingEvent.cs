using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class ErrorHandlingEvent
    {
        public void Example()
        {
            #region Usage
            List<string> errors = new List<string>();

            List<DateTime> c = JsonConvert.DeserializeObject<List<DateTime>>(@"[
              '2009-09-09T00:00:00Z',
              'I am not a date and will error!',
              [
                1
              ],
              '1977-02-20T00:00:00Z',
              null,
              '2000-12-01T00:00:00Z'
            ]",
                new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    },
                    Converters = { new IsoDateTimeConverter() }
                });

            // 2009-09-09T00:00:00Z
            // 1977-02-20T00:00:00Z
            // 2000-12-01T00:00:00Z

            // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
            // Unexpected token parsing date. Expected String, got StartArray.
            // Cannot convert null value to System.DateTime.
            #endregion
        }
    }
}