using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class ResponseWithNewGenericProperty<T> : SimpleResponse
    {
        public new T Data { get; set; }
    }

    public abstract class SimpleResponse
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        protected SimpleResponse()
        {

        }

        protected SimpleResponse(string message)
        {
            Message = message;
        }
    }
}
