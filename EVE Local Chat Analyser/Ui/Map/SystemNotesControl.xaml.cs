using System.Windows;
using System.Windows.Controls;
using EveLocalChatAnalyser.Ui.Models;
using EveLocalChatAnalyser.Utilities;

namespace EveLocalChatAnalyser.Ui.Map
{
    public class SystemNote
    {
        public int Id { get; set; }

        public string Note { get; set; }
    }

    /// <summary>
    ///     Interaction logic for SystemNotesControl.xaml
    /// </summary>
    public partial class SystemNotesControl : UserControl
    {
        public SystemNotesControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var selectedModel = DataContext as SolarSystemViewModel;

            if (selectedModel == null)
            {
                TxtNotes.Text = "";
                TxtNotes.IsEnabled = false;
                BtnSave.IsEnabled = false;
                return;
            }

            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<SystemNote>(typeof (SystemNote).Name);
                var notes = collection.FindById(selectedModel.ID);
                TxtNotes.Text = notes != null ? notes.Note : "";
            }

            TxtNotes.IsEnabled = true;
            BtnSave.IsEnabled = false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var model = DataContext as SolarSystemViewModel;
            if (model == null)
            {
                return;
            }

            using (var session = App.CreateStorageEngine())
            {
                var collection = session.GetCollection<SystemNote>(typeof (SystemNote).Name);

                collection.Upsert(
                                  new SystemNote
                                  {
                                      Id = model.ID,
                                      Note = TxtNotes.Text
                                  });
            }
            BtnSave.IsEnabled = false;
        }

        private void TxtNotes_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSave.IsEnabled = true;
        }
    }
}
