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

#if !(DOTNET || PORTABLE || PORTABLE40)
using System;
using System.ComponentModel;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a view of a <see cref="JProperty"/>.
    /// </summary>
    public class JPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public JPropertyDescriptor(string name)
            : base(name, null)
        {
        }

        private static JObject CastInstance(object instance)
        {
            return (JObject)instance;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value.
        /// </summary>
        /// <returns>
        /// true if resetting the component changes its value; otherwise, false.
        /// </returns>
        /// <param name="component">The component to test for reset capability. 
        ///                 </param>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, gets the current value of the property on a component.
        /// </summary>
        /// <returns>
        /// The value of a property for a given component.
        /// </returns>
        /// <param name="component">The component with the property for which to retrieve the value. 
        ///                 </param>
        public override object GetValue(object component)
        {
            JToken token = CastInstance(component)[Name];

            return token;
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be reset to the default value. 
        ///                 </param>
        public override void ResetValue(object component)
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be set. 
        ///                 </param><param name="value">The new value. 
        ///                 </param>
        public override void SetValue(object component, object value)
        {
            JToken token = (value is JToken) ? (JToken)value : new JValue(value);

            CastInstance(component)[Name] = token;
        }

        /// <summary>
        /// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
        /// </summary>
        /// <returns>
        /// true if the property should be persisted; otherwise, false.
        /// </returns>
        /// <param name="component">The component with the property to be examined for persistence. 
        ///                 </param>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the component this property is bound to.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of component this property is bound to. When the <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"/> or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"/> methods are invoked, the object specified might be an instance of this type.
        /// </returns>
        public override Type ComponentType
        {
            get { return typeof(JObject); }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether this property is read-only.
        /// </summary>
        /// <returns>
        /// true if the property is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the property.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the property.
        /// </returns>
        public override Type PropertyType
        {
            get { return typeof(object); }
        }

        /// <summary>
        /// Gets the hash code for the name of the member.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The hash code for the name of the member.
        /// </returns>
        protected override int NameHashCode
        {
            get
            {
                // override property to fix up an error in its documentation
                int nameHashCode = base.NameHashCode;
                return nameHashCode;
            }
        }
    }
}

#endif