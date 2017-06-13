#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Properties;

#endregion

namespace EveLocalChatAnalyser
{
    public class KosWarner
    {
        private static readonly string TADA_SOUND_FILE = Environment.GetEnvironmentVariable("WINDIR") +
                                                         "\\Media\\tada.wav";

        private static readonly SoundPlayer KOS_WARNING_PLAYER = new SoundPlayer(TADA_SOUND_FILE);

        private IList<IEveCharacter> _characters = new List<IEveCharacter>();

        private static bool IsWarningForKos
        {
            get { return Settings.Default.IsUsingKosInformation; }
        }

        public void UpdateOfLocal(IList<IEveCharacter> characters)
        {
            UpdateCharacterConnections(characters);

            var hasKosCharacterEntered =
                characters.Any(
                    curCharacter => curCharacter.LocalChangeStatus == LocalChangeStatus.Entered && curCharacter.IsCvaKos);

            if (IsWarningForKos && hasKosCharacterEntered)
            {
                PlayKosWarning();
            }
        }

        private static void CharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var character = ((EveCharacter) sender);
            if (IsWarningForKos && character.IsCvaKos && IsInLocal(character))
            {
                PlayKosWarning();
            }
        }

        private static bool IsInLocal(IEveCharacter character)
        {
            return character.LocalChangeStatus != LocalChangeStatus.Exited;
        }

        private static void PlayKosWarning()
        {
            Task.Factory.StartNew(KOS_WARNING_PLAYER.Play);
        }

        private void UpdateCharacterConnections(IList<IEveCharacter> characters)
        {
            foreach (var curChar in _characters)
            {
                curChar.PropertyChanged -= CharacterPropertyChanged;
            }

            foreach (var curChar in characters)
            {
                curChar.PropertyChanged += CharacterPropertyChanged;
            }

            _characters = characters;
        }
    }
}