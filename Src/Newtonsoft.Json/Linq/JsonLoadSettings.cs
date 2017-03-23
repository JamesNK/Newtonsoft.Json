using System;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when loading JSON.
    /// </summary>
    public class JsonLoadSettings
    {
        private CommentHandling _commentHandling;
        private LineInfoHandling _lineInfoHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLoadSettings"/> class.
        /// </summary>
        public JsonLoadSettings()
        {
            _lineInfoHandling = LineInfoHandling.Load;
            _commentHandling = CommentHandling.Ignore;
        }

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
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _commentHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how JSON line info is handled when loading JSON.
        /// </summary>
        /// <value>The JSON line info handling.</value>
        public LineInfoHandling LineInfoHandling
        {
            get { return _lineInfoHandling; }
            set
            {
                if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _lineInfoHandling = value;
            }
        }
    }
}