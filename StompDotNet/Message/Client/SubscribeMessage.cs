using System.Globalization;
using System.IO;

namespace StompDotNet.Message.Client
{
    class SubscribeMessage : IClientMessage
    {
        private readonly Frame _frame;

        public SubscribeMessage(int id, string topic, ProtocolVersion version)
        {
            _frame = new Frame {Command = "SUBSCRIBE"};
            if (version == ProtocolVersion.V_1_2)
            {
                _frame.Headers["id"] = id.ToString(CultureInfo.InvariantCulture);
            }
            
            _frame.Headers["destination"] = topic;
            _frame.Headers["prefetch-count"] = "1";
            _frame.Headers["persistent"] = "true";
            //TODO ack header
        }

        public void WriteTo(Stream stream)
        {
            _frame.WriteTo(stream);
        }
    }
}
