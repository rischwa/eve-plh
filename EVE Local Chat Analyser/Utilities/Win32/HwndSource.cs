using System.Collections.Generic;
using System.Windows.Forms;

namespace EveLocalChatAnalyser.Utilities.Win32
{
    public sealed class HwndSource : NativeWindow, IHwndSource
    {
        private readonly Dictionary<int, MessageReceived> _messageDelegatesByMessageType =
            new Dictionary<int, MessageReceived>();

        public HwndSource()
        {
            CreateHandle(new CreateParams());
        }

        public MessageReceived this[int msgType]
        {
            get
            {
                MessageReceived myDelegate;
                return _messageDelegatesByMessageType.TryGetValue(msgType, out myDelegate) ? myDelegate : null;
            }

            set
            {
                if (_messageDelegatesByMessageType.ContainsKey(msgType))
                {
                    _messageDelegatesByMessageType[msgType] += value;
                    return;
                }
                _messageDelegatesByMessageType[msgType] = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            var messageDelegate = this[m.Msg];
            if (messageDelegate != null)
            {
                messageDelegate(m);
            }
        }



        ~HwndSource()
        {
            DestroyHandle();
        }
    }
}