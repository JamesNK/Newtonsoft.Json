#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if HAVE_BENCHMARKS

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;

namespace Newtonsoft.Json.Tests.Benchmarks
{
    [MemoryDiagnoser]
    [Config(typeof(Config))]
    public class JsonTextReaderBenchmarks
    {
        private static readonly string text = System.IO.File.ReadAllText(TestFixtureBase.ResolvePath("large.json"));

        private static readonly string EmailProperty = StringLookupTable.Shared.Get("email");

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(BenchmarkDotNet.Jobs.Job.LongRun.WithGcServer(true));
            }
        }

        [Benchmark(Baseline = true)]
        public void ReadLargeJson()
        {
            var emails = new List<string>(5000);
            using (var fs = new StringReader(text))
            using (JsonTextReader jsonTextReader = new JsonTextReader(fs))
            {
                jsonTextReader.DateParseHandling = DateParseHandling.None;
                jsonTextReader.PropertyNameTable = null;

                while (jsonTextReader.Read())
                {
                    if (jsonTextReader.TokenType == JsonToken.PropertyName && jsonTextReader.Value.Equals("email"))
                    {
                        if (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.String)
                        {
                            emails.Add((string)jsonTextReader.Value);
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void ReadLargeJson_NameTable()
        {
            var emails = new List<string>(5000);
            using (var fs = new StringReader(text))
            using (JsonTextReader jsonTextReader = new JsonTextReader(fs))
            {
                jsonTextReader.DateParseHandling = DateParseHandling.None;
                jsonTextReader.PropertyNameTable = StringLookupTable.Shared;

                while (jsonTextReader.Read())
                {
                    if (jsonTextReader.TokenType == JsonToken.PropertyName && object.ReferenceEquals(jsonTextReader.Value, JsonTextReaderBenchmarks.EmailProperty))
                    {
                        if (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.String)
                        {
                            emails.Add((string)jsonTextReader.Value);
                        }
                    }
                }
            }
        }
    }

    public sealed class StringLookupTable : NameTable
    {
        public static readonly StringLookupTable Shared = new StringLookupTable();

        /// <summary>
        /// The precomputed De-Bruijn mask to depth lookup table.
        /// </summary>
        private static readonly uint[] DeBruijnMaskToDepthTable = new uint[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        /// <summary>
        /// The internal string lookup hash table.
        /// </summary>
        private string[] lookup = new string[8];

        /// <summary>
        /// Gets the interned reference to specified string.
        /// </summary>
        /// <param name="s">The string to lookup.</param>
        public string Get(string s)
        {
            var table = this.lookup;
            var mask = (uint)table.Length - 1;
            var hash = StringLookupTable.GetHash(s);

            for (var probe = 0u; probe <= mask; ++probe, hash += 2 * probe - 1)
            {
                var value = table[hash & mask];
                if (value == null)
                {
                    break;
                }
                else if (StringLookupTable.StringMatch(s, value))
                {
                    return value;
                }
            }

            return this.Add(s);
        }

        /// <summary>
        /// Gets the interned reference to specified string.
        /// </summary>
        /// <param name="buffer">The string buffer.</param>
        /// <param name="start">The string start.</param>
        /// <param name="length">The string length.</param>
        public override string Get(char[] buffer, int start, int length)
        {
            var table = this.lookup;
            var mask = (uint)table.Length - 1;
            var hash = StringLookupTable.GetHash(buffer, start, length);

            for (var probe = 0u; probe <= mask; ++probe, hash += 2 * probe - 1)
            {
                var value = table[hash & mask];
                if (value == null)
                {
                    break;
                }
                else if (StringLookupTable.StringMatch(buffer, start, length, value))
                {
                    return value;
                }
            }

            return this.Add(new string(buffer, start, length));
        }

        /// <summary>
        /// Adds the specified string to internal string lookup hash table.
        /// </summary>
        /// <param name="s">The string to add.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private string Add(string s)
        {
            lock (this)
            {
                while (true)
                {
                    var table = this.lookup;
                    var mask = (uint)table.Length - 1;
                    var hash = StringLookupTable.GetHash(s);
                    var depth = StringLookupTable.GetDepth(mask);

                    for (var probe = 0U; probe <= depth; ++probe, hash += 2 * probe - 1)
                    {
                        var value = table[hash & mask];
                        if (value == null)
                        {
                            return table[hash & mask] = s;
                        }
                        else if (StringLookupTable.StringMatch(s, value))
                        {
                            return value;
                        }
                    }

                    Volatile.Write(ref this.lookup, StringLookupTable.Resize(table, mask, depth));
                }
            }
        }

        /// <summary>
        /// Resizes the specified lookup hash table.
        /// </summary>
        /// <param name="table">The lookup hash table.</param>
        /// <param name="mask">The lookup hash table mask.</param>
        /// <param name="depth">The lookup hash table depth.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string[] Resize(string[] table, uint mask, uint depth)
        {
            resize:
            mask += mask + 1; depth++;

            var newTable = new string[mask + 1];
            foreach (var value in table)
            {
                if (value != null && !StringLookupTable.TryInsert(newTable, mask, depth, value))
                {
                    newTable = null;
                    goto resize;
                }
            }

            return newTable;
        }

        /// <summary>
        /// Tries to insert string into lookup hash table.
        /// </summary>
        /// <param name="table">The lookup hash table.</param>
        /// <param name="mask">The lookup hash table mask.</param>
        /// <param name="depth">The lookup hash table depth.</param>
        /// <param name="value">The string to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryInsert(string[] table, uint mask, uint depth, string value)
        {
            var hash = StringLookupTable.GetHash(value);
            for (var probe = 0U; probe <= depth; ++probe, hash += 2 * probe - 1)
            {
                if (table[hash & mask] == null)
                {
                    table[hash & mask] = value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determinate if two strings are equals.
        /// </summary>
        /// <param name="buffer">The first string buffer.</param>
        /// <param name="start">The first string start.</param>
        /// <param name="length">The first string length.</param>
        /// <param name="value">The second string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StringMatch(char[] buffer, int start, int length, string value)
        {
            if (value.Length == length)
            {
                for (var i = 0; i < value.Length; ++i)
                {
                    if (value[i] != buffer[start + i])
                    {
                        goto NotEquals;
                    }
                }

                return true;
            }

            NotEquals:
            return false;
        }

        /// <summary>
        /// Determinate if two strings are equals.
        /// </summary>
        /// <param name="s">The first string.</param>
        /// <param name="value">The second string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool StringMatch(string s, string value)
        {
            if (value.Length == s.Length)
            {
                if (!object.ReferenceEquals(s, value))
                {
                    for (var i = 0; i < value.Length; ++i)
                    {
                        if (value[i] != s[i])
                        {
                            goto NotEquals;
                        }
                    }
                }

                return true;
            }

            NotEquals:
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetHash(char[] key, int start, int length)
        {
            //return MarvinHash.ComputeHash(key, start, length);

            var hash = (uint)length;
            for (var i = start; i < start + length; ++i)
            {
                hash += (hash << 7) ^ key[i];
            }

            hash -= hash >> 17;
            hash -= hash >> 11;
            hash -= hash >> 5;

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetHash(string key)
        {
            //return MarvinHash.ComputeHash(key);

            var hash = (uint)key.Length;
            for (var i = 0; i < key.Length; ++i)
            {
                hash += (hash << 7) ^ key[i];
            }

            hash -= hash >> 17;
            hash -= hash >> 11;
            hash -= hash >> 5;

            return hash;
        }


        /// <summary>
        /// Gets the hash table depth for specified hash table mask.
        /// </summary>
        /// <param name="mask">The lookup hash table mask.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetDepth(uint mask)
        {
            return StringLookupTable.DeBruijnMaskToDepthTable[(mask * 0x07c4acdd) >> 27];
        }
    }

    /// <summary>
    /// The shared resource pool implementation.
    /// </summary>
    /// <typeparam name="T">The type of the array items.</typeparam>
    public sealed class SharedArrayPool<T>
    {
        /// <summary>
        /// The maximum length of each array in the pool (2^22).
        /// </summary>
        private const int MaxArrayLength = 4 * 1024 * 1024;

        /// <summary>
        /// The maximum number of arrays per bucket that are available for rent.
        /// </summary>
        private const int MaxNumberOfArraysPerBucket = 24;

        /// <summary>
        /// The lazily-initialized shared pool instances.
        /// </summary>
        private static SharedArrayPool<T>[] sharedInstances = null;

        /// <summary>
        /// Gets a shared <see cref="ArrayPool{T}"/> instance.
        /// </summary>
        public static SharedArrayPool<T> Shared
        {
            get
            {
                var instances = Volatile.Read(ref SharedArrayPool<T>.sharedInstances) ?? SharedArrayPool<T>.EnsureSharedCreated();
                return instances[MemoryUtility.CurrentExecutionId & (instances.Length - 1)];
            }
        }

        /// <summary>
        /// Ensures that <see cref="sharedInstances" /> has been initialized to a pool and returns it.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static SharedArrayPool<T>[] EnsureSharedCreated()
        {
            Interlocked.CompareExchange(
                location1: ref SharedArrayPool<T>.sharedInstances,
                value: Enumerable.Range(0, MemoryUtility.GetNextPowerOf2(Environment.ProcessorCount)).Select(index => new SharedArrayPool<T>()).ToArray(),
                comparand: null);

            return SharedArrayPool<T>.sharedInstances;
        }

        /// <summary>
        /// The thread-safe buckets containing buffers that can be rented and returned.
        /// </summary>
        private readonly Bucket[] buckets;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedArrayPool{T}"/> class.
        /// </summary>
        private SharedArrayPool()
        {
            this.buckets = new Bucket[MemoryUtility.GetBucketIndex(SharedArrayPool<T>.MaxArrayLength) + 1];

            for (var i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i] = new Bucket(
                    bufferLength: MemoryUtility.GetBucketBufferSize(i),
                    numberOfBuffers: SharedArrayPool<T>.MaxNumberOfArraysPerBucket);
            }
        }

        /// <summary>
        /// Gets the maximum length of array in the pool.
        /// </summary>
        public int MaximumLength
        {
            get { return SharedArrayPool<T>.MaxArrayLength; }
        }

        /// <summary>
        /// Retrieves a buffer that is at least the requested length.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array needed.</param>
        public T[] Rent(int minimumLength)
        {
            if (minimumLength == 0)
            {
                return new T[0];
            }

            var index = MemoryUtility.GetBucketIndex(minimumLength);
            return index < this.buckets.Length ? this.buckets[index].Rent() : new T[minimumLength];
        }

        /// <summary>
        /// Returns to the pool an array that was previously obtained.
        /// </summary>
        /// <param name="array">The buffer previously obtained to return to the pool.</param>
        public void Return(T[] array)
        {
            if (array.Length == 0)
            {
                return;
            }

            // Determine with what bucket this array length is associated.
            var index = MemoryUtility.GetBucketIndex(array.Length);
            if (index < this.buckets.Length)
            {
                // Return the buffer to its bucket. In the future, we might consider having Return return false
                // instead of dropping a bucket, in which case we could try to return to a lower-sized bucket,
                // just as how in Rent we allow renting from a higher-sized bucket.
                this.buckets[index].Return(array);
            }
        }

        /// <summary>
        /// Provides a thread-safe bucket containing buffers that can be rented and returned.
        /// </summary>
        private sealed class Bucket
        {
            /// <summary>
            /// The bucket buffers length.
            /// </summary>
            private readonly int bufferLength;

            /// <summary>
            /// The bucket buffers to rent and return.
            /// </summary>
            private readonly T[][] buffers;

            /// <summary>
            /// The bucket lock. Do not make this readonly; it's a mutable struct.
            /// </summary>
            private SpinLock buffersLock;

            /// <summary>
            /// The current bucket index.
            /// </summary>
            private int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="Bucket"/> class.
            /// </summary>
            /// <param name="bufferLength">Length of the buffer.</param>
            /// <param name="numberOfBuffers">The number of buffers.</param>
            internal Bucket(int bufferLength, int numberOfBuffers)
            {
                this.bufferLength = bufferLength;
                this.buffers = new T[numberOfBuffers][];
                this.buffersLock = new SpinLock(enableThreadOwnerTracking: false);
            }

            /// <summary>
            /// Takes an array from the bucket. If the bucket is empty, returns null.
            /// </summary>
            internal T[] Rent()
            {
                T[] buffer = null;

                // While holding the lock, grab whatever is at the next available index and
                // update the index.  We do as little work as possible while holding the spin
                // lock to minimize contention with other threads.  The try/finally is
                // necessary to properly handle thread aborts on platforms which have them.
                bool lockTaken = false;
                try
                {
                    this.buffersLock.Enter(ref lockTaken);

                    if (this.index < this.buffers.Length)
                    {
                        buffer = this.buffers[this.index];
                        this.buffers[this.index++] = null;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.buffersLock.Exit(false);
                    }
                }

                // While we were holding the lock, we grabbed whatever was at the next available index, if
                // there was one. If we tried and if we got back null, that means we hadn't yet allocated
                // for that slot, in which case we should do so now.
                return buffer ?? new T[this.bufferLength];
            }

            /// <summary>
            /// Attempts to return the buffer to the bucket.  If successful, the buffer will be stored
            /// in the bucket and true will be returned; otherwise, the buffer won't be stored, and false
            /// will be returned.
            /// </summary>
            /// <param name="array">The array to return.</param>
            internal void Return(T[] array)
            {
                if (array.Length != this.bufferLength)
                {
                    return;
                }

                // While holding the spin lock, if there's room available in the bucket,
                // put the buffer into the next available slot.  Otherwise, we just drop it.
                // The try/finally is necessary to properly handle thread aborts on platforms
                // which have them.
                bool lockTaken = false;
                try
                {
                    this.buffersLock.Enter(ref lockTaken);

                    if (this.index > 0)
                    {
                        this.buffers[--this.index] = array;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.buffersLock.Exit(false);
                    }
                }
            }
        }
    }
    

    /// <summary>
    /// The internal memory management utility.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "PInvoke methods are not exposed.")]
    internal static class MemoryUtility
    {
        #region CurrentExecutionId

        /// <summary>
        /// The execution identifier cache shift.
        /// </summary>
        private const int ExecutionIdCacheShift = 16;

        /// <summary>
        /// The execution identifier cache count down mask.
        /// </summary>
        private const int ExecutionIdCacheCountDownMask = (1 << MemoryUtility.ExecutionIdCacheShift) - 1;

        /// <summary>
        /// The execution identifier refresh rate.
        /// </summary>
        private const int ExecutionIdRefreshRate = 1000;

        /// <summary>
        /// The thread local execution identifier cache.
        /// </summary>
        [ThreadStatic]
        private static int threadExecutionIdCache;

        /// <summary>
        /// Gets the cached processor number used as a hint for which per-core resource to access.
        /// It is periodically refreshed to trail the actual thread core affinity.
        /// </summary>
        internal static int CurrentExecutionId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var executionIdCache = MemoryUtility.threadExecutionIdCache--;
                if ((executionIdCache & MemoryUtility.ExecutionIdCacheCountDownMask) == 0)
                {
                    executionIdCache = MemoryUtility.RefreshExecutionIdCache();
                }

                return executionIdCache >> MemoryUtility.ExecutionIdCacheShift;
            }
        }

        /// <summary>
        /// Refreshes the execution identifier cache.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int RefreshExecutionIdCache()
        {
            return MemoryUtility.threadExecutionIdCache = (MemoryUtility.GetCurrentProcessorNumber() << MemoryUtility.ExecutionIdCacheShift) | MemoryUtility.ExecutionIdRefreshRate;
        }

        /// <summary>
        /// Gets the number of the processor the current thread was running on during the call to this function.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentProcessorNumber();

        #endregion

        /// <summary>
        /// Disposes the specified disposable.
        /// </summary>
        /// <typeparam name="T">The type of disposable object.</typeparam>
        /// <param name="disposable">The disposable object.</param>
        internal static void Dispose<T>(ref T disposable)
            where T : class, IDisposable
        {
            var obj = Interlocked.Exchange(ref disposable, null);
            if (obj != null)
            {
                obj.Dispose();
            }
        }

        /// <summary>
        /// Selects the index of the array pool bucket.
        /// </summary>
        /// <param name="bufferSize">Size of the requested buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "Performance-critical code.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1501:StatementMustNotBeOnSingleLine", Justification = "Performance-critical code.")]
        internal static int GetBucketIndex(int bufferSize)
        {
            uint bitsRemaining = ((uint)bufferSize - 1) >> 4;

            int poolIndex = 0;
            if (bitsRemaining > 0xFFFF) { bitsRemaining >>= 16; poolIndex = 16; }
            if (bitsRemaining > 0xFF) { bitsRemaining >>= 8; poolIndex += 8; }
            if (bitsRemaining > 0xF) { bitsRemaining >>= 4; poolIndex += 4; }
            if (bitsRemaining > 0x3) { bitsRemaining >>= 2; poolIndex += 2; }
            if (bitsRemaining > 0x1) { bitsRemaining >>= 1; poolIndex += 1; }

            return poolIndex + (int)bitsRemaining;
        }

        /// <summary>
        /// Gets the buffer size for the array pool bucket.
        /// </summary>
        /// <param name="index">Index of the array pool bucket.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetBucketBufferSize(int index)
        {
            return 16 << index;
        }

        /// <summary>
        /// Gets the next power of 2.
        /// </summary>
        /// <param name="num">The number.</param>
        internal static int GetNextPowerOf2(int num)
        {
            num--;

            num |= num >> 1;
            num |= num >> 2;
            num |= num >> 4;
            num |= num >> 8;
            num |= num >> 16;

            return num + 1;
        }
    }

    /// <summary>
    /// The JSON array pool.
    /// </summary>
    internal sealed class JsonArrayPool : IArrayPool<char>
    {
        /// <summary>
        /// The instance of JSON array pool.
        /// </summary>
        public static readonly IArrayPool<char> Shared = new JsonArrayPool();

        /// <summary>
        /// Rent an array from the pool. This array must be returned when it is no longer needed.
        /// </summary>
        /// <param name="minimumLength">The minimum required length of the array. The returned array may be longer.</param>
        /// <returns>
        /// The rented array from the pool. This array must be returned when it is no longer needed.
        /// </returns>
        public char[] Rent(int minimumLength)
        {
            return SharedArrayPool<char>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// Return an array to the pool.
        /// </summary>
        /// <param name="array">The array that is being returned.</param>
        public void Return(char[] array)
        {
            SharedArrayPool<char>.Shared.Return(array);
        }
    }
}

#endif