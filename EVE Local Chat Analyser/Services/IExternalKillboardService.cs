using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Services
{
    public enum ExternalServiceType
    {
        EveKill = 0,
        ZKillboard = 1
    }

    public interface IExternalKillboardService
    {
        void OpenForCharacter(IEveCharacter character);
        void OpenForSystem(SolarSystemViewModel systemModel);
    }
}