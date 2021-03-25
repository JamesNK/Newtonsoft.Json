using System;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/>Ignored Init Value while write value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class JsonIgnoredInitValueAttribute : Attribute
    {
    }
}
