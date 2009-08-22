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

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.ComponentModel
{
  /// <summary>
  /// Represents a view of a <see cref="JObject"/>.
  /// </summary>
  public class JTypeDescriptor : ICustomTypeDescriptor
  {
    private readonly JObject _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="JTypeDescriptor"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public JTypeDescriptor(JObject value)
    {
      ValidationUtils.ArgumentNotNull(value, "value");
      _value = value;
    }

    /// <summary>
    /// Returns the properties for this instance of a component.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the properties for this component instance.
    /// </returns>
    public virtual PropertyDescriptorCollection GetProperties()
    {
      return GetProperties(null);
    }

    private static Type GetTokenPropertyType(JToken token)
    {
      if (token is JValue)
      {
        JValue v = (JValue) token;
        return (v.Value != null) ? v.Value.GetType() : typeof (object);
      }

      return token.GetType();
    }

    /// <summary>
    /// Returns the properties for this instance of a component using the attribute array as a filter.
    /// </summary>
    /// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
    /// <returns>
    /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the filtered properties for this component instance.
    /// </returns>
    public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
      PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);

      if (_value != null)
      {
        foreach (KeyValuePair<string, JToken> propertyValue in _value)
        {
          descriptors.Add(new JPropertyDescriptor(propertyValue.Key, GetTokenPropertyType(propertyValue.Value)));
        }
      }

      return descriptors;
    }

    /// <summary>
    /// Returns a collection of custom attributes for this instance of a component.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.ComponentModel.AttributeCollection"/> containing the attributes for this object.
    /// </returns>
    public AttributeCollection GetAttributes()
    {
      return AttributeCollection.Empty;
    }

    /// <summary>
    /// Returns the class name of this instance of a component.
    /// </summary>
    /// <returns>
    /// The class name of the object, or null if the class does not have a name.
    /// </returns>
    public string GetClassName()
    {
      return null;
    }

    /// <summary>
    /// Returns the name of this instance of a component.
    /// </summary>
    /// <returns>
    /// The name of the object, or null if the object does not have a name.
    /// </returns>
    public string GetComponentName()
    {
      return null;
    }

    /// <summary>
    /// Returns a type converter for this instance of a component.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.ComponentModel.TypeConverter"/> that is the converter for this object, or null if there is no <see cref="T:System.ComponentModel.TypeConverter"/> for this object.
    /// </returns>
    public TypeConverter GetConverter()
    {
      return new TypeConverter();
    }

    /// <summary>
    /// Returns the default event for this instance of a component.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.ComponentModel.EventDescriptor"/> that represents the default event for this object, or null if this object does not have events.
    /// </returns>
    public EventDescriptor GetDefaultEvent()
    {
      return null;
    }

    /// <summary>
    /// Returns the default property for this instance of a component.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the default property for this object, or null if this object does not have properties.
    /// </returns>
    public PropertyDescriptor GetDefaultProperty()
    {
      return null;
    }

    /// <summary>
    /// Returns an editor of the specified type for this instance of a component.
    /// </summary>
    /// <param name="editorBaseType">A <see cref="T:System.Type"/> that represents the editor for this object.</param>
    /// <returns>
    /// An <see cref="T:System.Object"/> of the specified type that is the editor for this object, or null if the editor cannot be found.
    /// </returns>
    public object GetEditor(Type editorBaseType)
    {
      return null;
    }

    /// <summary>
    /// Returns the events for this instance of a component using the specified attribute array as a filter.
    /// </summary>
    /// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
    /// <returns>
    /// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the filtered events for this component instance.
    /// </returns>
    public EventDescriptorCollection GetEvents(Attribute[] attributes)
    {
      return EventDescriptorCollection.Empty;
    }

    /// <summary>
    /// Returns the events for this instance of a component.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.ComponentModel.EventDescriptorCollection"/> that represents the events for this component instance.
    /// </returns>
    public EventDescriptorCollection GetEvents()
    {
      return EventDescriptorCollection.Empty;
    }

    /// <summary>
    /// Returns an object that contains the property described by the specified property descriptor.
    /// </summary>
    /// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that represents the property whose owner is to be found.</param>
    /// <returns>
    /// An <see cref="T:System.Object"/> that represents the owner of the specified property.
    /// </returns>
    public object GetPropertyOwner(PropertyDescriptor pd)
    {
      return null;
    }
  }
}
#endif