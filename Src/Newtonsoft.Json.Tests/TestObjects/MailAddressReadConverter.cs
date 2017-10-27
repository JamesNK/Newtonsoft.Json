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
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.TestObjects
{
#if !(DNXCORE50) || NETSTANDARD2_0
    public class MailAddressReadConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(System.Net.Mail.MailAddress);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var messageJObject = serializer.Deserialize<JObject>(reader);
            if (messageJObject == null)
            {
                return null;
            }

            var address = messageJObject.GetValue("Address", StringComparison.OrdinalIgnoreCase).ToObject<string>();

            JToken displayNameToken;
            string displayName;
            if (messageJObject.TryGetValue("DisplayName", StringComparison.OrdinalIgnoreCase, out displayNameToken)
                && !string.IsNullOrEmpty(displayName = displayNameToken.ToObject<string>()))
            {
                return new System.Net.Mail.MailAddress(address, displayName);
            }

            return new System.Net.Mail.MailAddress(address);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
#endif
}