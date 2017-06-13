using System.Globalization;
using System.IO;
using System.Text;

namespace StompDotNet.Message.Client
{
    public class SendMessage : IClientMessage
    {
        private readonly Frame _frame;

        public SendMessage(string destination, string contentType, string body)
        {
            _frame = new Frame
                     {
                         Command = "SEND",
                         Headers = {
                                       {"destination", destination},
                                       {"content-type", contentType},
                                       {"content-length", Encoding.UTF8.GetByteCount(body)
                                           .ToString(CultureInfo.InvariantCulture)}
                                   },
                                   Body = body

                     };
        }

        public void WriteTo(Stream stream)
        {
            _frame.WriteTo(stream);
            stream.Flush();
        }
    }
}
