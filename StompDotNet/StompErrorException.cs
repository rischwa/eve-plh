using System;
using System.Collections.Generic;

namespace StompDotNet
{
    public class StompErrorException : ApplicationException
    {
        private readonly Frame _frame;

        internal StompErrorException(Frame frame)
        {
            _frame = frame;
        }

        public Dictionary<string, string> Headers { get { return _frame.Headers; } }

        public override string Message
        {
            get { return HeaderMessage; }
        }

        public string HeaderMessage { get{
            string tmp;
            return _frame.Headers.TryGetValue("message", out tmp) ? tmp : "";}
        }

        public string BodyMessage { get { return _frame.Body; } }
    }
}