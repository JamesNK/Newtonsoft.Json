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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
    /// </summary>
    public class JTokenReader : JsonReader, IJsonLineInfo
    {
        private readonly string _initialPath;
        private readonly JToken _root;
        private JToken _parent;
        private JToken _current;

        /// <summary>
        /// Gets the <see cref="JToken"/> at the reader's current position.
        /// </summary>
        public JToken CurrentToken
        {
            get { return _current; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JTokenReader"/> class.
        /// </summary>
        /// <param name="token">The token to read from.</param>
        public JTokenReader(JToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            _root = token;
        }

        internal JTokenReader(JToken token, string initialPath)
            : this(token)
        {
            _initialPath = initialPath;
        }

        /// <summary>
        /// Reads the next JSON token from the stream.
        /// </summary>
        /// <returns>
        /// true if the next token was read successfully; false if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            if (CurrentState != State.Start)
            {
                if (_current == null)
                {
                    return false;
                }

                JContainer container = _current as JContainer;
                if (container != null && _parent != container)
                {
                    return ReadInto(container);
                }
                else
                {
                    return ReadOver(_current);
                }
            }

            _current = _root;
            SetToken(_current);
            return true;
        }

        private bool ReadOver(JToken t)
        {
            if (t == _root)
            {
                return ReadToEnd();
            }

            JToken next = t.Next;
            if ((next == null || next == t) || t == t.Parent.Last)
            {
                if (t.Parent == null)
                {
                    return ReadToEnd();
                }

                return SetEnd(t.Parent);
            }
            else
            {
                _current = next;
                SetToken(_current);
                return true;
            }
        }

        private bool ReadToEnd()
        {
            _current = null;
            SetToken(JsonToken.None);
            return false;
        }

        private JsonToken? GetEndToken(JContainer c)
        {
            switch (c.Type)
            {
                case JTokenType.Object:
                    return JsonToken.EndObject;
                case JTokenType.Array:
                    return JsonToken.EndArray;
                case JTokenType.Constructor:
                    return JsonToken.EndConstructor;
                case JTokenType.Property:
                    return null;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type.");
            }
        }

        private bool ReadInto(JContainer c)
        {
            JToken firstChild = c.First;
            if (firstChild == null)
            {
                return SetEnd(c);
            }
            else
            {
                SetToken(firstChild);
                _current = firstChild;
                _parent = c;
                return true;
            }
        }

        private bool SetEnd(JContainer c)
        {
            JsonToken? endToken = GetEndToken(c);
            if (endToken != null)
            {
                SetToken(endToken.GetValueOrDefault());
                _current = c;
                _parent = c;
                return true;
            }
            else
            {
                return ReadOver(c);
            }
        }

        private void SetToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    SetToken(JsonToken.StartObject);
                    break;
                case JTokenType.Array:
                    SetToken(JsonToken.StartArray);
                    break;
                case JTokenType.Constructor:
                    SetToken(JsonToken.StartConstructor, ((JConstructor)token).Name);
                    break;
                case JTokenType.Property:
                    SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
                    break;
                case JTokenType.Comment:
                    SetToken(JsonToken.Comment, ((JValue)token).Value);
                    break;
                case JTokenType.Integer:
                    SetToken(JsonToken.Integer, ((JValue)token).Value);
                    break;
                case JTokenType.Float:
                    SetToken(JsonToken.Float, ((JValue)token).Value);
                    break;
                case JTokenType.String:
                    SetToken(JsonToken.String, ((JValue)token).Value);
                    break;
                case JTokenType.Boolean:
                    SetToken(JsonToken.Boolean, ((JValue)token).Value);
                    break;
                case JTokenType.Null:
                    SetToken(JsonToken.Null, ((JValue)token).Value);
                    break;
                case JTokenType.Undefined:
                    SetToken(JsonToken.Undefined, ((JValue)token).Value);
                    break;
                case JTokenType.Date:
                    SetToken(JsonToken.Date, ((JValue)token).Value);
                    break;
                case JTokenType.Raw:
                    SetToken(JsonToken.Raw, ((JValue)token).Value);
                    break;
                case JTokenType.Bytes:
                    SetToken(JsonToken.Bytes, ((JValue)token).Value);
                    break;
                case JTokenType.Guid:
                    SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
                    break;
                case JTokenType.Uri:
                    object v = ((JValue)token).Value;
                    if (v is Uri)
                    {
                        SetToken(JsonToken.String, ((Uri)v).OriginalString);
                    }
                    else
                    {
                        SetToken(JsonToken.String, SafeToString(v));
                    }
                    break;
                case JTokenType.TimeSpan:
                    SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
            }
        }

        private string SafeToString(object value)
        {
            return (value != null) ? value.ToString() : null;
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            if (CurrentState == State.Start)
            {
                return false;
            }

            IJsonLineInfo info = _current;
            return (info != null && info.HasLineInfo());
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo info = _current;
                if (info != null)
                {
                    return info.LineNumber;
                }

                return 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo info = _current;
                if (info != null)
                {
                    return info.LinePosition;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the path of the current JSON token. 
        /// </summary>
        public override string Path
        {
            get
            {
                string path = base.Path;

                if (!string.IsNullOrEmpty(_initialPath))
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return _initialPath;
                    }

                    if (path.StartsWith('['))
                    {
                        path = _initialPath + path;
                    }
                    else
                    {
                        path = _initialPath + "." + path;
                    }
                }

                return path;
            }
        }
    }
}