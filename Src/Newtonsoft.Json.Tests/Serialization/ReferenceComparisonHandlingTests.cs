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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class ReferenceComparisonHandlingTests : TestFixtureBase
    {
        [Test]
        public void ShouldUseObjectEqualsByDefault()
        {
            bool equalsCalled = false;
            var source = new SettingNotSpecified(() => equalsCalled = true)
            {
                Value = new object()
            };

            Assert.Throws<JsonSerializationException>(delegate
            {
                JsonConvert.SerializeObject(source, Formatting.Indented);
            });
            Assert.IsTrue(equalsCalled);
        }

        [Test]
        public void ShouldUseTheJsonSerializerSetting()
        {
            bool equalsCalled = false;
            var source = new SettingNotSpecified(() => equalsCalled = true)
            {
                Value = new object()
            };

            JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceComparisonHandling = ReferenceComparisonHandling.ReferenceEquals
            });

            Assert.IsFalse(equalsCalled);
        }

        [Test]
        public void ShouldUseTheJsonPropertyAttributeSetting()
        {
            bool equalsCalled = false;
            var source = new SettingSpecifiedAtThePropertyLevel(() => equalsCalled = true)
            {
                Value = new object()
            };

            JsonConvert.SerializeObject(source, Formatting.Indented);

            Assert.IsFalse(equalsCalled);
        }

        [Test]
        public void ShouldUseTheJsonObjectAttributeSetting()
        {
            bool equalsCalled = false;
            var source = new SettingSpecifiedAtTheClassLevel(() => equalsCalled = true)
            {
                Value = new object()
            };

            JsonConvert.SerializeObject(source, Formatting.Indented);

            Assert.IsFalse(equalsCalled);
        }

        [Test]
        public void ShouldUseTheJsonPropertyAttributeSettingsForTheContaintedItems()
        {
            bool equalsCalled = false;
            var source = new SettingSpecifiedAtThePropertyLevelForItemContainedInTheProperty
            {
                Value = new SettingSpecifiedAtTheClassLevel(() => equalsCalled = true)
                {
                    Value = new object()
                }
            };

            JsonConvert.SerializeObject(source, Formatting.Indented);

            Assert.IsFalse(equalsCalled);
        }
    }

    public class SettingNotSpecified
    {
        public virtual object Value { get; set; }

        public SettingNotSpecified(Action onEquals = null)
        {
            _onEquals = onEquals;
        }

        public override bool Equals(object obj)
        {
            if(_onEquals != null)_onEquals();

            if (obj == null)
                return false;

            var otherContainer = obj as SettingNotSpecified;

            if (otherContainer != null)
                return EqualityComparer<object>.Default.Equals(Value, otherContainer.Value);

            return EqualityComparer<object>.Default.Equals(Value, obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<object>.Default.GetHashCode(Value);
        }
        
        [JsonIgnore]
        internal Action _onEquals;
    }

    [JsonObject(ItemReferenceComparisonHandling = ReferenceComparisonHandling.ReferenceEquals)]
    public class SettingSpecifiedAtTheClassLevel : SettingNotSpecified
    {
        public SettingSpecifiedAtTheClassLevel(Action onEquals = null)
            : base(onEquals)
        {

        }
    }

    public class SettingSpecifiedAtThePropertyLevel : SettingNotSpecified
    {
        [JsonProperty(ReferenceComparisonHandling = ReferenceComparisonHandling.ReferenceEquals)]
        public override object Value { get; set; }

        public SettingSpecifiedAtThePropertyLevel(Action onEquals = null) 
            : base(onEquals)
        {
        }
    }

    public class SettingSpecifiedAtThePropertyLevelForItemContainedInTheProperty
    {
        [JsonProperty(ItemReferenceComparisonHandling = ReferenceComparisonHandling.ReferenceEquals)]
        public SettingNotSpecified Value;
    }
}