namespace Newtonsoft.Json
{
    /// <summary>
    /// Provides an interface for using pooled buffers.
    /// </summary>
    /// <typeparam name="T">The buffer type content.</typeparam>
    public interface IJsonBufferPool<T>
    {
        /// <summary>
        /// Rent a buffer from the pool. This buffer must be returned when it is no longer needed.
        /// </summary>
        /// <param name="minSize">The minimum required size of the buffer. The returned buffer may be larger.</param>
        /// <returns>The rented buffer from the pool.</returns>
        T[] RentBuffer(int minSize);

        /// <summary>
        /// Return a buffer to the pool.
        /// </summary>
        /// <param name="buffer">The buffer that is being returned.</param>
        void ReturnBuffer(ref T[] buffer);
    }
}