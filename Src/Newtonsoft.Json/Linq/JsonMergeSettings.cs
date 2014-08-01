using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when merging JSON.
    /// </summary>
    public class JsonMergeSettings
    {
        private MergeArrayHandling _mergeArrayHandling;

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
    }
}