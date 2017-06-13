using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Ui.Settings;
using EveLocalChatAnalyser.Utilities;
using Newtonsoft.Json;
using PLHLib;

namespace EveLocalChatAnalyser.Services
{
    public interface ICoalitionService
    {
        Task<IList<Coalition>> GetCoalitions();
    }

    class CoalitionService : ICoalitionService
    {
        private class JsonAlliance
        {
            public string name { get; set; }
        }
        private class JsonCoalition
        {
            public string name { get; set; }
            public IList<JsonAlliance> alliances { get; set; }
        }


        private class CoalitionsContainer
        {
            public IList<JsonCoalition> coalitions;
        }
        private IList<Coalition> _coalitions; 
        public async Task<IList<Coalition>> GetCoalitions()
        {
            if (_coalitions == null)
            {
               TaskEx.Run(() => LoadCoalitions()).Wait();
            }
            return _coalitions;
            
        }

        private void LoadCoalitions()
        {
            const string URL = "http://rischwa.net/api/coalitions/current";
            var response = WebUtilities.GetHttpGetResponseFrom(URL);

            var result = JsonConvert.DeserializeObject<CoalitionsContainer>(response).coalitions;

            _coalitions = result.Select(
                                       x => new Coalition
                                            {
                                                Name = x.name,
                                                MemberAlliances = x.alliances.Select(y => y.name).ToList()
                                            }).ToArray();

        }
    }
}
