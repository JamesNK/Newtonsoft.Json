using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json {

  /// <summary>
  /// Specifies non-empty list handling options for the <see cref="JsonSerializer"/>.
  /// </summary>
  public enum NonEmptyListHandling
  {
    /// <summary>
    /// Ignore non-empty lists when deserializing; their value will remain the same.
    /// </summary>
    Do_Not_Replace = 0,
    /// <summary>
    /// Replace non-empty lists when deserializing.
    /// </summary>
    Replace = 1
  }
}
