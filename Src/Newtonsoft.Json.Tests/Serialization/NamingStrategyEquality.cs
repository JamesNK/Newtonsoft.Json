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

using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class NamingStrategyEquality: TestFixtureBase
    {
        [Test]
        public void CamelCaseNamingStrategyEquality()
        {
            var s1 = new CamelCaseNamingStrategy();
            var s2 = new CamelCaseNamingStrategy();
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }

        [Test]
        public void CamelCaseNamingStrategyEqualityVariants()
        {
            CheckInequality<CamelCaseNamingStrategy>(false, false, true);
            CheckInequality<CamelCaseNamingStrategy>(false, true, false);
            CheckInequality<CamelCaseNamingStrategy>(true, false, false);
            CheckInequality<CamelCaseNamingStrategy>(false, true, true);
            CheckInequality<CamelCaseNamingStrategy>(true, true, false);
            CheckInequality<CamelCaseNamingStrategy>(true, true, true);
        }

        [Test]
        public void DefaultNamingStrategyEquality()
        {
            var s1 = new DefaultNamingStrategy();
            var s2 = new DefaultNamingStrategy();
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }

        [Test]
        public void DefaultNamingStrategyEqualityVariants()
        {
            CheckInequality<DefaultNamingStrategy>(false, false, true);
            CheckInequality<DefaultNamingStrategy>(false, true, false);
            CheckInequality<DefaultNamingStrategy>(true, false, false);
            CheckInequality<DefaultNamingStrategy>(false, true, true);
            CheckInequality<DefaultNamingStrategy>(true, true, false);
            CheckInequality<DefaultNamingStrategy>(true, true, true);
        }

        [Test]
        public void SnakeCaseStrategyEquality()
        {
            var s1 = new SnakeCaseNamingStrategy();
            var s2 = new SnakeCaseNamingStrategy();
            Assert.IsTrue(s1.Equals(s2));
            Assert.IsTrue(s1.GetHashCode() == s2.GetHashCode());
        }

        [Test]
        public void SnakeCaseNamingStrategyEqualityVariants()
        {
            CheckInequality<SnakeCaseNamingStrategy>(false, false, true);
            CheckInequality<SnakeCaseNamingStrategy>(false, true, false);
            CheckInequality<SnakeCaseNamingStrategy>(true, false, false);
            CheckInequality<SnakeCaseNamingStrategy>(false, true, true);
            CheckInequality<SnakeCaseNamingStrategy>(true, true, false);
            CheckInequality<SnakeCaseNamingStrategy>(true, true, true);
        }


        [Test]
        public void DifferentStrategyEquality()
        {
            NamingStrategy s1 = new SnakeCaseNamingStrategy();
            NamingStrategy s2 = new DefaultNamingStrategy();
            Assert.IsFalse(s1.Equals(s2));
            Assert.IsFalse(s1.GetHashCode() == s2.GetHashCode());
        }

        private void CheckInequality<T>(bool overrideSpecifiedNames, bool processDictionaryKeys, bool processExtensionDataNames)
            where T : NamingStrategy, new()
        {
            var s1 = new T
            {
                OverrideSpecifiedNames = false,
                ProcessDictionaryKeys = false,
                ProcessExtensionDataNames = false
            };

            var s2 = new T
            {
                OverrideSpecifiedNames = overrideSpecifiedNames,
                ProcessDictionaryKeys = processDictionaryKeys,
                ProcessExtensionDataNames = processExtensionDataNames
            };

            Assert.IsFalse(s1.Equals(s2));
            Assert.IsFalse(s1.GetHashCode() == s2.GetHashCode());
        }
    }
}
