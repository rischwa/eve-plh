using System;
using System.Runtime.Serialization;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Exceptions
{
    [Serializable]
    public class EveLocalChatAnalyserException : Exception
    {
        public EveLocalChatAnalyserException()
        {
        }

        public EveLocalChatAnalyserException(string message) : base(message)
        {
        }

        public EveLocalChatAnalyserException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EveLocalChatAnalyserException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}