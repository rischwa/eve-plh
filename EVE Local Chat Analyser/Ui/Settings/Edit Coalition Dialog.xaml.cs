using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EveLocalChatAnalyser.Utilities;
using Newtonsoft.Json;

namespace EveLocalChatAnalyser.Ui.Settings
{
    /// <summary>
    ///     Interaction logic for Edit_Coalition_Dialog.xaml
    /// </summary>
    public partial class EditCoalitionDialog : Window
    {
        private EditCoalitionDialog()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
                      //otherwise window content is pure white (controls are there)
                      //after update to windows 8.1 -> problem with .net 4.5.1???
                      //problem disappeared, maybe driver update ...
                      this.ActivateSoftwareRendering();
        }

        public static bool TryCreateCoalition(out Coalition coalition)
        {
            var dialog = new EditCoalitionDialog
                {
                    Title = "Add Coalition"
                };
            return TryGetResult(out coalition, dialog);
        }

        public static bool TryEditCoalition(Coalition editInput, out Coalition result)
        {
            var dialog = new EditCoalitionDialog
                {
                    TxtName = {Text = editInput.Name},
                    TxtMemberAlliances = {Text = string.Join(",\n", editInput.MemberAlliances)},
                    Title = "Edit Coalition"
                };

            return TryGetResult(out result, dialog);
        }

        private static bool TryGetResult(out Coalition coalition, EditCoalitionDialog dialog)
        {
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                coalition = new Coalition
                    {
                        Name = dialog.TxtName.Text,
                        MemberAlliances = dialog.TxtMemberAlliances.Text.Split(',', ':', ';').Select(x => x.Trim()).OrderBy(x=>x).ToList()
                    };

                return true;
            }

            coalition = null;
            return false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Coalition
    {
        [JsonProperty]
        public String Name { get; set; }
        [JsonProperty]
        public List<String> MemberAlliances { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string MemberAlliancesStr { get { return string.Join("\n", MemberAlliances); } }

        protected bool Equals(Coalition other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Coalition) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}