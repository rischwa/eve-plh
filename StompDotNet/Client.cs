using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using StompDotNet.Message.Client;
using StompDotNet.Message.Server;

namespace StompDotNet
{
    public delegate void MessageHandler(MessageMessage message);

    public delegate void Disconnected(Exception e);

    public delegate void ErrorHandler(ErrorMessage errorMessage);
    public class Client : IDisposable
    {
        private readonly TcpClient _client;

        private readonly Dictionary<string, List<Action<MessageMessage>>> _messageHandlers =
            new Dictionary<string, List<Action<MessageMessage>>>();

        private readonly NetworkStream _stream;
        private readonly Thread _thread;
        private int _id  = -1;
        private ProtocolVersion _protocolVersion;

        public event ErrorHandler ErrorMessageReceived;
        public event Disconnected Disconnected;

        protected virtual void OnDisconnected(Exception e)
        {
            Disconnected handler = Disconnected;
            if (handler != null) handler(e);
        }

        protected virtual void OnErrorMessageReceived(ErrorMessage errormessage)
        {
            ErrorHandler handler = ErrorMessageReceived;
            if (handler != null) handler(errormessage);
        }

        public Client(string host, int port, Authentication auth = null)
        {
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();

            Connect(host, auth);

            _thread = new Thread(Start);
            _thread.Start();
        }

        public void Dispose()
        {
            _thread.Abort();
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                if (_client.Connected)
                {
                    new Disconnect().WriteTo(_stream);
                }
            }
            finally
            {
                _client.Close();
            }
        }

        public void Subscribe(string topic, Action<MessageMessage> handler)
        {
            lock (_messageHandlers)
            {
                List<Action<MessageMessage>> handlers;
                if (!_messageHandlers.TryGetValue(topic, out handlers))
                {
                    handlers = new List<Action<MessageMessage>>();
                    _messageHandlers[topic] = handlers;
                    new SubscribeMessage(++_id, topic, _protocolVersion).WriteTo(_stream);
                }

                handlers.Add(handler);
            }
        }

        public void Send(string destination, string contentType, string content)
        {
            var msg = new SendMessage(destination, contentType, content);
            msg.WriteTo(_stream);
        }

        private void Start()
        {
            try
            {
                while (true)
                {
                    var frame = Frame.ReadFrom(_stream);
                    if (ErrorMessage.IsError(frame))
                    {
                        //TODO in andere threads verlagern?
                        OnErrorMessageReceived(new ErrorMessage(frame));
                        continue;
                    }
                    var message = new MessageMessage(frame);

                    List<Action<MessageMessage>> handlers;
                    lock (_messageHandlers)
                    {
                        if (!_messageHandlers.TryGetValue(message.Destination, out handlers))
                        {
                            return;
                        }
                        handlers = handlers.ToList();
                    }
                    foreach (var curHandler in handlers)
                    {
                        curHandler.Invoke(message);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                OnDisconnected(e);
            }
            Disconnect();
        }

        private void Connect(string host, Authentication auth)
        {
            var message = auth == null
                              ? new ConnectMessage(host)
                              : new ConnectMessage(host, auth.Username, auth.Password);

            message.WriteTo(_stream);

            var answerFrame = Frame.ReadFrom(_stream);
            ErrorMessage.ThrowIfIsError(answerFrame);

            var connected = new ConnectedMessage(answerFrame);
            _protocolVersion = connected.ProtocolVersion;
        }

        public class Authentication
        {
            public Authentication(string username, string password)
            {
                Username = username;
                Password = password;
            }

            public string Username { get; private set; }
            public string Password { get; private set; }
        }
    }
}