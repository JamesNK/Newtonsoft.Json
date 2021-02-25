namespace Newtonsoft.Json
{
    /// <summary>
    /// Provides an interface for using pooled arrays.
    /// </summary>
    /// <typeparam name="T">The array type content.</typeparam>
    public interface IArrayPool<T>
    {
        /// <summary>
        /// Rent an array from the pool. This array must be returned when it is no longer needed.
        /// </summary>
        /// <param name="minimumLength">The minimum required length of the array. The returned array may be longer.</param>
        /// <returns>The rented array from the pool. This array must be returned when it is no longer needed.</returns>
        T[] Rent(int minimumLength);

        /// <summary>
        /// Return an array to the pool.
        /// </summary>
        /// <param name="array">The array that is being returned.</param>
        void Return(T[]? array);
    }
}