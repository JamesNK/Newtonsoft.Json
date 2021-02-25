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

#if HAVE_ASYNC

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    public abstract partial class JContainer
    {
        internal async Task ReadTokenFromAsync(JsonReader reader, JsonLoadSettings? options, CancellationToken cancellationToken = default)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            int startDepth = reader.Depth;

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }

            await ReadContentFromAsync(reader, options, cancellationToken).ConfigureAwait(false);

            if (reader.Depth > startDepth)
            {
                throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }
        }

        private async Task ReadContentFromAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;

            JContainer? parent = this;

            do
            {
                if (parent is JProperty p && p.Value != null)
                {
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                }

                MiscellaneousUtils.Assert(parent != null);

                switch (reader.TokenType)
                {
                    case JsonToken.None:
                        // new reader. move to actual content
                        break;
                    case JsonToken.StartArray:
                        JArray a = new JArray();
                        a.SetLineInfo(lineInfo, settings);
                        parent.Add(a);
                        parent = a;
                        break;

                    case JsonToken.EndArray:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartObject:
                        JObject o = new JObject();
                        o.SetLineInfo(lineInfo, settings);
                        parent.Add(o);
                        parent = o;
                        break;
                    case JsonToken.EndObject:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartConstructor:
                        JConstructor constructor = new JConstructor(reader.Value!.ToString());
                        constructor.SetLineInfo(lineInfo, settings);
                        parent.Add(constructor);
                        parent = constructor;
                        break;
                    case JsonToken.EndConstructor:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                        JValue v = new JValue(reader.Value);
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Comment:
                        if (settings != null && settings.CommentHandling == CommentHandling.Load)
                        {
                            v = JValue.CreateComment(reader.Value!.ToString());
                            v.SetLineInfo(lineInfo, settings);
                            parent.Add(v);
                        }
                        break;
                    case JsonToken.Null:
                        v = JValue.CreateNull();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Undefined:
                        v = JValue.CreateUndefined();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.PropertyName:
                        JProperty? property = ReadProperty(reader, settings, lineInfo, parent);
                        if (property != null)
                        {
                            parent = property;
                        }
                        else
                        {
                            await reader.SkipAsync().ConfigureAwait(false);
                        }
                        break;
                    default:
                        throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
                }
            } while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}

#endif
