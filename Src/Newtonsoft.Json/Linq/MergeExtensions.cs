namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// <para>Extensions for merging JTokens</para>
    /// </summary>
    public static class MergeExtensions
    {
        /// <summary>
        /// <para>Creates a new token which is the merge of the passed tokens</para>
        /// </summary>
        /// <param name="left">Token</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <param name="options">Options for merge</param>
        /// <returns>A new merged token</returns>
        public static JToken Merge(
            this JToken left, JToken right, MergeOptions options)
        {
            if (left.Type != JTokenType.Object)
                return right.DeepClone();

            var leftClone = (JContainer) left.DeepClone();
            MergeInto(leftClone, right, options);

            return leftClone;
        }

        /// <summary>
        /// <para>Creates a new token which is the merge of the passed tokens</para>
        /// <para>Default options are used</para>
        /// </summary>
        /// <param name="left">Token</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <returns>A new merged token</returns>
        public static JToken Merge(
            this JToken left, JToken right)
        {
            return Merge(left, right, MergeOptions.Default);
        }

        /// <summary>
        /// <para>Merge the right token into the left</para>
        /// </summary>
        /// <param name="left">Token to be merged into</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        /// <param name="options">Options for merge</param>
        public static void MergeInto(
            this JContainer left, JToken right, MergeOptions options)
        {
            foreach (var rightChild in right.Children<JProperty>())
            {
                var rightChildProperty = rightChild;
                var leftPropertyValue = left.SelectToken(rightChildProperty.Name);

                if (leftPropertyValue == null)
                {
                    // no matching property, just add 
                    left.Add(rightChild);
                }
                else
                {
                    var leftProperty = (JProperty) leftPropertyValue.Parent;

                    var leftArray = leftPropertyValue as JArray;
                    var rightArray = rightChildProperty.Value as JArray;
                    if (leftArray != null && rightArray != null)
                    {
                        switch (options.ArrayHandling)
                        {
                            case MergeOptionArrayHandling.Concat:

                                foreach (var rightValue in rightArray)
                                {
                                    leftArray.Add(rightValue);
                                }

                                break;
                            case MergeOptionArrayHandling.Overwrite:

                                leftProperty.Value = rightChildProperty.Value;
                                break;
                        }
                    }

                    else
                    {
                        var leftObject = leftPropertyValue as JObject;
                        if (leftObject == null)
                        {
                            // replace value
                            leftProperty.Value = rightChildProperty.Value;
                        }

                        else
                            // recurse object
                            MergeInto(leftObject, rightChildProperty.Value, options);
                    }
                }
            }
        }

        /// <summary>
        /// <para>Merge the right token into the left</para>
        /// <para>Default options are used</para>
        /// </summary>
        /// <param name="left">Token to be merged into</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        public static void MergeInto(
            this JContainer left, JToken right)
        {
            MergeInto(left, right, MergeOptions.Default);
        }
    }
}