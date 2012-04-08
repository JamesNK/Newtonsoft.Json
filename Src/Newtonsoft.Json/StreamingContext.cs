#if PocketPC
#pragma warning disable 1591

// This class is... borrowed from .NET and Microsoft for a short time.
// Hopefully Microsoft will add DateTimeOffset to the compact framework
// or I will rewrite a striped down version of this file myself

namespace System.Runtime.Serialization
{
  public enum StreamingContextStates
  {
    All = 255,
    Clone = 64,
    CrossAppDomain = 128,
    CrossMachine = 2,
    CrossProcess = 1,
    File = 4,
    Other = 32,
    Persistence = 8,
    Remoting = 16
  }

  public struct StreamingContext
  {
    internal object m_additionalContext;
    internal StreamingContextStates m_state;
    public StreamingContext(StreamingContextStates state)
      : this(state, null)
    {
    }

    public StreamingContext(StreamingContextStates state, object additional)
    {
      this.m_state = state;
      this.m_additionalContext = additional;
    }

    public object Context
    {
      get
      {
        return this.m_additionalContext;
      }
    }
    public override bool Equals(object obj)
    {
      return ((obj is StreamingContext) && ((((StreamingContext)obj).m_additionalContext == this.m_additionalContext) && (((StreamingContext)obj).m_state == this.m_state)));
    }

    public override int GetHashCode()
    {
      return (int)this.m_state;
    }

    public StreamingContextStates State
    {
      get
      {
        return this.m_state;
      }
    }
  }
}
#endif