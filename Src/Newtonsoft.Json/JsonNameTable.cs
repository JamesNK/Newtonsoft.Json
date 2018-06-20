namespace Newtonsoft.Json
{
    /// <summary>
    /// Base class for a table of atomized string objects.
    /// </summary>
    public abstract class JsonNameTable
    {
        /// <summary>
        /// Gets the string containing the same characters as the specified range of characters in the given array.
        /// </summary>
        /// <param name="key">The character array containing the name to find.</param>
        /// <param name="start">The zero-based index into the array specifying the first character of the name.</param>
        /// <param name="length">The number of characters in the name.</param>
        public abstract string Get(char[] key, int start, int length);
    }
}
