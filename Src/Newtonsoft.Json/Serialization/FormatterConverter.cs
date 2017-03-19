using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

#if HAVE_BINARY_SERIALIZATION && !HAVE_BINARY_FORMATTER

namespace Newtonsoft.Json.Serialization
{
    internal class FormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        public object Convert(object value, TypeCode typeCode)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
        }

        public bool ToBoolean(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        public byte ToByte(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        public char ToChar(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }

        public DateTime ToDateTime(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        public decimal ToDecimal(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public double ToDouble(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public short ToInt16(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        public int ToInt32(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public long ToInt64(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        public sbyte ToSByte(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        public float ToSingle(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        public string ToString(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public ushort ToUInt16(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        public uint ToUInt32(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        public ulong ToUInt64(object value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));
            return System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }
    }
}

#endif