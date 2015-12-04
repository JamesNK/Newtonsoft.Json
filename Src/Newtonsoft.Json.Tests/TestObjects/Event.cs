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

namespace Newtonsoft.Json.Tests.TestObjects
{
    /// <summary>
    /// What types of events are there? Just sticking to a basic set of four for now.
    /// </summary>
    /// <remarks></remarks>
    public enum EventType
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public sealed class Event
    {
        /// <summary>
        /// If no current user is specified, returns Nothing (0 from VB)
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private static int GetCurrentUserId()
        {
            return 0;
        }

        /// <summary>
        /// Gets either the application path or the current stack trace.
        /// NOTE: You MUST call this from the top level entry point. Otherwise,
        /// the stack trace will be buried in Logger itself.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private static string GetCurrentSubLocation()
        {
            return "";
        }

        private string _sublocation;
        private int _userId;
        private EventType _type;
        private string _summary;
        private string _details;
        private string _stackTrace;
        private string _tag;
        private DateTime _time;

        public Event(string summary)
        {
            _summary = summary;
            _time = DateTime.Now;

            if (_userId == 0)
            {
                _userId = GetCurrentUserId();
            }
            //This call only works at top level for now.
            //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
            if (_sublocation == null)
            {
                _sublocation = GetCurrentSubLocation();
            }
        }

        public Event(string sublocation, int userId, EventType type, string summary, string details, string stackTrace, string tag)
        {
            _sublocation = sublocation;
            _userId = userId;
            _type = type;
            _summary = summary;
            _details = details;
            _stackTrace = stackTrace;
            _tag = tag;
            _time = DateTime.Now;

            if (_userId == 0)
            {
                _userId = GetCurrentUserId();
            }
            //If _stackTrace = Nothing Then _stackTrace = Environment.StackTrace
            if (_sublocation == null)
            {
                _sublocation = GetCurrentSubLocation();
            }
        }

        public override string ToString()
        {
            return string.Format("{{ sublocation = {0}, userId = {1}, type = {2}, summary = {3}, details = {4}, stackTrace = {5}, tag = {6} }}", _sublocation, _userId, _type, _summary, _details, _stackTrace, _tag);
        }

        public string sublocation
        {
            get { return _sublocation; }
            set { _sublocation = value; }
        }

        public int userId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public EventType type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        public string details
        {
            get { return _details; }
            set { _details = value; }
        }

        public string stackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        public string tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public DateTime time
        {
            get { return _time; }
        }
    }
}