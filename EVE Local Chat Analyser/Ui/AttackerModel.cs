using PLHLib;

namespace EveLocalChatAnalyser.Ui
{
    public class AttackerModel
    {
        private readonly Attacker _attacker;
        
        public AttackerModel(Attacker attacker)
        {
            _attacker = attacker;
        }

        public string CharacterName => _attacker.CharacterName;

        public string CorporationName => _attacker.CorporationName;

        public string AllianceName => _attacker.AllianceName;

        public string ShipName => ShipTypes.Instance[_attacker.ShipTypeID];
    }
}