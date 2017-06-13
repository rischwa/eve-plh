using System.Collections.Generic;

namespace StompDotNet.Message.Server
{
    public class ErrorMessage
    {
        private readonly Frame _frame;

        public static bool IsError(Frame frame)
        {
            return frame.Command == "ERROR";
        }

        public static void ThrowIfIsError(Frame frame)
        {
            if (IsError(frame))
            {
                new ErrorMessage(frame).Throw(); ;
            }
        }

        public ErrorMessage(Frame frame)
        {
            _frame = frame;
        }

        public Dictionary<string, string> Headers { get { return _frame.Headers; } }

        public string HeaderMessage
        {
            get
            {
                string tmp;
                return _frame.Headers.TryGetValue("message", out tmp) ? tmp : "";
            }
        }

        public string BodyMessage { get { return _frame.Body; } }

        public void Throw()
        {
            throw new StompErrorException(_frame);
        }
    }
}
