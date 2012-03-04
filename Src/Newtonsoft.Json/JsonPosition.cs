using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json
{
  internal enum JsonContainerType
  {
    None,
    Object,
    Array,
    Constructor
  }

  internal struct JsonPosition
  {
    internal JsonContainerType Type;
    internal int? Position;
    internal string PropertyName;

    internal void WriteTo(StringBuilder sb)
    {
      switch (Type)
      {
        case JsonContainerType.Object:
          if (PropertyName != null)
          {
            if (sb.Length > 0)
              sb.Append(".");
            sb.Append(PropertyName);
          }
          break;
        case JsonContainerType.Array:
        case JsonContainerType.Constructor:
          if (Position != null)
          {
            sb.Append("[");
            sb.Append(Position);
            sb.Append("]");
          }
          break;
      }
    }

    internal bool InsideContainer()
    {
      switch (Type)
      {
        case JsonContainerType.Object:
          return (PropertyName != null);
        case JsonContainerType.Array:
        case JsonContainerType.Constructor:
          return (Position != null);
      }

      return false;
    }

    internal static string BuildPath(IEnumerable<JsonPosition> positions)
    {
      StringBuilder sb = new StringBuilder();

      foreach (JsonPosition state in positions)
      {
        state.WriteTo(sb);
      }

      return sb.ToString();
    }
  }
}