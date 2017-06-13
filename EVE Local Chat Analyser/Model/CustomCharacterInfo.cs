using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Utilities;
using LiteDB;

namespace EveLocalChatAnalyser.Model
{
    public class CustomCharacterInfo
    {
        public CustomCharacterInfo()
        {
            Tags = new List<string>();
        }

        [BsonId]
        public long Id { get; set; }

        public string IconImage { get; set; }

        public List<string> Tags { get; set; } 
    }


    public class CustomCharacterInfoViewModel : INotifyPropertyChanged
    {
        private CustomCharacterInfo _info;

        public CustomCharacterInfoViewModel(CustomCharacterInfo info)
        {
            _info = info;
        }

        public ReadOnlyCollection<string> Tags
        {
            get { return _info.Tags.AsReadOnly(); }
            set
            {
                _info.Tags = value.ToList();
                UpsertData();
                OnPropertyChanged();
            }
        }

        private void UpsertData()
        {
            var repository  = DIContainer.GetInstance<ICustomCharacterInfoRepository>();
            repository.UpsertCustomCharacterInfo(_info);
        }

        public string IconImage
        {
            get { return _info.IconImage; }
            set
            {
                _info.IconImage = value;
                UpsertData();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
