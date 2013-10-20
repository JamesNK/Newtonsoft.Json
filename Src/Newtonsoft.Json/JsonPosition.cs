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
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    internal enum JsonContainerType
    {
        None,
        Object,
        Array,
        Constructor
    }

    internal struct JsonPosition
    {
        internal JsonContainerType Type;
        internal int Position;
        internal string PropertyName;
        internal bool HasIndex;

        public JsonPosition(JsonContainerType type)
        {
            Type = type;
            HasIndex = TypeHasIndex(type);
            Position = -1;
            PropertyName = null;
        }

        internal void WriteTo(StringBuilder sb)
        {
            switch (Type)
            {
                case JsonContainerType.Object:
                    if (sb.Length > 0)
                        sb.Append(".");
                    sb.Append(PropertyName);
                    break;
                case JsonContainerType.Array:
                case JsonContainerType.Constructor:
                    sb.Append("[");
                    sb.Append(Position);
                    sb.Append("]");
                    break;
            }
        }

        internal static bool TypeHasIndex(JsonContainerType type)
        {
            return (type == JsonContainerType.Array || type == JsonContainerType.Constructor);
        }

        internal static string BuildPath(IEnumerable<JsonPosition> positions)
        {
            StringBuilder sb = new StringBuilder();

            foreach (JsonPosition state in positions)
            {
                state.WriteTo(sb);
            }

            return sb.ToString();
        }

        internal static string FormatMessage(IJsonLineInfo lineInfo, string path, string message)
        {
            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine))
            {
                message = message.Trim();

                if (!message.EndsWith("."))
                    message += ".";

                message += " ";
            }

            message += "Path '{0}'".FormatWith(CultureInfo.InvariantCulture, path);

            if (lineInfo != null && lineInfo.HasLineInfo())
                message += ", line {0}, position {1}".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition);

            message += ".";

            return message;
        }
    }
}