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

#if !(NET35 || NET20 || PORTABLE || ASPNETCORE50)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests.Documentation
{
    public class LogEventInfo
    {
        public LogLevel Level;
        public string Message;
        public Exception Exception;
    }

    public class LogLevel
    {
        public static LogLevel Info;
        public static LogLevel Trace;
        public static LogLevel Error;
        public static LogLevel Warn;
        public static LogLevel Off;
    }

    public class Logger
    {
        public void Log(LogEventInfo logEvent)
        {
        }
    }

    public static class LogManager
    {
        public static Logger GetLogger(string className)
        {
            return new Logger();
        }
    }

    [TestFixture]
    public class TraceWriterTests : TestFixtureBase
    {
        #region CustomTraceWriterExample
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
        public void MemoryTraceWriterTest()
        {
            #region MemoryTraceWriterExample
            Staff staff = new Staff();
            staff.Name = "Arnie Admin";
            staff.Roles = new List<string> { "Administrator" };
            staff.StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.SerializeObject(
                staff,
                new JsonSerializerSettings { TraceWriter = traceWriter, Converters = { new JavaScriptDateTimeConverter() } });

            Console.WriteLine(traceWriter);
            // 2012-11-11T12:08:42.761 Info Started serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
            // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.799 Info Finished serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
            // 2013-05-18T21:38:11.255 Verbose Serialized JSON: 
            // {
            //   "Name": "Arnie Admin",
            //   "StartDate": new Date(
            //     976623132000
            //   ),
            //   "Roles": [
            //     "Administrator"
            //   ]
            // }
            #endregion

            MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;

            Assert.AreEqual(916, memoryTraceWriter.ToString().Length);
            Assert.AreEqual(7, memoryTraceWriter.GetTraceMessages().Count());
        }
    }
}

#endif