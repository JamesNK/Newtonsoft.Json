using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Provides data for the Error event.
  /// </summary>
  public class ErrorEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the current object the error event is being raised against.
    /// </summary>
    /// <value>The current object the error event is being raised against.</value>
    public object CurrentObject { get; private set; }
    /// <summary>
    /// Gets the error context.
    /// </summary>
    /// <value>The error context.</value>
    public ErrorContext ErrorContext { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
    /// </summary>
    /// <param name="currentObject">The current object.</param>
    /// <param name="errorContext">The error context.</param>
    public ErrorEventArgs(object currentObject, ErrorContext errorContext)
    {
      CurrentObject = currentObject;
      ErrorContext = errorContext;
    }
  }
}