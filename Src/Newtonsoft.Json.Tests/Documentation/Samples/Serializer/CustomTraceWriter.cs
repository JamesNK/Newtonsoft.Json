using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class CustomTraceWriter
    {
        #region Types
        public class NLogTraceWriter : ITraceWriter
        {
            private static readonly Logger Logger = LogManager.GetLogger("NLogTraceWriter");

            public TraceLevel LevelFilter
            {
                // trace all messages. nlog can handle filtering
                get { return TraceLevel.Verbose; }
            }

            public void Trace(TraceLevel level, string message, Exception ex)
            {
                LogEventInfo logEvent = new LogEventInfo
                {
                    Message = message,
                    Level = GetLogLevel(level),
                    Exception = ex
                };

                // log Json.NET message to NLog
                Logger.Log(logEvent);
            }

            private LogLevel GetLogLevel(TraceLevel level)
            {
                switch (level)
                {
                    case TraceLevel.Error:
                        return LogLevel.Error;
                    case TraceLevel.Warning:
                        return LogLevel.Warn;
                    case TraceLevel.Info:
                        return LogLevel.Info;
                    case TraceLevel.Off:
                        return LogLevel.Off;
                    default:
                        return LogLevel.Trace;
                }
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            IList<string> countries = new List<string>
            {
                "New Zealand",
                "Australia",
                "Denmark",
                "China"
            };

            string json = JsonConvert.SerializeObject(countries, Formatting.Indented, new JsonSerializerSettings
            {
                TraceWriter = new NLogTraceWriter()
            });

            Console.WriteLine(json);
            // [
            //   "New Zealand",
            //   "Australia",
            //   "Denmark",
            //   "China"
            // ]
            #endregion
        }
    }
}