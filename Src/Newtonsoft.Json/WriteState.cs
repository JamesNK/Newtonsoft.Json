using System;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Specifies the state of the <see cref="JsonWriter"/>.
  /// </summary>
  public enum WriteState
  {
    /// <summary>
    /// An exception has been thrown, which has left the <see cref="JsonWriter"/> in an invalid state.
    /// You may call the <see cref="JsonWriter.Close"/> method to put the <see cref="JsonWriter"/> in the <c>Closed</c> state.
    /// Any other <see cref="JsonWriter"/> method calls results in an <see cref="InvalidOperationException"/> being thrown. 
    /// </summary>
    Error,
    /// <summary>
    /// The <see cref="JsonWriter.Close"/> method has been called. 
    /// </summary>
    Closed,
    /// <summary>
    /// An object is being written. 
    /// </summary>
    Object,
    /// <summary>
    /// A array is being written.
    /// </summary>
    Array,
    /// <summary>
    /// A constructor is being written.
    /// </summary>
    Constructor,
    /// <summary>
    /// A property is being written.
    /// </summary>
    Property,
    /// <summary>
    /// A write method has not been called.
    /// </summary>
    Start
  }
}