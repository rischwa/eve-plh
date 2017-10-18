#region

using System.Diagnostics;
using EveLocalChatAnalyser.Ui.Models;

#endregion

namespace EveLocalChatAnalyser.Services
{
    internal class EveKillService : IExternalKillboardService
    {
        public void OpenForCharacter(IEveCharacter character)
        {
            var url = "https://zkillboard.com/charactor/search/" + character.Id + "/";

            Process.Start(url);
        }

        public void OpenForSystem(SolarSystemViewModel solarSystem)
        {
            var url = "https://zkillboard.com/system/" + solarSystem.ID + "/";

            Process.Start(url);
        }
    }
}
