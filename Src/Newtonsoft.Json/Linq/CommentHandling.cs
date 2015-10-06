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
}