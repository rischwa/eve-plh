using System.Collections.Generic;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper
{
    public interface IAnalysisProvider
    {
        string FieldName { get; }

        object GetValueFromCollection(int characterId, IList<Kill> kills);
    }
}