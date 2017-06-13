using System;

namespace StompDotNet
{
    public class StompProtocolException : Exception
    {
        public StompProtocolException(string msg) : base(msg)
        {
         
        }
    }
}
