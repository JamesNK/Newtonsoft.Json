#if HAVE_TRACE_WRITER
using System;
using System.Diagnostics;
using DiagnosticsTrace = System.Diagnostics.Trace;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Represents a trace writer that writes to the application's <see cref="TraceListener"/> instances.
    /// </summary>
    public class DiagnosticsTraceWriter : ITraceWriter
    {
        /// <summary>
        /// Gets the <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
        /// For example a filter level of <see cref="TraceLevel.Info"/> will exclude <see cref="TraceLevel.Verbose"/> messages and include <see cref="TraceLevel.Info"/>,
        /// <see cref="TraceLevel.Warning"/> and <see cref="TraceLevel.Error"/> messages.
        /// </summary>
        /// <value>
        /// The <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
        /// </value>
        public TraceLevel LevelFilter { get; set; }

        private TraceEventType GetTraceEventType(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Error:
                    return TraceEventType.Error;
                case TraceLevel.Warning:
                    return TraceEventType.Warning;
                case TraceLevel.Info:
                    return TraceEventType.Information;
                case TraceLevel.Verbose:
                    return TraceEventType.Verbose;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }

        /// <summary>
        /// Writes the specified trace level, message and optional exception.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
        /// <param name="message">The trace message.</param>
        /// <param name="ex">The trace exception. This parameter is optional.</param>
        public void Trace(TraceLevel level, string message, Exception? ex)
        {
            if (level == TraceLevel.Off)
            {
                return;
            }

            TraceEventCache eventCache = new TraceEventCache();
            TraceEventType traceEventType = GetTraceEventType(level);

            foreach (TraceListener listener in DiagnosticsTrace.Listeners)
            {
                if (!listener.IsThreadSafe)
                {
                    lock (listener)
                    {
                        listener.TraceEvent(eventCache, "Newtonsoft.Json", traceEventType, 0, message);
                    }
                }
                else
                {
                    listener.TraceEvent(eventCache, "Newtonsoft.Json", traceEventType, 0, message);
                }

                if (DiagnosticsTrace.AutoFlush)
                {
                    listener.Flush();
                }
            }
        }
    }
}

#endif