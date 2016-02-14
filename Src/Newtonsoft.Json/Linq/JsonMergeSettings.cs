using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when merging JSON.
    /// </summary>
    public class JsonMergeSettings
    {
        private MergeArrayHandling _mergeArrayHandling;
        private MergeNullValueHandling _mergeNullValueHandling;

        /// <summary>
        /// Gets or sets the method used when merging JSON arrays.
        /// </summary>
        /// <value>The method used when merging JSON arrays.</value>
        public MergeArrayHandling MergeArrayHandling
        {
            get { return _mergeArrayHandling; }
            set
            {
                if (value < MergeArrayHandling.Concat || value > MergeArrayHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeArrayHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how how null value properties are merged.
        /// </summary>
        /// <value>How null value properties are merged.</value>
        public MergeNullValueHandling MergeNullValueHandling
        {
            get { return _mergeNullValueHandling; }
            set
            {
                if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeNullValueHandling = value;
            }
        }
    }
}