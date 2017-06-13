using System.Diagnostics;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Services
{
    internal class ZKillboardService : IExternalKillboardService
    {
        public void OpenForCharacter(IEveCharacter character)
        {
            var url = string.Format(@"https://zkillboard.com/character/{0}/", character.Id);

            Process.Start(url);
        }

        public void OpenForSystem(SolarSystemViewModel systemId)
        {
            var url = string.Format(@"https://zkillboard.com/system/{0}/", systemId.ID);

            Process.Start(url);
        }
    }

}