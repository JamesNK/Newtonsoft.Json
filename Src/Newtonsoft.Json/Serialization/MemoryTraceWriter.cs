using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Represents a trace writer that writes to memory. When the trace message limit is
  /// reached then old trace messages will be removed as new messages are added.
  /// </summary>
  public class MemoryTraceWriter : ITraceWriter
  {
    private readonly Queue<string> _traceMessages;

    /// <summary>
    /// Gets the <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
    /// For example a filter level of <code>Info</code> will exclude <code>Verbose</code> messages and include <code>Info</code>,
    /// <code>Warning</code> and <code>Error</code> messages.
    /// </summary>
    /// <value>
    /// The <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
    /// </value>
    public TraceLevel LevelFilter { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTraceWriter"/> class.
    /// </summary>
    public MemoryTraceWriter()
    {
      LevelFilter = TraceLevel.Verbose;
      _traceMessages = new Queue<string>();
    }

    /// <summary>
    /// Writes the specified trace level, message and optional exception.
    /// </summary>
    /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
    /// <param name="message">The trace message.</param>
    /// <param name="ex">The trace exception. This parameter is optional.</param>
    public void Trace(TraceLevel level, string message, Exception ex)
    {
      string traceMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture) + " " + level.ToString("g") + " " + message;

      if (_traceMessages.Count >= 1000)
        _traceMessages.Dequeue();

      _traceMessages.Enqueue(traceMessage);
    }

    /// <summary>
    /// Returns an enumeration of the most recent trace messages.
    /// </summary>
    /// <returns>An enumeration of the most recent trace messages.</returns>
    public IEnumerable<string> GetTraceMessages()
    {
      return _traceMessages;
    }

    /// <summary>
    /// Returns a <see cref="String"/> of the most recent trace messages.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> of the most recent trace messages.
    /// </returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      foreach (string traceMessage in _traceMessages)
      {
        if (sb.Length > 0)
          sb.AppendLine();

        sb.Append(traceMessage);
      }

      return sb.ToString();
    }
  }
}