using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Represents a trace writer.
    /// </summary>
    public interface ITraceWriter
    {
        /// <summary>
        /// Gets the <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
        /// For example a filter level of <see cref="TraceLevel.Info"/> will exclude <see cref="TraceLevel.Verbose"/> messages and include <see cref="TraceLevel.Info"/>,
        /// <see cref="TraceLevel.Warning"/> and <see cref="TraceLevel.Error"/> messages.
        /// </summary>
        /// <value>The <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.</value>
        TraceLevel LevelFilter { get; }

        /// <summary>
        /// Writes the specified trace level, message and optional exception.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
        /// <param name="message">The trace message.</param>
        /// <param name="ex">The trace exception. This parameter is optional.</param>
        void Trace(TraceLevel level, string message, Exception? ex);
    }
}