using System;

namespace EveLocalChatAnalyser.Ui.Map.Statistics
{
    public class SettingsViewModel
    {
        private readonly string _propertyName;

        private TypedCalculatorViewModel _selectedCalculator;

        public SettingsViewModel(string propertyName, Func<string, TypedCalculatorViewModel> converter)
        {
            _propertyName = propertyName;
            _selectedCalculator = converter((string)Properties.Settings.Default[_propertyName]);
        }
        

            

        public TypedCalculatorViewModel SelectedValue
        {
            get { return _selectedCalculator; }
            set
            {
                if (value == _selectedCalculator)
                {
                    return;
                }
                _selectedCalculator = value;
                Properties.Settings.Default[_propertyName] = value.Name;
            }
        }
    }
}