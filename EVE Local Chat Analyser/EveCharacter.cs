#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EveLocalChatAnalyser.Exceptions;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Ui.Settings;
using EveLocalChatAnalyser.Utilities;

#endregion

namespace EveLocalChatAnalyser
{
    public enum LocalChangeStatus
    {
        Stayed,

        Entered,

        Exited
    }

    public class CharacterPosition
    {
        public DateTime LastTimeSeen { get; set; }
        public string System { get; set; }
    }

    public interface IEveCharacter : INotifyPropertyChanged
    {
        IEnumerable<CharacterPosition> KnownPositions { get; }

        double SecurityStatus { get; }

        IEnumerable<Coalition> Coalitions { get; }
        
        Age Age { get; }

        [CanBeNull]
        string Alliance { get; }

        [NotNull]
        string Corporation { get; }

        [NotNull]
        string Id { get; }

        int FactionId { get; }

        string FactionName { get; }

        void SetFaction(int id, string name);

        bool IsCvaKos { get; set; }

        LocalChangeStatus LocalChangeStatus { get; set; }

        [NotNull]
        string Name { get; }

        KillboardInformation KillboardInformation { get; set; }

        void AddKnownPosition(CharacterPosition position);
    }

    /// <summary>
    ///     Collected information on an EVE Online character.
    ///     Only notifies on changes to IsCvaKos, the loading of IsCvaKos value happens deferred.
    /// </summary>
    public class EveCharacter : IEveCharacter
    {
        private readonly Age _age;
        private readonly double _securityStatus;
        private readonly string _alliance;

        private readonly string _corporation;

        private readonly String _id;
        private readonly LinkedList<CharacterPosition> _knownPositions = new LinkedList<CharacterPosition>();

        private readonly string _name;

        private bool _isCvaKos;
        private KillboardInformation _killboardInformation;
        private LocalChangeStatus _localChangeStatus;
        private int _factionId;
        private string _factionName;
        private List<Coalition> _coalitions;

        public EveCharacter()
        {
        }

        public EveCharacter([NotNull] string id, [NotNull] string name, double securityStatus, [NotNull] Age age, [NotNull] string corporation, [CanBeNull] string alliance, IEnumerable<CharacterPosition> knownPositions)
        {
            _id = id;
            _name = name;
            _securityStatus = securityStatus;
            _age = age;
            _corporation = corporation;
            _alliance = alliance;
            _knownPositions = new LinkedList<CharacterPosition>(knownPositions);
            InitCoalitions();
            Settings.Default.PropertyChanged += SettingsPropertyChanged;
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.CoalitionsJson))
            {
                InitCoalitions();
            }
        }

        private void InitCoalitions()
        {
            _coalitions = Settings.Default.MergedCoalitions.Where(c=>c.MemberAlliances.Contains(_alliance)).ToList();
            OnPropertyChanged("Coalitions");
        }

        public IEnumerable<CharacterPosition> KnownPositions
        {
            get { return _knownPositions; }
        }

        public double SecurityStatus { get { return _securityStatus; } }
        public IEnumerable<Coalition> Coalitions { get { return _coalitions; } }

        public void AddKnownPosition(CharacterPosition position)
        {
            _knownPositions.AddFirst(position);
            OnPropertyChanged("KnownPositions");
        }

        public Age Age
        {
            get { return _age; }
        }

        [CanBeNull]
        public string Alliance
        {
            get { return _alliance; }
        }

        [NotNull]
        public string Corporation
        {
            get { return _corporation; }
        }

        [NotNull]
        public string Id
        {
            get { return _id; }
        }

        public int FactionId
        {
            get { return _factionId; }
        }

        public string FactionName
        {
            get { return _factionName; }
        }

        public void SetFaction(int id, string name)
        {
            _factionId = id;
            _factionName = name;
            OnPropertyChanged("FactionId");
        }

        public bool IsCvaKos
        {
            get { return _isCvaKos; }
            set
            {
                if (_isCvaKos == value)
                {
                    return;
                }
                _isCvaKos = value;
                OnPropertyChanged("IsCvaKos");
            }
        }

        public LocalChangeStatus LocalChangeStatus

        {
            get { return _localChangeStatus; }
            set
            {
                if (value == _localChangeStatus) return;
                _localChangeStatus = value;
                OnPropertyChanged("LocalChangeStatus");
            }
        }

        [NotNull]
        public string Name
        {
            get { return _name; }
        }

        public KillboardInformation KillboardInformation
        {
            get { return _killboardInformation; }
            set
            {
                _killboardInformation = value;
                OnPropertyChanged("KillboardInformation");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == GetType() && Equals((IEveCharacter) obj);
        }

        public override int GetHashCode()
        {
            return (Name.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Name, Corporation, Alliance);
        }

        protected bool Equals(IEveCharacter other)
        {
            return string.Equals(Name, other.Name);
        }

        [NotifyPropertyChangedInvocator]
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() => handler(this, new PropertyChangedEventArgs(propertyName))));
            }
        }
    }

    public class ArchivedEveCharacter : IEveCharacter
    {
        private readonly IEveCharacter _eveCharacter;
        private readonly LocalChangeStatus _status;

        private static readonly string LOCAL_CHANGE_STATUS_PROPERTY =
            NotifyUtils.GetPropertyName((IEveCharacter eveChar) => eveChar.LocalChangeStatus);
        
        public ArchivedEveCharacter(IEveCharacter eveCharacter, LocalChangeStatus status)
        {
            _eveCharacter = eveCharacter;
            _status = status;
            _eveCharacter.PropertyChanged += EveCharacterOnPropertyChanged;
        }

        public string FactionName
        {
            get { return _eveCharacter.FactionName; }
        }

        public int FactionId
        {
            get { return _eveCharacter.FactionId; }
        }

        private void EveCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == LOCAL_CHANGE_STATUS_PROPERTY)
            {
                return;
            }

            OnPropertyChanged(propertyChangedEventArgs.PropertyName);
        }

        public IEnumerable<CharacterPosition> KnownPositions
        {
            get { return _eveCharacter.KnownPositions; }
        }

        public double SecurityStatus { get { return _eveCharacter.SecurityStatus; } }
        public IEnumerable<Coalition> Coalitions { get { return _eveCharacter.Coalitions; } }

        public void AddKnownPosition(CharacterPosition position)
        {
            _eveCharacter.AddKnownPosition(position);
        }

        public Age Age
        {
            get { return _eveCharacter.Age; }
        }

        public string Alliance
        {
            get { return _eveCharacter.Alliance; }
        }

        public string Corporation
        {
            get { return _eveCharacter.Corporation; }
        }

        public string Id
        {
            get { return _eveCharacter.Id; }
        }

        public void SetFaction(int id, string name)
        {
            throw new EveLocalChatAnalyserException("can't set faction for archived characters");
        }

        public bool IsCvaKos
        {
            get { return _eveCharacter.IsCvaKos; }
            set { _eveCharacter.IsCvaKos = value; }
        }

        public LocalChangeStatus LocalChangeStatus
        {
            get { return _status; }
            set { throw new EveLocalChatAnalyserException("can't change status of archived eve character"); }
        }

        public string Name
        {
            get { return _eveCharacter.Name; }
        }

        public KillboardInformation KillboardInformation
        {
            get { return _eveCharacter.KillboardInformation; }
            set { _eveCharacter.KillboardInformation = value;}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}