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

#if !(NET20 || NET35 || NET40 || NETFX_CORE || PORTABLE40 || DNXCORE50)
using System.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Bson;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests
{
    [Serializable]
    [DataContract]
    public class Image
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public byte[] Data { get; set; }
    }

    [TestFixture]
    public class PerformanceTests : TestFixtureBase
    {
#if DEBUG
        public int Iterations = 1;
#else
        public int Iterations = 100;
        //public int Iterations = 10000;
#endif

        #region Data
        private const string BsonHex =
            @"A9-01-00-00-04-73-74-72-69-6E-67-73-00-2B-00-00-00-0A-30-00-02-31-00-19-00-00-00-4D-61-72-6B-75-73-20-65-67-67-65-72-20-5D-3E-3C-5B-2C-20-28-32-6E-64-29-00-0A-32-00-00-03-64-69-63-74-69-6F-6E-61-72-79-00-37-00-00-00-10-56-61-6C-20-26-20-61-73-64-31-00-01-00-00-00-10-56-61-6C-32-20-26-20-61-73-64-31-00-03-00-00-00-10-56-61-6C-33-20-26-20-61-73-64-31-00-04-00-00-00-00-02-4E-61-6D-65-00-05-00-00-00-52-69-63-6B-00-09-4E-6F-77-00-EF-BD-69-EC-25-01-00-00-01-42-69-67-4E-75-6D-62-65-72-00-E7-7B-CC-26-96-C7-1F-42-03-41-64-64-72-65-73-73-31-00-47-00-00-00-02-53-74-72-65-65-74-00-0B-00-00-00-66-66-66-20-53-74-72-65-65-74-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-6F-FF-31-53-26-01-00-00-00-04-41-64-64-72-65-73-73-65-73-00-A2-00-00-00-03-30-00-4B-00-00-00-02-53-74-72-65-65-74-00-0F-00-00-00-1F-61-72-72-61-79-3C-61-64-64-72-65-73-73-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-6F-73-0C-E7-25-01-00-00-00-03-31-00-4C-00-00-00-02-53-74-72-65-65-74-00-10-00-00-00-61-72-72-61-79-20-32-20-61-64-64-72-65-73-73-00-02-50-68-6F-6E-65-00-0F-00-00-00-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-00-09-45-6E-74-65-72-65-64-00-6F-17-E6-E1-25-01-00-00-00-00-00";

        private const string BinaryFormatterHex =
            @"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-4C-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2C-20-56-65-72-73-69-6F-6E-3D-33-2E-35-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-05-01-00-00-00-1F-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-54-65-73-74-43-6C-61-73-73-07-00-00-00-05-5F-4E-61-6D-65-04-5F-4E-6F-77-0A-5F-42-69-67-4E-75-6D-62-65-72-09-5F-41-64-64-72-65-73-73-31-0A-5F-41-64-64-72-65-73-73-65-73-07-73-74-72-69-6E-67-73-0A-64-69-63-74-69-6F-6E-61-72-79-01-00-00-04-03-03-03-0D-05-1D-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-02-00-00-00-90-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-2C-20-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2C-20-56-65-72-73-69-6F-6E-3D-33-2E-35-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-7F-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-E1-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-44-69-63-74-69-6F-6E-61-72-79-60-32-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-2C-5B-53-79-73-74-65-6D-2E-49-6E-74-33-32-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-02-00-00-00-06-03-00-00-00-04-52-69-63-6B-B6-25-3A-D1-C5-59-CC-88-0F-33-34-31-32-33-31-32-33-31-32-33-2E-31-32-31-09-04-00-00-00-09-05-00-00-00-09-06-00-00-00-09-07-00-00-00-05-04-00-00-00-1D-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-03-00-00-00-07-5F-73-74-72-65-65-74-06-5F-50-68-6F-6E-65-08-5F-45-6E-74-65-72-65-64-01-01-00-0D-02-00-00-00-06-08-00-00-00-0A-66-66-66-20-53-74-72-65-65-74-06-09-00-00-00-0E-28-35-30-33-29-20-38-31-34-2D-36-33-33-35-B6-BD-B8-BF-74-69-CC-88-04-05-00-00-00-90-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-2C-20-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2C-20-56-65-72-73-69-6F-6E-3D-33-2E-35-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-04-00-00-1F-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-5B-5D-02-00-00-00-08-08-09-0A-00-00-00-02-00-00-00-02-00-00-00-04-06-00-00-00-7F-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-03-00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-06-00-00-08-08-09-0B-00-00-00-03-00-00-00-03-00-00-00-04-07-00-00-00-E1-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-44-69-63-74-69-6F-6E-61-72-79-60-32-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-2C-5B-53-79-73-74-65-6D-2E-49-6E-74-33-32-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-04-00-00-00-07-56-65-72-73-69-6F-6E-08-43-6F-6D-70-61-72-65-72-08-48-61-73-68-53-69-7A-65-0D-4B-65-79-56-61-6C-75-65-50-61-69-72-73-00-03-00-03-08-92-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-47-65-6E-65-72-69-63-45-71-75-61-6C-69-74-79-43-6F-6D-70-61-72-65-72-60-31-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-08-E5-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4B-65-79-56-61-6C-75-65-50-61-69-72-60-32-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-2C-5B-53-79-73-74-65-6D-2E-49-6E-74-33-32-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-5B-5D-03-00-00-00-09-0C-00-00-00-03-00-00-00-09-0D-00-00-00-07-0A-00-00-00-00-01-00-00-00-04-00-00-00-04-1D-4E-65-77-74-6F-6E-73-6F-66-74-2E-4A-73-6F-6E-2E-54-65-73-74-73-2E-41-64-64-72-65-73-73-02-00-00-00-09-0E-00-00-00-09-0F-00-00-00-0D-02-11-0B-00-00-00-04-00-00-00-0A-06-10-00-00-00-18-4D-61-72-6B-75-73-20-65-67-67-65-72-20-5D-3E-3C-5B-2C-20-28-32-6E-64-29-0D-02-04-0C-00-00-00-92-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-47-65-6E-65-72-69-63-45-71-75-61-6C-69-74-79-43-6F-6D-70-61-72-65-72-60-31-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-00-00-00-00-07-0D-00-00-00-00-01-00-00-00-03-00-00-00-03-E3-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4B-65-79-56-61-6C-75-65-50-61-69-72-60-32-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-2C-5B-53-79-73-74-65-6D-2E-49-6E-74-33-32-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-04-EF-FF-FF-FF-E3-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4B-65-79-56-61-6C-75-65-50-61-69-72-60-32-5B-5B-53-79-73-74-65-6D-2E-53-74-72-69-6E-67-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-2C-5B-53-79-73-74-65-6D-2E-49-6E-74-33-32-2C-20-6D-73-63-6F-72-6C-69-62-2C-20-56-65-72-73-69-6F-6E-3D-32-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-62-37-37-61-35-63-35-36-31-39-33-34-65-30-38-39-5D-5D-02-00-00-00-03-6B-65-79-05-76-61-6C-75-65-01-00-08-06-12-00-00-00-0A-56-61-6C-20-26-20-61-73-64-31-01-00-00-00-01-ED-FF-FF-FF-EF-FF-FF-FF-06-14-00-00-00-0B-56-61-6C-32-20-26-20-61-73-64-31-03-00-00-00-01-EB-FF-FF-FF-EF-FF-FF-FF-06-16-00-00-00-0B-56-61-6C-33-20-26-20-61-73-64-31-04-00-00-00-01-0E-00-00-00-04-00-00-00-06-17-00-00-00-0E-1F-61-72-72-61-79-3C-61-64-64-72-65-73-73-09-09-00-00-00-B6-FD-0B-45-F4-58-CC-88-01-0F-00-00-00-04-00-00-00-06-19-00-00-00-0F-61-72-72-61-79-20-32-20-61-64-64-72-65-73-73-09-09-00-00-00-B6-3D-A2-1A-2B-58-CC-88-0B";

        private const string XmlText =
            @"<TestClass xmlns=""http://schemas.datacontract.org/2004/07/Newtonsoft.Json.Tests"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Address1><Entered>2010-01-21T11:12:16.0809174+13:00</Entered><Phone>(503) 814-6335</Phone><Street>fff Street</Street></Address1><Addresses><Address><Entered>2009-12-31T11:12:16.0809174+13:00</Entered><Phone>(503) 814-6335</Phone><Street>&#x1F;array&lt;address</Street></Address><Address><Entered>2009-12-30T11:12:16.0809174+13:00</Entered><Phone>(503) 814-6335</Phone><Street>array 2 address</Street></Address></Addresses><BigNumber>34123123123.121</BigNumber><Name>Rick</Name><Now>2010-01-01T12:12:16.0809174+13:00</Now><dictionary xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><a:KeyValueOfstringint><a:Key>Val &amp; asd1</a:Key><a:Value>1</a:Value></a:KeyValueOfstringint><a:KeyValueOfstringint><a:Key>Val2 &amp; asd1</a:Key><a:Value>3</a:Value></a:KeyValueOfstringint><a:KeyValueOfstringint><a:Key>Val3 &amp; asd1</a:Key><a:Value>4</a:Value></a:KeyValueOfstringint></dictionary><strings xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><a:string i:nil=""true""/><a:string>Markus egger ]&gt;&lt;[, (2nd)</a:string><a:string i:nil=""true""/></strings></TestClass>";

        public const string JsonText =
            @"{""strings"":[null,""Markus egger ]><[, (2nd)"",null],""dictionary"":{""Val & asd1"":1,""Val2 & asd1"":3,""Val3 & asd1"":4},""Name"":""Rick"",""Now"":""\/Date(1262301136080+1300)\/"",""BigNumber"":34123123123.121,""Address1"":{""Street"":""fff Street"",""Phone"":""(503) 814-6335"",""Entered"":""\/Date(1264025536080+1300)\/""},""Addresses"":[{""Street"":""\u001farray<address"",""Phone"":""(503) 814-6335"",""Entered"":""\/Date(1262211136080+1300)\/""},{""Street"":""array 2 address"",""Phone"":""(503) 814-6335"",""Entered"":""\/Date(1262124736080+1300)\/""}]}";

        private const string JsonIsoText =
            @"{""strings"":[null,""Markus egger ]><[, (2nd)"",null],""dictionary"":{""Val & asd1"":1,""Val2 & asd1"":3,""Val3 & asd1"":4},""Name"":""Rick"",""Now"":""2012-02-25T19:55:50.6095676+13:00"",""BigNumber"":34123123123.121,""Address1"":{""Street"":""fff Street"",""Phone"":""(503) 814-6335"",""Entered"":""2012-02-24T18:55:50.6095676+13:00""},""Addresses"":[{""Street"":""\u001farray<address"",""Phone"":""(503) 814-6335"",""Entered"":""2012-02-24T18:55:50.6095676+13:00""},{""Street"":""array 2 address"",""Phone"":""(503) 814-6335"",""Entered"":""2012-02-24T18:55:50.6095676+13:00""}]}";

        private const string SimpleJsonText =
            @"{""Id"":2311,""Name"":""Simple-1"",""Address"":""Planet Earth"",""Scores"":[82,96,49,40,38,38,78,96,2,39]}";

        public enum SerializeMethod
        {
            JsonNet,
            JsonNetWithIsoConverter,
            JsonNetBinary,
            JsonNetLinq,
            JsonNetManual,
            BinaryFormatter,
            JavaScriptSerializer,
            DataContractSerializer,
            DataContractJsonSerializer
        }
        #endregion

        [Test]
        public void SerializeSimpleObject()
        {
            SimpleObject value = CreateSimpleObject();

            SerializeTests(value);
        }

        [Test]
        public void SerializeAnonymous()
        {
            var helloWorld = new { message = "Hello, World!" };

            BenchmarkSerializeMethod(SerializeMethod.JsonNet, helloWorld);
        }

        [Test]
        public void DeserializeSimpleObject()
        {
            DeserializeTests<SimpleObject>(SimpleJsonText);
        }

        [Test]
        public void Serialize()
        {
            TestClass test = CreateSerializationObject();

            SerializeTests(test);
        }

        [Test]
        public void ReadLargeJson()
        {
            for (int i = 0; i < 10; i++)
            {
                using (var fs = System.IO.File.OpenText("large.json"))
                using (JsonTextReader jsonTextReader = new JsonTextReader(fs))
                {
                    while (jsonTextReader.Read())
                    {
                    }
                }
            }
        }

        public class Friend
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class RootObject
        {
            public string _id { get; set; }
            public int index { get; set; }
            public Guid guid { get; set; }
            public bool isActive { get; set; }
            public string balance { get; set; }
            public Uri picture { get; set; }
            public int age { get; set; }
            public string eyeColor { get; set; }
            public string name { get; set; }
            public string gender { get; set; }
            public string company { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string address { get; set; }
            public string about { get; set; }
            public DateTime registered { get; set; }
            public double latitude { get; set; }
            public decimal longitude { get; set; }
            public List<string> tags { get; set; }
            public List<Friend> friends { get; set; }
            public string greeting { get; set; }
            public string favoriteFruit { get; set; }
        }

        [Test]
        public void DeserializeLargeJson()
        {
            var json = System.IO.File.ReadAllText("large.json");

            BenchmarkDeserializeMethod<IList<RootObject>>(SerializeMethod.JsonNet, json, Iterations / 10, false);
        }

        [Test]
        public void SerializeKeyValuePair()
        {
            IList<KeyValuePair<string, int>> value = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < 100; i++)
            {
                value.Add(new KeyValuePair<string, int>("Key" + i, i));
            }

            BenchmarkSerializeMethod(SerializeMethod.JsonNet, value);

            string json = JsonConvert.SerializeObject(value);

            BenchmarkDeserializeMethod<IList<KeyValuePair<string, int>>>(SerializeMethod.JsonNet, json);
        }

        private void SerializeTests(object value)
        {
            BenchmarkSerializeMethod(SerializeMethod.DataContractSerializer, value);
            BenchmarkSerializeMethod(SerializeMethod.BinaryFormatter, value);
            BenchmarkSerializeMethod(SerializeMethod.JavaScriptSerializer, value);
            BenchmarkSerializeMethod(SerializeMethod.DataContractJsonSerializer, value);
            BenchmarkSerializeMethod(SerializeMethod.JsonNet, value);
            BenchmarkSerializeMethod(SerializeMethod.JsonNetLinq, value);
            BenchmarkSerializeMethod(SerializeMethod.JsonNetManual, value);
            BenchmarkSerializeMethod(SerializeMethod.JsonNetWithIsoConverter, value);
            BenchmarkSerializeMethod(SerializeMethod.JsonNetBinary, value);
        }

        [Test]
        public void Deserialize()
        {
            BenchmarkDeserializeMethod<TestClass>(SerializeMethod.DataContractSerializer, XmlText);
            BenchmarkDeserializeMethod<TestClass>(SerializeMethod.BinaryFormatter, HexToBytes(BinaryFormatterHex));
            DeserializeTests<TestClass>(JsonText);
            BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JsonNetWithIsoConverter, JsonIsoText);
            BenchmarkDeserializeMethod<TestClass>(SerializeMethod.JsonNetBinary, HexToBytes(BsonHex));
        }

        public void DeserializeTests<T>(string json)
        {
            BenchmarkDeserializeMethod<T>(SerializeMethod.JavaScriptSerializer, json);
            BenchmarkDeserializeMethod<T>(SerializeMethod.DataContractJsonSerializer, json);
            BenchmarkDeserializeMethod<T>(SerializeMethod.JsonNet, json);
            BenchmarkDeserializeMethod<T>(SerializeMethod.JsonNetManual, json);
        }

        [Test]
        public void SerializeSizeNormal()
        {
            SerializeSize(CreateSerializationObject());
        }

        [Test]
        public void SerializeSizeData()
        {
            Image image = new Image();
            image.Data = System.IO.File.ReadAllBytes(@"bunny_pancake.jpg");
            image.FileName = "bunny_pancake.jpg";
            image.Author = "Hironori Akutagawa";
            image.Caption = "I have no idea what you are talking about so here's a bunny with a pancake on its head";

            SerializeSize(image);
        }

#if !(PORTABLE40)
#if !(PORTABLE)
        [Test]
        public void ConvertXmlNode()
        {
            XmlDocument doc = new XmlDocument();
            using (FileStream file = System.IO.File.OpenRead("large_sample.xml"))
            {
                doc.Load(file);
            }

            JsonConvert.SerializeXmlNode(doc);
        }
#endif

        [Test]
        public void ConvertXNode()
        {
            XDocument doc;
            using (FileStream file = System.IO.File.OpenRead("large_sample.xml"))
            {
                doc = XDocument.Load(file);
            }

            JsonConvert.SerializeXNode(doc);
        }
#endif

        private T TimeOperation<T>(Func<T> operation, string name)
        {
            // warm up
            operation();

            Stopwatch timed = new Stopwatch();
            timed.Start();

            T result = operation();

            Console.WriteLine(name);
            Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);

            timed.Stop();

            return result;
        }

        [Test]
        public void LargeArrayJTokenPathPerformance()
        {
            JArray a = new JArray();
            for (int i = 0; i < 100000; i++)
            {
                a.Add(i);
            }

            JToken first = a.First;
            JToken last = a.Last;

            int interations = 1000;

            TimeOperation(() =>
            {
                string p = null;
                for (int i = 0; i < interations; i++)
                {
                    p = first.Path;
                }

                return p;
            }, "First");

            TimeOperation(() =>
            {
                string p = null;
                for (int i = 0; i < interations; i++)
                {
                    p = last.Path;
                }

                return p;
            }, "Last");
        }

        [Test]
        public void LargeArrayAddPerformance()
        {
            JArray a1 = new JArray();

            JArray a2 = new JArray();
            for (int i = 0; i < 100000; i++)
            {
                a2.Add(i);
            }

            int interations = 1000;

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    a1.Add(interations);
                }

                return a1;
            }, "Small");

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    a2.Add(interations);
                }

                return a2;
            }, "Large");
        }

        [Test]
        public void BuildJObject()
        {
            JObject o = new JObject();
            for (int i = 0; i < 50; i++)
            {
                o[i.ToString()] = i;
            }
            string jsonText = o.ToString();

            // this is extremely slow with 5000 interations
            int interations = 1000;

            TimeOperation(() =>
            {
                JObject oo = null;
                for (int i = 0; i < interations; i++)
                {
                    oo = JObject.Parse(jsonText);
                }

                return oo;
            }, "JObject");
        }

        [Test]
        public void BuildJObjectComparedToXml()
        {
            const long totalIterations = 100000;

            const String xml =
                @"<?xml  version=""1.0"" encoding=""ISO-8859-1""?>
                <root>
                    <property name=""Property1"">1</property>
                    <property name=""Property2"">2</property>
                    <property name=""Property3"">3</property>
                    <property name=""Property4"">4</property>
                    <property name=""Property5"">5</property>
                </root>";

            const String json =
                @"{
                    ""Property1"":""1"",
                    ""Property2"":""2"",
                    ""Property3"":""3"",
                    ""Property4"":""4"",
                    ""Property5"":""5""
                }";

            var watch = new Stopwatch();
            watch.Start();
            for (long iteration = 0; iteration < totalIterations; ++iteration)
            {
                var obj = JObject.Parse(json);
                obj["Property1"].Value<Int32>();
                obj["Property2"].Value<Int32>();
                obj["Property3"].Value<Int32>();
                obj["Property4"].Value<Int32>();
                obj["Property5"].Value<Int32>();
            }
            watch.Stop();
            var performance1 = (totalIterations / watch.ElapsedMilliseconds) * 1000;
            Console.WriteLine("JSON: " + watch.Elapsed.TotalSeconds);

            watch.Reset();
            watch.Start();
            for (long iteration = 0; iteration < totalIterations; ++iteration)
            {
                var doc = XDocument.Parse(xml);
                var alarmProperties = doc.Descendants("property");
                foreach (var property in alarmProperties)
                {
                    var attr = property.Attribute("name");
                    var name = attr.Value;
                    switch (name)
                    {
                        case "Property1":
                            Int32.Parse(property.Value);
                            break;
                        case "Property2":
                            Int32.Parse(property.Value);
                            break;
                        case "Property3":
                            Int32.Parse(property.Value);
                            break;
                        case "Property4":
                            Int32.Parse(property.Value);
                            break;
                        case "Property5":
                            Int32.Parse(property.Value);
                            break;
                    }
                }
            }
            watch.Stop();
            var performance2 = (totalIterations / watch.ElapsedMilliseconds) * 1000;
            Console.WriteLine("XML: " + watch.Elapsed.TotalSeconds);
        }

        [Test]
        public void SerializeString()
        {
            string text = @"The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.
";

            int interations = 1000;

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    using (StringWriter w = StringUtils.CreateStringWriter(text.Length))
                    {
                        char[] buffer = null;
                        JavaScriptUtils.WriteEscapedJavaScriptString(w, text, '"', true, JavaScriptUtils.DoubleQuoteCharEscapeFlags, StringEscapeHandling.Default, null, ref buffer);
                    }
                }

                return "";
            }, "New");
        }

        [Test]
        public void JTokenToObject()
        {
            JValue s = new JValue("String!");

            int interations = 1000000;

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    s.ToObject(typeof(string));
                }

                return "";
            }, "New");

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    s.ToObject(typeof(string), new JsonSerializer());
                }

                return "";
            }, "Old");

            TimeOperation(() =>
            {
                for (int i = 0; i < interations; i++)
                {
                    s.Value<string>();
                }

                return "";
            }, "Value");
        }

        private void SerializeSize(object value)
        {
            // this is extremely slow with 5000 interations
            int interations = 100;

            byte[] jsonBytes = TimeOperation(() =>
            {
                string json = null;
                for (int i = 0; i < interations; i++)
                {
                    json = JsonConvert.SerializeObject(value, Formatting.None);
                }

                return Encoding.UTF8.GetBytes(json);
            }, "Json.NET");

            byte[] bsonBytes = TimeOperation(() =>
            {
                MemoryStream ms = null;
                for (int i = 0; i < interations; i++)
                {
                    ms = new MemoryStream();
                    JsonSerializer serializer = new JsonSerializer();
                    BsonWriter writer = new BsonWriter(ms);

                    serializer.Serialize(writer, value);
                    writer.Flush();
                }

                return ms.ToArray();
            }, "Json.NET BSON");

            byte[] xmlBytes = TimeOperation(() =>
            {
                MemoryStream ms = null;
                for (int i = 0; i < interations; i++)
                {
                    ms = new MemoryStream();
                    DataContractSerializer dataContractSerializer = new DataContractSerializer(value.GetType());
                    dataContractSerializer.WriteObject(ms, value);
                }

                return ms.ToArray();
            }, "DataContractSerializer");

            byte[] wcfJsonBytes = TimeOperation(() =>
            {
                MemoryStream ms = null;
                for (int i = 0; i < interations; i++)
                {
                    ms = new MemoryStream();
                    DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(value.GetType());
                    dataContractJsonSerializer.WriteObject(ms, value);
                }

                return ms.ToArray();
            }, "DataContractJsonSerializer");

            byte[] binaryFormatterBytes = TimeOperation(() =>
            {
                MemoryStream ms = null;
                for (int i = 0; i < interations; i++)
                {
                    ms = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, value);
                }

                return ms.ToArray();
            }, "BinaryFormatter");

            Console.WriteLine("Json.NET size: {0} bytes", jsonBytes.Length);
            Console.WriteLine("BSON size: {0} bytes", bsonBytes.Length);
            Console.WriteLine("WCF JSON size: {0} bytes", wcfJsonBytes.Length);
            Console.WriteLine("WCF XML size: {0} bytes", xmlBytes.Length);
            Console.WriteLine("BinaryFormatter size: {0} bytes", binaryFormatterBytes.Length);
        }

        #region Serialize
        private static readonly byte[] Buffer = new byte[4096];

        public void BenchmarkSerializeMethod(SerializeMethod method, object value)
        {
            Serialize(method, value);

            Stopwatch timed = new Stopwatch();
            timed.Start();

            string json = null;
            for (int x = 0; x < Iterations; x++)
            {
                json = Serialize(method, value);
            }

            timed.Stop();

            Console.WriteLine("Serialize method: {0}", method);
            Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);
            Console.WriteLine(json);
            Console.WriteLine();
        }

        private TestClass CreateSerializationObject()
        {
            TestClass test = new TestClass();

            test.dictionary = new Dictionary<string, int> { { "Val & asd1", 1 }, { "Val2 & asd1", 3 }, { "Val3 & asd1", 4 } };

            test.Address1.Street = "fff Street";
            test.Address1.Entered = DateTime.Now.AddDays(20);

            test.BigNumber = 34123123123.121M;
            test.Now = DateTime.Now.AddHours(1);
            test.strings = new List<string>() { null, "Markus egger ]><[, (2nd)", null };

            Address address = new Address();
            address.Entered = DateTime.Now.AddDays(-1);
            address.Street = "\u001farray\u003caddress";

            test.Addresses.Add(address);

            address = new Address();
            address.Entered = DateTime.Now.AddDays(-2);
            address.Street = "array 2 address";
            test.Addresses.Add(address);
            return test;
        }

        private static SimpleObject CreateSimpleObject()
        {
            return new SimpleObject
            {
                Name = "Simple-1",
                Id = 2311,
                Address = "Planet Earth",
                Scores = new[] { 82, 96, 49, 40, 38, 38, 78, 96, 2, 39 }
            };
        }

        public string SerializeWebExtensions(object value)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();

            return ser.Serialize(value);
        }

        public string SerializeDataContractJson(object value)
        {
            DataContractJsonSerializer dataContractSerializer
                = new DataContractJsonSerializer(value.GetType());

            MemoryStream ms = new MemoryStream();
            dataContractSerializer.WriteObject(ms, value);

            ms.Seek(0, SeekOrigin.Begin);

            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

        public string SerializeDataContract(object value)
        {
            DataContractSerializer dataContractSerializer
                = new DataContractSerializer(value.GetType());

            MemoryStream ms = new MemoryStream();
            dataContractSerializer.WriteObject(ms, value);

            ms.Seek(0, SeekOrigin.Begin);

            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

        private string Serialize(SerializeMethod method, object value)
        {
            string json;

            switch (method)
            {
                case SerializeMethod.JsonNet:
                    json = JsonConvert.SerializeObject(value);
                    break;
                case SerializeMethod.JsonNetWithIsoConverter:
                    json = JsonConvert.SerializeObject(value, new IsoDateTimeConverter());
                    break;
                case SerializeMethod.JsonNetLinq:
                {
                    TestClass c = value as TestClass;
                    if (c != null)
                    {
                        JObject o = new JObject(
                            new JProperty("strings", new JArray(
                                c.strings
                                )),
                            new JProperty("dictionary", new JObject(c.dictionary.Select(d => new JProperty(d.Key, d.Value)))),
                            new JProperty("Name", c.Name),
                            new JProperty("Now", c.Now),
                            new JProperty("BigNumber", c.BigNumber),
                            new JProperty("Address1", new JObject(
                                new JProperty("Street", c.Address1.Street),
                                new JProperty("Phone", c.Address1.Phone),
                                new JProperty("Entered", c.Address1.Entered))),
                            new JProperty("Addresses", new JArray(c.Addresses.Select(a =>
                                new JObject(
                                    new JProperty("Street", a.Street),
                                    new JProperty("Phone", a.Phone),
                                    new JProperty("Entered", a.Entered)))))
                            );

                        json = o.ToString(Formatting.None);
                    }
                    else
                    {
                        json = string.Empty;
                    }
                    break;
                }
                case SerializeMethod.JsonNetManual:
                {
                    TestClass c = value as TestClass;
                    if (c != null)
                    {
                        StringWriter sw = new StringWriter();
                        JsonTextWriter writer = new JsonTextWriter(sw);
                        writer.WriteStartObject();
                        writer.WritePropertyName("strings");
                        writer.WriteStartArray();
                        foreach (string s in c.strings)
                        {
                            writer.WriteValue(s);
                        }
                        writer.WriteEndArray();
                        writer.WritePropertyName("dictionary");
                        writer.WriteStartObject();
                        foreach (KeyValuePair<string, int> keyValuePair in c.dictionary)
                        {
                            writer.WritePropertyName(keyValuePair.Key);
                            writer.WriteValue(keyValuePair.Value);
                        }
                        writer.WriteEndObject();
                        writer.WritePropertyName("Name");
                        writer.WriteValue(c.Name);
                        writer.WritePropertyName("Now");
                        writer.WriteValue(c.Now);
                        writer.WritePropertyName("BigNumber");
                        writer.WriteValue(c.BigNumber);
                        writer.WritePropertyName("Address1");
                        writer.WriteStartObject();
                        writer.WritePropertyName("Street");
                        writer.WriteValue(c.BigNumber);
                        writer.WritePropertyName("Street");
                        writer.WriteValue(c.BigNumber);
                        writer.WritePropertyName("Street");
                        writer.WriteValue(c.BigNumber);
                        writer.WriteEndObject();
                        writer.WritePropertyName("Addresses");
                        writer.WriteStartArray();
                        foreach (Address address in c.Addresses)
                        {
                            writer.WriteStartObject();
                            writer.WritePropertyName("Street");
                            writer.WriteValue(address.Street);
                            writer.WritePropertyName("Phone");
                            writer.WriteValue(address.Phone);
                            writer.WritePropertyName("Entered");
                            writer.WriteValue(address.Entered);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                        writer.WriteEndObject();

                        writer.Flush();
                        json = sw.ToString();
                    }
                    else
                    {
                        json = string.Empty;
                    }
                    break;
                }
                case SerializeMethod.JsonNetBinary:
                {
                    MemoryStream ms = new MemoryStream(Buffer);
                    JsonSerializer serializer = new JsonSerializer();
                    BsonWriter writer = new BsonWriter(ms);
                    serializer.Serialize(writer, value);

                    //json = BitConverter.ToString(ms.ToArray(), 0, (int)ms.Position);
                    json = "Bytes = " + ms.Position;
                    break;
                }
                case SerializeMethod.JavaScriptSerializer:
                    json = SerializeWebExtensions(value);
                    break;
                case SerializeMethod.DataContractJsonSerializer:
                    json = SerializeDataContractJson(value);
                    break;
                case SerializeMethod.DataContractSerializer:
                    json = SerializeDataContract(value);
                    break;
                case SerializeMethod.BinaryFormatter:
                    json = SerializeBinaryFormatter(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method));
            }

            return json;
        }

        private string SerializeBinaryFormatter(object value)
        {
            string json;
            MemoryStream ms = new MemoryStream(Buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, value);

            json = "Bytes = " + ms.Position;
            //json = BitConverter.ToString(ms.ToArray(), 0, (int)ms.Position);
            return json;
        }
        #endregion

        #region Deserialize
        public void BenchmarkDeserializeMethod<T>(SerializeMethod method, object json, int? iterations = null, bool warmUp = true)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (warmUp)
            {
                Deserialize<T>(method, json);
            }

            Stopwatch timed = new Stopwatch();
            timed.Start();

            iterations = iterations ?? Iterations;

            T value = default(T);
            for (int x = 0; x < iterations.Value; x++)
            {
                value = Deserialize<T>(method, json);
            }

            timed.Stop();

            Console.WriteLine("Deserialize method: {0}", method);
            Console.WriteLine("{0} ms", timed.ElapsedMilliseconds);
            Console.WriteLine(value);
            Console.WriteLine();
        }

        public T DeserializeJsonNet<T>(string json, bool isoDateTimeConverter)
        {
            Type type = typeof(T);

            JsonSerializer serializer = new JsonSerializer();
            //serializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            //serializer.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            //serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            if (isoDateTimeConverter)
            {
                serializer.Converters.Add(new IsoDateTimeConverter());
            }

            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                //reader.ArrayPool = JsonArrayPool.Instance;

                var value = (T)serializer.Deserialize(reader, type);
                return value;
            }
        }

        public TestClass DeserializeJsonNetManual(string json)
        {
            TestClass c = new TestClass();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    switch (propertyName)
                    {
                        case "strings":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                c.strings.Add((string)reader.Value);
                            }
                            break;
                        case "dictionary":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                            {
                                string key = (string)reader.Value;
                                c.dictionary.Add(key, reader.ReadAsInt32().GetValueOrDefault());
                            }
                            break;
                        case "Name":
                            c.Name = reader.ReadAsString();
                            break;
                        case "Now":
                            c.Now = reader.ReadAsDateTime().GetValueOrDefault();
                            break;
                        case "BigNumber":
                            c.BigNumber = reader.ReadAsDecimal().GetValueOrDefault();
                            break;
                        case "Address1":
                            reader.Read();
                            c.Address1 = CreateAddress(reader);
                            break;
                        case "Addresses":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                var address = CreateAddress(reader);
                                c.Addresses.Add(address);
                            }
                            break;
                    }
                }
                else
                {
                    break;
                }
            }

            return c;
        }

        private static Address CreateAddress(JsonTextReader reader)
        {
            Address a = new Address();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch ((string)reader.Value)
                    {
                        case "Street":
                            a.Street = reader.ReadAsString();
                            break;
                        case "Phone":
                            a.Phone = reader.ReadAsString();
                            break;
                        case "Entered":
                            a.Entered = reader.ReadAsDateTime().GetValueOrDefault();
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
            return a;
        }

        public T DeserializeJsonNetBinary<T>(byte[] bson)
        {
            Type type = typeof(T);

            JsonSerializer serializer = new JsonSerializer();
            serializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            serializer.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            return (T)serializer.Deserialize(new BsonReader(new MemoryStream(bson)), type);
        }

        public T DeserializeWebExtensions<T>(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

            return ser.Deserialize<T>(json);
        }

        public T DeserializeDataContractJson<T>(string json)
        {
            DataContractJsonSerializer dataContractSerializer = new DataContractJsonSerializer(typeof(T));

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return (T)dataContractSerializer.ReadObject(ms);
        }

        private T Deserialize<T>(SerializeMethod method, object json)
        {
            switch (method)
            {
                case SerializeMethod.JsonNet:
                    return DeserializeJsonNet<T>((string)json, false);
                case SerializeMethod.JsonNetWithIsoConverter:
                    return DeserializeJsonNet<T>((string)json, true);
                case SerializeMethod.JsonNetManual:
                    if (typeof(T) == typeof(TestClass))
                    {
                        return (T)(object)DeserializeJsonNetManual((string)json);
                    }

                    return default(T);
                case SerializeMethod.JsonNetBinary:
                    return DeserializeJsonNetBinary<T>((byte[])json);
                case SerializeMethod.BinaryFormatter:
                    return DeserializeBinaryFormatter<T>((byte[])json);
                case SerializeMethod.JavaScriptSerializer:
                    return DeserializeWebExtensions<T>((string)json);
                case SerializeMethod.DataContractSerializer:
                    return DeserializeDataContract<T>((string)json);
                case SerializeMethod.DataContractJsonSerializer:
                    return DeserializeDataContractJson<T>((string)json);
                default:
                    throw new ArgumentOutOfRangeException(nameof(method));
            }
        }

        private T DeserializeDataContract<T>(string xml)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            return (T)serializer.ReadObject(ms);
        }

        private T DeserializeBinaryFormatter<T>(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(new MemoryStream(bytes));
        }
        #endregion

        [Test]
        public void SerializeLargeObject()
        {
            LargeRecursiveTestClass rootValue = null;
            LargeRecursiveTestClass parentValue = null;
            for (int i = 0; i < 20; i++)
            {
                LargeRecursiveTestClass currentValue = new LargeRecursiveTestClass()
                {
                    Integer = int.MaxValue,
                    Text = "The quick red fox jumped over the lazy dog."
                };

                if (rootValue == null)
                {
                    rootValue = currentValue;
                }
                if (parentValue != null)
                {
                    parentValue.Child = currentValue;
                }

                parentValue = currentValue;
            }

            BenchmarkSerializeMethod(SerializeMethod.JsonNetBinary, rootValue);
        }

        [Test]
        public void SerializeUnicodeChars()
        {
            string s = (new string('\0', 30));

            BenchmarkSerializeMethod(SerializeMethod.JsonNet, s);
        }

        [Test]
        public void ParseJObject()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                JObject o = JObject.Parse(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}");
            }
            timer.Stop();

            string linq = timer.Elapsed.TotalSeconds.ToString();
            Console.WriteLine(linq);
        }

        [Test]
        public void JObjectToString()
        {
            JObject test = JObject.Parse(JsonText);

            TimeOperation<object>(() =>
            {
                for (int i = 0; i < Iterations; i++)
                {
                    test["dummy"] = new JValue(i);
                    test.ToString(Formatting.None);
                }
                return null;
            }, "JObject.ToString");
        }

        [Test]
        public void JObjectToString2()
        {
            JObject test = JObject.Parse(JsonText);
            MemoryStream ms = new MemoryStream();

            TimeOperation<object>(() =>
            {
                for (int i = 0; i < Iterations; i++)
                {
                    test["dummy"] = new JValue(i);
                    ms.Seek(0, SeekOrigin.Begin);
                    JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(ms));
                    test.WriteTo(jsonTextWriter);
                    jsonTextWriter.Flush();
                    ms.ToArray();

                    //Encoding.UTF8.GetBytes(test.ToString(Formatting.None));
                }
                return null;
            }, "JObject.ToString");
        }

        [Test]
        public void JObjectCreationAndPropertyAccess()
        {
            TimeOperation<object>(() =>
            {
                for (int i = 0; i < Iterations * 100; i++)
                {
                    JObject test = new JObject(
                        new JProperty("one", 1),
                        new JProperty("two", 2));

                    test["i"] = i;
                    int j = (int)test["i"];
                    test["j"] = j;
                }
                return null;
            }, "JObjectCreationAndPropertyAccess");
        }

        [Test]
        public void NestedJToken()
        {
            Stopwatch sw;
            for (int i = 10000; i <= 100000; i += 10000)
            {
                sw = new Stopwatch();
                sw.Start();
                JArray ija = new JArray();
                JToken ijt = ija;
                for (int j = 0; j < i; j++)
                {
                    JArray temp = new JArray();
                    ija.Add(temp);
                    ija = temp;
                }
                ija.Add(1);
                sw.Stop();
                Console.WriteLine("Created a JToken of depth {0} (using OM) in {1} millis", i, sw.ElapsedMilliseconds);
            }
        }

        [Test]
        public void DeserializeNestedJToken()
        {
            string json = (new string('[', 100000)) + "1" + ((new string(']', 100000)));

            Stopwatch sw;
            sw = new Stopwatch();
            sw.Start();

            var a = (JArray)JsonConvert.DeserializeObject(json);

            sw.Stop();

            Assert.AreEqual(1, a.Count);

            Console.WriteLine("Deserialize big ass nested array in {0} millis", sw.ElapsedMilliseconds);
        }
    }

    public class LargeRecursiveTestClass
    {
        public LargeRecursiveTestClass Child { get; set; }
        public string Text { get; set; }
        public int Integer { get; set; }
    }

    #region Classes
    [Serializable]
    [DataContract]
    public class TestClass
    {
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Name = "Rick";

        [DataMember]
        public DateTime Now
        {
            get { return _Now; }
            set { _Now = value; }
        }

        private DateTime _Now = DateTime.Now;

        [DataMember]
        public decimal BigNumber
        {
            get { return _BigNumber; }
            set { _BigNumber = value; }
        }

        private decimal _BigNumber = 1212121.22M;

        [DataMember]
        public Address Address1
        {
            get { return _Address1; }
            set { _Address1 = value; }
        }

        private Address _Address1 = new Address();

        [DataMember]
        public List<Address> Addresses
        {
            get { return _Addresses; }
            set { _Addresses = value; }
        }

        private List<Address> _Addresses = new List<Address>();

        [DataMember]
        public List<string> strings = new List<string>();

        [DataMember]
        public Dictionary<string, int> dictionary = new Dictionary<string, int>();
    }

    [Serializable]
    [DataContract]
    public class Address
    {
        [DataMember]
        public string Street
        {
            get { return _street; }
            set { _street = value; }
        }

        private string _street = "32 Kaiea";

        [DataMember]
        public string Phone
        {
            get { return _Phone; }
            set { _Phone = value; }
        }

        private string _Phone = "(503) 814-6335";

        [DataMember]
        public DateTime Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }

        private DateTime _Entered = DateTime.Parse("01/01/2007", CultureInfo.CurrentCulture.DateTimeFormat);
    }

    [DataContract]
    [Serializable]
    public class SimpleObject
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public int[] Scores { get; set; }
    }
    #endregion
}

#endif