using System.Collections.Generic;
using LiteDB;

namespace EveLocalChatAnalyser.Services.EVE_API
{
    public class EveCharacterPositions
    {
        public EveCharacterPositions()
        {
            Positions = new List<CharacterPosition>();
        }
        [BsonId]
        public long CharacterId { get; set; }
        public List<CharacterPosition> Positions { get; set; }
    }
}