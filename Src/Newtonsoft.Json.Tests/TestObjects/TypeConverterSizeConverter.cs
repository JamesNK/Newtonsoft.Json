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
using System.ComponentModel;
using System.Globalization;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class TypeConverterSizeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (str == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str2 = str.Trim();
            if (str2.Length == 0)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }
            string[] strArray = str2.Split(',');
            int[] numArray = new int[strArray.Length];
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = (int)converter.ConvertFromString(context, culture, strArray[i]);
            }
            if (numArray.Length == 2)
            {
                return new TypeConverterSize(numArray[0], numArray[1]);
            }

            throw new ArgumentException("Bad format.");
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (value is TypeConverterSize)
            {
                if (destinationType == typeof(string))
                {
                    TypeConverterSize size = (TypeConverterSize)value;
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
                    string[] strArray = new string[2];
                    int num = 0;
                    strArray[num++] = converter.ConvertToString(context, culture, size.Width);
                    strArray[num++] = converter.ConvertToString(context, culture, size.Height);
                    return string.Join(", ", strArray);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}