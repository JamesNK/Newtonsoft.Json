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

#if HAVE_REFLECTION_EMIT
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Get and set values for a <see cref="MemberInfo"/> using dynamic methods.
    /// </summary>
    public class DynamicValueProvider : IValueProvider
    {
        private readonly MemberInfo _memberInfo;
        private Func<object, object> _getter;
        private Action<object, object> _setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicValueProvider"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public DynamicValueProvider(MemberInfo memberInfo)
        {
            ValidationUtils.ArgumentNotNull(memberInfo, nameof(memberInfo));
            _memberInfo = memberInfo;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target to set the value on.</param>
        /// <param name="value">The value to set on the target.</param>
        public void SetValue(object target, object value)
        {
            try
            {
                if (_setter == null)
                {
                    _setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(_memberInfo);
                }

#if DEBUG
                // dynamic method doesn't check whether the type is 'legal' to set
                // add this check for unit tests
                if (value == null)
                {
                    if (!ReflectionUtils.IsNullable(ReflectionUtils.GetMemberUnderlyingType(_memberInfo)))
                    {
                        throw new JsonSerializationException("Incompatible value. Cannot set {0} to null.".FormatWith(CultureInfo.InvariantCulture, _memberInfo));
                    }
                }
                else if (!ReflectionUtils.GetMemberUnderlyingType(_memberInfo).IsAssignableFrom(value.GetType()))
                {
                    throw new JsonSerializationException("Incompatible value. Cannot set {0} to type {1}.".FormatWith(CultureInfo.InvariantCulture, _memberInfo, value.GetType()));
                }
#endif

                _setter(target, value);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="target">The target to get the value from.</param>
        /// <returns>The value.</returns>
        public object GetValue(object target)
        {
            try
            {
                if (_getter == null)
                {
                    _getter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(_memberInfo);
                }

                return _getter(target);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }
    }
}

#endif