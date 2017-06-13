using System.Windows;
using System.Windows.Media;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Ui
{
    public class DScanItemViewModel : ViewModelBase
    {
        public IDScanItem Item { get; private set; }

        private Brush _backgroundColor = SystemColors.WindowBrush;
        private Visibility _visibility;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                OnPropertyChanged();
            }
        }

        public DScanItemViewModel(IDScanItem item)
        {
            Item = item;
        }

        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (Equals(value, _backgroundColor)) return;
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        

        public override void Dispose()
        {
        }
    }
}