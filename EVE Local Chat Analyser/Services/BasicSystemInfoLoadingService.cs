using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EveLocalChatAnalyser.Model;
using EveLocalChatAnalyser.Services.EVE_API;
using EveLocalChatAnalyser.Ui.Models;

namespace EveLocalChatAnalyser.Services
{
    public class BasicSystemInfoLoadingService
    {
        
        public async Task<SolarSystemViewModel> GetSolarSystemViewModelBySystemName(string name)
        {
            var model = UniverseDataDB.GetSystemViewModelFor(name);
            var sovDataForSystem = SystemSovDataRepository.GetSovDataForSystem(model.ID).ConfigureAwait(false);
            var conquerableStationsBySystemId = new EveConquerableStationListService().GetConquerableStationsBySystemId().ConfigureAwait(false);
            var killsTask = new EveKillsService().GetKillsBySystemId().ConfigureAwait(false);
            var jumpsTask = new EveJumpsService().GetJumpsBySystemId().ConfigureAwait(false);

            int jumpCount;
            model.JumpCount = (await jumpsTask).TryGetValue(model.ID, out jumpCount) ? jumpCount : 0;

            var fwData = await new EveFactionWarfareService().GetFactionWarfareOccupancyBySystemId().ConfigureAwait(false);
            model.Sovereignity = await sovDataForSystem;

            var kills = await killsTask;
            KillsBySystem killsBySystem;
            model.Kills = kills.TryGetValue(model.ID, out killsBySystem)
                              ? killsBySystem
                              : new KillsBySystem {SolarSystemId = model.ID};
            
            var conquerableStations = await conquerableStationsBySystemId;

            InitConquerableStations(conquerableStations, model);
            InitFwValues(model, fwData);

            return model;
        }

        private static void InitConquerableStations(IDictionary<long, Station[]> conquerableStations, SolarSystemViewModel model)
        {
            Station[] conquerableStation;
            if (conquerableStations.TryGetValue(model.ID, out conquerableStation))
            {
                model.Stations = model.Stations.Concat(conquerableStation).ToArray();
            }
        }

        private static void InitFwValues(SolarSystemViewModel model, IDictionary<long, FactionWarfareOccupancy> fwData)
        {
            FactionWarfareOccupancy fwValue;
            model.Sovereignity.IsFactionWarefareSystem = fwData.TryGetValue(model.ID, out fwValue);
            if (model.Sovereignity.IsFactionWarefareSystem)
            {
// ReSharper disable PossibleNullReferenceException
                model.Sovereignity.FactionWarefareOccupyingFactionId = fwValue.OccupyingFactionId == 0 ? fwValue.OwningFactionId : fwValue.OccupyingFactionId;
// ReSharper restore PossibleNullReferenceException
            }
        }

        public async Task<IList<SolarSystemViewModel>> GetSurroundingSystemsFor(int level, params SolarSystemViewModel[] solarSystem)
        {
            var killsTask = new EveKillsService().GetKillsBySystemId().ConfigureAwait(false);
            var conquerableStationsTask = new EveConquerableStationListService().GetConquerableStationsBySystemId().ConfigureAwait(false);
            var fwDataTask = new EveFactionWarfareService().GetFactionWarfareOccupancyBySystemId().ConfigureAwait(false);
            var jumpsTask = new EveJumpsService().GetJumpsBySystemId().ConfigureAwait(false);

            
            var models = solarSystem.SelectMany(x=>UniverseDataDB.GetSurroundingSystemsFor(x, level)).ToArray();
           // var models = UniverseDataDB.GetLowSecSystems();
            var kills = await killsTask;
            var conquerableStations = await conquerableStationsTask;
            var fwData = await fwDataTask;
            var jumps = await jumpsTask;

            foreach (var curModel in models)
            {
                int jumpCount;
                curModel.JumpCount = jumps.TryGetValue(curModel.ID, out jumpCount) ? jumpCount : 0;

                curModel.Sovereignity = await SystemSovDataRepository.GetSovDataForSystem(curModel.ID).ConfigureAwait(false);

                KillsBySystem killsBySystem;
                curModel.Kills = kills.TryGetValue(curModel.ID, out killsBySystem)
                                  ? killsBySystem
                                  : new KillsBySystem { SolarSystemId = curModel.ID };

                InitConquerableStations(conquerableStations, curModel);
                InitFwValues(curModel, fwData);
            }

            return models;
        }

        //TODO extract similarities
        public async Task<IList<SolarSystemViewModel>> GetRegionalSystemsFor(SolarSystemViewModel system)
        {
            var killsTask = new EveKillsService().GetKillsBySystemId().ConfigureAwait(false);
            var conquerableStationsTask = new EveConquerableStationListService().GetConquerableStationsBySystemId().ConfigureAwait(false);
            var fwDataTask = new EveFactionWarfareService().GetFactionWarfareOccupancyBySystemId().ConfigureAwait(false);
            var jumpsTask = new EveJumpsService().GetJumpsBySystemId().ConfigureAwait(false);


            var models =  UniverseDataDB.GetRegionalSystemsFor(system).ToArray();
            var kills = await killsTask;
            var conquerableStations = await conquerableStationsTask;
            var fwData = await fwDataTask;
            var jumps = await jumpsTask;

            foreach (var curModel in models)
            {
                int jumpCount;
                curModel.JumpCount = jumps.TryGetValue(curModel.ID, out jumpCount) ? jumpCount : 0;

                curModel.Sovereignity = await SystemSovDataRepository.GetSovDataForSystem(curModel.ID).ConfigureAwait(false);

                KillsBySystem killsBySystem;
                curModel.Kills = kills.TryGetValue(curModel.ID, out killsBySystem)
                                  ? killsBySystem
                                  : new KillsBySystem { SolarSystemId = curModel.ID };

                InitConquerableStations(conquerableStations, curModel);
                InitFwValues(curModel, fwData);
            }

            var surroundings = await GetSurroundingSystemsFor(1, models);
            return models.Union(surroundings).ToArray();
        }

    }
}
