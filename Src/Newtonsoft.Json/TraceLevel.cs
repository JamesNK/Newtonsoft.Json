
#if (DOTNET || PORTABLE40 || PORTABLE)
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Specifies what messages to output for the <see cref="ITraceWriter"/> class.
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>
        /// Output no tracing and debugging messages.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Output error-handling messages.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Output warnings and error-handling messages.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Output informational messages, warnings, and error-handling messages.
        /// </summary>
        Info = 3,

        /// <summary>
        /// Output all debugging and tracing messages.
        /// </summary>
        Verbose = 4
    }
}

#endif