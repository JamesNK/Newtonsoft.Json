using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when merging JSON.
    /// </summary>
    public class JsonMergeSettings
    {
        private MergeArrayHandling _mergeArrayHandling;

        private MergeObjectHandling _mergeObjectHandling;

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
                    throw new ArgumentOutOfRangeException("value");
                
                _mergeArrayHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets the method used when merging JSON objects.
        /// </summary>
        /// <value>The method used when merging JSON objects.</value>
        public MergeObjectHandling MergetObjectHandling
        {
            get { return _mergeObjectHandling; }
            set
            {
                if ((value | MergeObjectHandling.All) != MergeObjectHandling.All)
                    throw new ArgumentOutOfRangeException("value");

                _mergeObjectHandling = value;
            }
        }
    }
}