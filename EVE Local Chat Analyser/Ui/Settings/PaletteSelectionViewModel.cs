using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using EveLocalChatAnalyser.Annotations;
using EveLocalChatAnalyser.Ui.Map.Statistics;
using EveLocalChatAnalyser.Ui.Map.Statistics.Palettes;

namespace EveLocalChatAnalyser.Ui.Settings
{

    public class PaletteViewModel
    {
        public bool Equals(PaletteViewModel other)
        {
            return string.Equals(Name, other.Name);
        }

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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((PaletteViewModel) obj);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public string Name { get; set; }
        public IPalette Palette { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
    public sealed class PaletteSelectionViewModel : INotifyPropertyChanged
    {
        private readonly string _propertyName;
        private PaletteViewModel _selectedValue;

        public PaletteSelectionViewModel(string propertyName)
        {
            _propertyName = propertyName;
            SelectedValue = GetInitialSelectedValue();
        }

        private PaletteViewModel GetInitialSelectedValue()
        {
            var settingsValue = (string)Properties.Settings.Default[_propertyName];
            return PaletteCollection.Values.FirstOrDefault(x => x.Name == settingsValue) ?? PaletteCollection.Values.First();
        }


        public PaletteViewModel SelectedValue

        {
            get { return _selectedValue; }
            set
            {
                if (Equals(value, _selectedValue))
                {
                    return;
                }
                _selectedValue = value;
                Properties.Settings.Default[_propertyName] = _selectedValue.Name;


                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
