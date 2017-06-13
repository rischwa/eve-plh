using System.IO;

namespace StompDotNet.Message.Client
{
    interface IClientMessage
    {
        void WriteTo(Stream stream);
    }
}