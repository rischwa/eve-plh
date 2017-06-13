using System;
using System.ComponentModel;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows;
using EveLocalChatAnalyser.Properties;
using EveLocalChatAnalyser.Ui;
using EveLocalChatAnalyser.Utilities.QuickAction;

namespace EveLocalChatAnalyser.Utilities.VoiceCommands
{
    public interface IVoiceCommands
    {
    }

    public class VoiceCommands : IVoiceCommands
    {
        private const string TEST = "test";
        private const string QUICK_ACTION = "quickAction";
        private const string TOGGLE_MINIMIZE = "toggleMinimize";
        private readonly IQuickAction _quickAction;
        private SpeechRecognitionEngine _engine;

        public VoiceCommands(IQuickAction quickAction)
        {
            _quickAction = quickAction;
            Settings.Default.PropertyChanged += SettingsOnPropertyChanged;
            Setup();
        }

        public Exception Exception { get; set; }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.AreVoiceCommandsEnabled)
                || propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.VoiceCommandToggleMinimze)
                || propertyChangedEventArgs.PropertyName == NotifyUtils.GetPropertyName((Settings s) => s.VoiceCommandQuickAction))
            {
                Setup();
            }
        }

        private void Setup()
        {
            if (Settings.Default.AreVoiceCommandsEnabled)
            {
                SetupEngine();
            }
            else
            {
                ClearEngine();
            }
        }

        private void SetupEngine()
        {
            Exception = null;
            try
            {
                ClearEngine();

                _engine = new SpeechRecognitionEngine();
                _engine.SetInputToDefaultAudioDevice();

                RegisterPhrases();

                _engine.SpeechRecognized += EngineOnSpeechRecognized;
                _engine.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception e)
            {
                ClearEngine();
                Exception = e;
            }
        }

        private void EngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            if (speechRecognizedEventArgs == null || speechRecognizedEventArgs.Result == null)
            {
                return;
            }
            var grammar = speechRecognizedEventArgs.Result.Grammar;
            if (grammar == null || speechRecognizedEventArgs.Result.Confidence < 0.8)
            {
                return;
            }

            switch (grammar.Name)
            {
                case TEST:
                    RunTestCommand();
                    break;
                case QUICK_ACTION:
                    _quickAction.Run();
                    break;
                case TOGGLE_MINIMIZE:
                    ((App) Application.Current).OnToggleMinimize("");
                    break;
                default:
                    break;
            }
        }

        private void RunTestCommand()
        {
            try
            {
                var synthesizer = new SpeechSynthesizer();
                synthesizer.Speak("Enjoy Pirate's Little Helper with voice recognition. Yar!");
                ShowTestDialog();

                synthesizer.Dispose();
            }
            catch (Exception e)
            {
                ShowTestDialog();
            }
        }

        private static void ShowTestDialog()
        {
            var dialog = new TextBoxDialog() {Width = 300, Height = 100};
            dialog.TextBox.Text = "Enjoy  Pirate's Little Helper with voice recognition!";


            dialog.ShowDialog();

            dialog.Close();
        }

        private void RegisterPhrases()
        {
            _engine.LoadGrammar(
                                new Grammar(new GrammarBuilder("Test voice recognition one two"))
                                {
                                    Name = TEST
                                });
            RegisterGrammar(QUICK_ACTION, Settings.Default.VoiceCommandQuickAction);
            RegisterGrammar(TOGGLE_MINIMIZE, Settings.Default.VoiceCommandToggleMinimze);
        }

        private void RegisterGrammar(string actionId, string phrase)
        {
            if (!string.IsNullOrEmpty(phrase))
            {
                _engine.LoadGrammar(
                                    new Grammar(new GrammarBuilder(phrase))
                                    {
                                        Name = actionId
                                    });
            }
        }

        private void ClearEngine()
        {
            if (_engine != null)
            {
                try
                {
                    _engine.Dispose();
                }
                catch (Exception)
                {
                }
                _engine = null;
            }
        }
    }
}
