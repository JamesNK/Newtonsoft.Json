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
        /// <returns>A new merged token</returns>
        public static JToken Merge(
            this JToken left, JToken right)
        {
            if (left.Type != JTokenType.Object)
                return right.DeepClone();

            var leftClone = (JContainer)left.DeepClone();
            MergeInto(leftClone, right);

            return leftClone;
        }

        /// <summary>
        /// <para>Merge the right token into the left</para>
        /// </summary>
        /// <param name="left">Token to be merged into</param>
        /// <param name="right">Token to merge, overwriting the left</param>
        public static void MergeInto(
            this JContainer left, JToken right)
        {
            foreach (var rightChild in right.Children<JProperty>())
            {
                var rightChildProperty = rightChild;
                var leftProperty = left.SelectToken(rightChildProperty.Name);

                if (leftProperty == null)
                {
                    // no matching property, just add 
                    left.Add(rightChild);
                }
                else
                {
                    var leftObject = leftProperty as JObject;
                    if (leftObject == null)
                    {
                        // replace value
                        var leftParent = (JProperty) leftProperty.Parent;
                        leftParent.Value = rightChildProperty.Value;
                    }

                    else
                        // recurse object
                        MergeInto(leftObject, rightChildProperty.Value);
                }
            }
        }
    }
}