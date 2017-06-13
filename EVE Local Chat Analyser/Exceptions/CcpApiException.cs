using System;
using System.Runtime.Serialization;
using EveLocalChatAnalyser.Properties;

namespace EveLocalChatAnalyser.Exceptions
{
    [Serializable]
    internal class CcpApiException : EveLocalChatAnalyserException
    {
        private readonly int _errorCode;

        public CcpApiException()
        {
        }

        public CcpApiException(int errorCode, string message) : base(message)
        {
            _errorCode = errorCode;
        }

        public CcpApiException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            _errorCode = errorCode;
        }

        protected CcpApiException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public int ErrorCode
        {
            get { return _errorCode; }
        }

        public bool HasErrorCode
        {
            get { return _errorCode != -1; }
        }
    }
}