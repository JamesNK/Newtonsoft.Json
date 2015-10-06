using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when loading JSON.
    /// </summary>
    public class JsonLoadSettings
    {
        private CommentHandling _commentHandling;

        /// <summary>
        /// Gets or sets how JSON comments are handled when loading JSON.
        /// </summary>
        /// <value>The JSON comment handling.</value>
        public CommentHandling CommentHandling
        {
            get { return _commentHandling; }
            set
            {
                if (value < CommentHandling.Ignore || value > CommentHandling.Load)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _commentHandling = value;
            }
        }
    }
}