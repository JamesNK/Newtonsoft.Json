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

using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

#if !(DNXCORE50 || NET20 || NET35) || NETSTANDARD2_0

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class CustomTraceWriter : TestFixtureBase
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

        [Test]
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

            StringAssert.AreEqual(@"[
  ""New Zealand"",
  ""Australia"",
  ""Denmark"",
  ""China""
]", json);
        }
    }
}

#endif