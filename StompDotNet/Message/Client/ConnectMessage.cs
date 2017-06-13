using System;
using System.IO;

namespace StompDotNet.Message.Client
{
    public class ConnectMessage : IClientMessage
    {
        private readonly Frame _frame;
        //TODO an versionen anpassen
        public ConnectMessage(string host, string login = null, string passcode = null)
        {
            _frame = new Frame {Command = "CONNECT"};

            _frame.Headers["accept-version"] = "1.0,1.1,1.2";
            _frame.Headers["host"] = "/";
            _frame.Headers["login"] = login ?? "";
            _frame.Headers["passcode"] = passcode ?? "";
        }

        public void WriteTo(Stream stream)
        {
            _frame.WriteTo(stream);
            stream.Flush();
        }

        public void Dump()
        {
            _frame.WriteTo(Console.OpenStandardOutput());
            Console.Out.Flush();
        }
    }
}