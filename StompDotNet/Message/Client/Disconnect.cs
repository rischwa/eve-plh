using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompDotNet.Message.Client
{
    class Disconnect : IClientMessage
    {
        private static readonly Frame FRAME = new Frame(){Command = "DISCONNECT"};
        public void WriteTo(Stream stream)
        {
            FRAME.WriteTo(stream);
        }
    }
}
