using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.AnalysisProvider;
using EVE_Killboard_Analyser.Helper.TagCreator;
using EVE_Killboard_Analyser.Models;
using PLHLib;
using log4net;
using Ninject.Activation;

namespace EVE_Killboard_Analyser.Controllers
{
    internal class Result : BaseResult<CharacterV1Data>
    {
        public Result(CharacterV1DataEntry dataEntry)
        {
            data = new CharacterV1Data(dataEntry);
        }
    }

    public class CharactersV1Controller : ApiController
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (CharactersV1Controller));
        private readonly IKillboard _killboard;
        private readonly ITagCreator[] _tagCreators;


        //public CharactersV1Controller(IKillboard killboard, ITagCreator[] tagCreators)
        //{
        //    _killboard = killboard;
        //    _tagCreators = tagCreators;
        //}
        public CharactersV1Controller()
        {
            _killboard = new ZKillboard();
            _tagCreators = new ITagCreator[] {new CapitalCharTagCreator(), new CarebearTagCreator(), new CynoTagCreator(), new ECMTagCreator(), new GankerTagCreator(), new OffGridBoosterTagCreator(), new SmartbomberTag(),  };
        }
        //  [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<Object> Get(int id)
        {
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            var clientIp = Request.GetClientIp();
            
            var start = DateTime.UtcNow;
            try
            {
                using (var context = new DatabaseContext())
                {
                    IpBlock.ThrowIfBlocked(Request, context);

                    context.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");

                    var result = await UpdateEntryFor(context, id);

                    LOGGER.Info(string.Format("{0}: request from {1} done in {2}s", clientIp, id, (DateTime.UtcNow - start).TotalSeconds));
                    return new Result(result);
                }
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (AggregateException e)
            {
                var innerException = e.Flatten();
                LOGGER.Error(string.Format("{0}: Error getting DataEntry for {1}", clientIp, id), innerException);
                return new {status = "error", message = innerException.Message, stacktrace = innerException.StackTrace};
            }
            catch (Exception e)
            {
                LOGGER.Error(string.Format("{0}: Error getting DataEntry for {1}", clientIp, id), e);
                return new {status = "error", message = e.Message, stacktrace = e.StackTrace};
            }
        }

        private async Task<CharacterV1DataEntry> UpdateEntryFor(DatabaseContext context, int id)
        {
            var statistics = Task.Factory.StartNew(() => _killboard.GetStatistics(id), TaskCreationOptions.LongRunning);
            var kills = await GetAllKills(id, context);
            var result = GetAnalysisFromCollection(id, kills);
            AddSpecialTags(context, id, result);
            try
            {
                result.Statistics = await statistics;
            }
            catch (Exception e)
            {
                LOGGER.Warn(string.Format("Could not get statistics for {0}", id), e);
            }
            result.LastSeen = LastSeen.GetValueFromCollection(context, id, kills);

            return result;
        }

        private static void AddSpecialTags(DatabaseContext context, int id, CharacterV1DataEntry result)
        {
            result.Tags = result.Tags.Concat(context.SpecialTags.Where(x => x.CharacterId == id)).ToList();
        }

        private Task<IList<Kill>> GetAllKills(int id, DatabaseContext context)
        {
            return Task.Factory.StartNew(() => (IList<Kill>) CharacterLoader.GetKillsOfCharacter(id, context),
                                         TaskCreationOptions.LongRunning);
        }

        private CharacterV1DataEntry GetAnalysisFromCollection(int id, IList<Kill> kills)
        {
            var favouriteShipsAnalyser = new FavouriteShips();
            var outOfAllianceAssociationsAnalyser = new OutOfAllianceAssociations();
            var outOfCorporationAssociationsAnalyser = new OutOfCorporationAssociations();
            var avgAttackerCount = new AvgShipCountOnRecentKills();

            //using (new TimeProfilingLogger("Collect tags/analysis"))
            //{
                var favouriteShipList = (List<string>) favouriteShipsAnalyser.GetValueFromCollection(id, kills);
                var attackerCount = (double) avgAttackerCount.GetValueFromCollection(id, kills);
                var outOfAllianceAssociations =
                    (List<string>) outOfAllianceAssociationsAnalyser.GetValueFromCollection(id, kills);
                var outOfCorporationAssociations =
                    (List<string>) outOfCorporationAssociationsAnalyser.GetValueFromCollection(id, kills);
                var theTags = _tagCreators.SelectMany(creator => creator.TagsFromCollection(id, kills)).ToList();
                if (theTags.Count > 1)
                {
                    theTags = theTags.Where(x => x != CarebearTagCreator.CAREBEAR).ToList();
                }

                return new CharacterV1DataEntry(id,
                                                favouriteShipList,
                                                attackerCount,
                                                theTags, outOfAllianceAssociations, outOfCorporationAssociations);
            //}
        }
    }
}