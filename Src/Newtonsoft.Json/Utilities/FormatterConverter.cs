#if NETSTANDARD1_3

#region License, Terms and Author(s)
//
// FormatterConverter
// Copyright (c) 2016 Fabrício Godoy, .NET Foundation. All rights reserved.
//
//  Author(s):
//
//      Fabrício Godoy, https://github.com/skarllot/
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Utilities
{
    /// <summary>
    /// Represents a base implementation of the <see cref="IFormatterConverter"/>
    /// interface that uses the <see cref="System.Convert"/> class.
    /// </summary>
    internal class FormatterConverter : IFormatterConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatterConverter"/> class.
        /// </summary>
        public FormatterConverter()
        { }

        /// <summary>
        /// Converts a value to the given <see cref="Type"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="type">The <see cref="Type"/> into which <paramref name="value"/> is converted.</param>
        /// <returns>The converted <paramref name="value"/> or null if the <paramref name="type"/> parameter is null.</returns>
        public object Convert(object value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to the given <see cref="TypeCode"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <param name="typeCode">The <see cref="TypeCode"/> into which <paramref name="value"/> is converted.</param>
        /// <returns>The converted <paramref name="value"/> or null if the <paramref name="typeCode"/> parameter is <see cref="TypeCode.Empty"/>.</returns>
        public object Convert(object value, TypeCode typeCode)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public bool ToBoolean(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="char"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public char ToChar(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="sbyte"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public sbyte ToSByte(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToSByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public byte ToByte(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="short"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public short ToInt16(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public ushort ToUInt16(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public int ToInt32(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="uint"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public uint ToUInt32(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public long ToInt64(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="ulong"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public ulong ToUInt64(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public float ToSingle(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public double ToDouble(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public decimal ToDecimal(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public DateTime ToDateTime(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The converted <paramref name="value"/>.</returns>
        public string ToString(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}

#endif
