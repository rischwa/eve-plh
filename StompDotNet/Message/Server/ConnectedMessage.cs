using System.IO;

namespace StompDotNet.Message.Server
{
    public class ConnectedMessage
    {
        private readonly ProtocolVersion _protocolVersion;

        private ConnectedMessage(ProtocolVersion protocolVersion)
        {
            _protocolVersion = protocolVersion;
        }
        public ConnectedMessage(Frame frame)
        {
            if (frame.Command != "CONNECTED")
            {
                throw new StompProtocolException("Expected CONNECTED command, but got " + frame.Command);
            }

            string protocolVersionString;
            frame.Headers.TryGetValue("version", out protocolVersionString);
            //TODO check version und assign und so
            _protocolVersion = string.IsNullOrEmpty(protocolVersionString) ? ProtocolVersion.V_1_0 : ParseVersion(protocolVersionString); 
        }
        public static ConnectedMessage ReadFrom(Stream stream)
        {
            //TODO error frames als exceptions behandeln grundsaetzlich?
            var frame = Frame.ReadFrom(stream);
            return new ConnectedMessage(frame);
            //TODO optionale felder

        }

        private static ProtocolVersion ParseVersion(string protocolVersionString)
        {
            if (protocolVersionString == "1.0")
            {
                return ProtocolVersion.V_1_0;
            }

            if (protocolVersionString == "1.1")
            {
                return ProtocolVersion.V_1_1;
            }

            if (protocolVersionString == "1.2")
            {
                return ProtocolVersion.V_1_2;
            }

            throw new StompProtocolException("Unknown protocol version in CONNECTED command: " + protocolVersionString);
        }

        public ProtocolVersion ProtocolVersion
        {
            get { return _protocolVersion; }
        }
    }
}