namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies how JSON comments are handled when loading JSON.
    /// </summary>
    public enum CommentHandling
    {
        /// <summary>
        /// Ignore comments.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Load comments as a <see cref="JValue"/> with type <see cref="JTokenType.Comment"/>.
        /// </summary>
        Load = 1
    }

    /// <summary>
    /// Specifies how line information is handled when loading JSON.
    /// </summary>
    public enum LineInfoHandling
    {
        /// <summary>
        /// Ignore line information.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Load line information.
        /// </summary>
        Load = 1
    }
}