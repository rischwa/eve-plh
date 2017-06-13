using System;
using System.Runtime.Serialization;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Exceptions
{
    [Serializable]
    public class CharacterRetrievalException : EveLocalChatAnalyserException
    {
        public CharacterRetrievalException()
        {
        }

        public CharacterRetrievalException(string message) : base(message)
        {
        }

        public CharacterRetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CharacterRetrievalException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}