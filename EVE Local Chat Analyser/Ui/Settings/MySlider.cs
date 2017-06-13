using System.Reflection;
using System.Windows.Controls;

namespace EveLocalChatAnalyser.Ui.Settings
{
    public class MySlider : Slider
    {
        private ToolTip _autoToolTip;

        public ToolTip AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    var field = typeof (Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
// ReSharper disable PossibleNullReferenceException
                    _autoToolTip = (ToolTip) field.GetValue(this);
// ReSharper restore PossibleNullReferenceException
                }
                return _autoToolTip;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if (AutoToolTip != null)
            {
                AutoToolTip.FontSize = newValue;
            }
        }
    }
}