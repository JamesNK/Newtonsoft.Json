using NUnit.Framework.Constraints;
using System;

namespace Newtonsoft.Json.Tests
{
    internal class Js
    {
        internal static IResolveConstraint IsEqualTo(Object expected)
        {
            return new JsonEqualsConstraint(expected);
        }
    }
}
