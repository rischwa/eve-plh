using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser
{
    //TODO als service machen
    public class LocalChatHistory : ViewModelBase
    {
        public static readonly string HAS_NEXT_PROPERTY = NotifyUtils.GetPropertyName((LocalChatHistory h) => h.HasNext);

        public static readonly string HAS_PREVIOUS_PROPERTY =
            NotifyUtils.GetPropertyName((LocalChatHistory h) => h.HasPrevious);

        private readonly LinkedList<Entry> _history = new LinkedList<Entry>();
        private LinkedListNode<Entry> _node;
        private Entry _current;
        private Visibility _visibility;

        public LocalChatHistory()
        {
            Visibility = Visibility.Collapsed;
        }

        public ICollection<Entry> List { get { return _history; } }

        public bool HasNext
        {
            get { return (_node != null && _node.Next != null) || (_node == null && _history.Any()); }
        }

        public bool HasPrevious
        {
            get { return _node != null && _node.Previous != null; }
        }
        
        public void Reset()
        {
            var status = new Status(this);
            _node = null;
            Current = null;
            Visibility = Visibility.Collapsed;
            status.CheckChanges();
        }

        public Entry Current
        {
            get { return _current; }
            set
            {
                if (Equals(value, _current)) return;
                _current = value;
                OnPropertyChanged();
            }
        }

        public Entry Previous
        {
            get
            {
                if (_node == null || _node.Previous == null)
                {
                    throw new ElementNotAvailableException();
                }

                var status = new Status(this);

                Current = (_node = _node.Previous).Value;

                status.CheckChanges();
                return Current;
            }
        }

        public Entry Next
        {
            get
            {
                var status = new Status(this);
                if (_node == null)
                {
                    if (!_history.Any())
                    {
                        throw new ElementNotAvailableException();
                    }
                    _node = _history.First;
                }
                else
                {
                    if (_node.Next == null)
                    {
                        throw new ElementNotAvailableException();
                    }
                    _node = _node.Next;
                }
                
                Current = _node.Value;
                status.CheckChanges();
                return Current;
            }
        }

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

        public void AddEntry(IEnumerable<IEveCharacter> characters, string system)
        {
            var eveCharacters = characters.ToList();
            if (!eveCharacters.Any())//Clear
            {
                return;
            }
            var chars = eveCharacters.Select(character => new ArchivedEveCharacter(character, character.LocalChangeStatus));
            var entry = new Entry(system, DateTime.UtcNow, chars.Cast<IEveCharacter>().ToList());
            var status = new Status(this);

            _history.AddFirst(entry);
            
            _node = _history.First;
            
            Current = _node.Value;
            status.CheckChanges();
        }

        public override void Dispose()
        {
        }

        public class Entry
        {
            public Entry(string system, DateTime timeStamp, IList<IEveCharacter> characters)
            {
                Characters = characters;
                TimeStamp = timeStamp;
                System = system;
            }

            public string System { get; private set; }
            public DateTime TimeStamp { get; private set; }
            public IList<IEveCharacter> Characters { get; private set; }
        }

        private class Status
        {
            private readonly bool _hasNext;
            private readonly bool _hasPrevious;
            private readonly LocalChatHistory _history;
            private readonly Entry _current;
            public Status(LocalChatHistory history)
            {
                _current = history.Current;
                _history = history;
                _hasNext = history.HasNext;
                _hasPrevious = history.HasPrevious;
            }

            public void CheckChanges()
            {
                if (_current != _history.Current)
                {
                    _history.Visibility = _history.Current == null || _history.Current == _history._history.First.Value
                                              ? Visibility.Collapsed
                                              : Visibility.Visible;
                }
                if (_hasNext != _history.HasNext)
                {
                    _history.OnPropertyChanged(HAS_NEXT_PROPERTY);
                }
                if (_hasPrevious != _history.HasPrevious)
                {
                    _history.OnPropertyChanged(HAS_PREVIOUS_PROPERTY);
                }
            }
        }
    }
}