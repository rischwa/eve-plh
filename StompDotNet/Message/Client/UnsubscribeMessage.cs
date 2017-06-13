using System.Globalization;
using System.IO;

namespace StompDotNet.Message.Client
{
    class UnsubscribeMessage : IClientMessage
    {
        private readonly Frame _frame;

        public UnsubscribeMessage(int id)
        {
            _frame = new Frame {Command = "UNSUBSCRIBE"};
            _frame.Headers["id"] = id.ToString(CultureInfo.InvariantCulture);
        }

        public void WriteTo(Stream stream)
        {
            _frame.WriteTo(stream);
            stream.Flush();
        }
    }
}
