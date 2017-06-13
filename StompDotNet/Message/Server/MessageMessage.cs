using System;

namespace StompDotNet.Message.Server
{
    //TODO optionalmessage headers
    public class MessageMessage
    {
        private readonly string _destination;
        private readonly Frame _frame;
        private readonly int _messageId;
        private readonly int _subscribtion;

        public MessageMessage(Frame frame)
        {
            if (frame.Command != "MESSAGE")
            {
                throw new StompProtocolException(string.Format("Expected 'MESSAGE' command, but got '{0}'",
                                                               frame.Command));
            }
            if (!frame.Headers.TryGetValue("destination", out _destination))
            {
                throw new StompProtocolException("Missing 'destination' header in MESSAGE server message");
            }

            //TODO nicht in vers. 1.0
            //if (!TryReadIntHeader(frame, "subscription", out _subscribtion))
            //{
            //    throw new StompProtocolException("Missing or invalid  'subscription' header in MESSAGE server message");
            //}

            //if (!TryReadIntHeader(frame, "message-id", out _messageId))
            //{
            //    throw new StompProtocolException("Missing or invalid 'message-id' header in MESSAGE server message");
            //}

            _frame = frame;
        }

        public string Destination
        {
            get { return _destination; }
        }

        public int MessageId
        {
            get { return _messageId; }
        }

        public int Subscription
        {
            get { return _subscribtion; }
        }

        public string Body
        {
            get
            {
                return _frame.Body;
            }
        }

        public void Dump()
        {
            _frame.WriteTo(Console.OpenStandardOutput());
        }
    }
}