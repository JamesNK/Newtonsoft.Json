namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// <para>Options for merging JTokens</para>
    /// </summary>
    public class MergeOptions
    {
        /// <summary>
        /// <para>How to handle arrays</para>
        /// </summary>
        public MergeOptionArrayHandling ArrayHandling { get; set; }

        /// <summary>
        /// <para>Default for merge options</para>
        /// </summary>
        public static readonly MergeOptions Default = new MergeOptions
            {
                ArrayHandling = MergeOptionArrayHandling.Overwrite
            };
    }
}