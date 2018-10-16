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

#if !NET20
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1566 : TestFixtureBase
    {
        [Test]
        public void Github_deserialize_pr_state_should_be_case_insensitive()
        {
            // Arrange
            var jsonWithUppercase = "{\"state\": \"APPROVED\"}";
            var jsonWithLowercase = "{\"state\": \"approved\"}";

            // Act
            var jsonObjectWithUppercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithUppercase);
            var jsonObjectWithLowercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithLowercase);

            // Assert
            Assert.AreEqual(GitHubPullRequestReviewState.Approved, jsonObjectWithUppercase.State);
            Assert.AreEqual(GitHubPullRequestReviewState.Approved, jsonObjectWithLowercase.State);
        }

        [Test]
        public void Github_deserialize_pr_state_changes_requested_should_be_case_insensitive()
        {
            // Arrange
            var jsonWithUppercase = "{\"state\": \"CHANGES_REQUESTED\"}";
            var jsonWithLowercase = "{\"state\": \"changes_requested\"}";

            // Act
            var jsonObjectWithUppercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithUppercase);
            var jsonObjectWithLowercase = JsonConvert.DeserializeObject<GitHubPullRequestReview>(jsonWithLowercase);

            // Assert
            Assert.AreEqual(GitHubPullRequestReviewState.ChangesRequested, jsonObjectWithUppercase.State);
            Assert.AreEqual(GitHubPullRequestReviewState.ChangesRequested, jsonObjectWithLowercase.State);
        }

        public enum GitHubPullRequestReviewState
        {
            [EnumMember(Value = "approved")]
            Approved,

            [EnumMember(Value = "changes_requested")]
            ChangesRequested,

            [EnumMember(Value = "commented")]
            Commented,

            [EnumMember(Value = "dismissed")]
            Dismissed,

            [EnumMember(Value = "pending")]
            Pending
        }

        public class GitHubPullRequestReview
        {
            [JsonProperty("state")]
            [JsonConverter(typeof(StringEnumConverter))]
            public GitHubPullRequestReviewState State;
        }
    }
}
#endif