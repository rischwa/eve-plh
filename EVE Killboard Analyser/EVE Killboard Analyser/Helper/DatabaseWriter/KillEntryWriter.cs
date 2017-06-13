using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EVE_Killboard_Analyser.Models;
using PLHLib;
using log4net;

namespace EVE_Killboard_Analyser.Helper.DatabaseWriter
{
    public class KillEntryWriter : DatabaseWriter<KillEntries>
    {
        private const int MAX_NUMBER_OF_WRITE_REQUESTS = 200;
        private static readonly KillEntryWriter _instance = new KillEntryWriter();
        private static readonly ILog LOG = LogManager.GetLogger(typeof (KillEntryWriter));
        private static readonly LinkedList<long> LAST_KILL_IDS = new LinkedList<long>();
        private static readonly HashSet<long> LAST_KILL_IDS_HASH = new HashSet<long>();
        private const int MAX_LAST_KILL_COUNT = 500;

        private KillEntryWriter()
        {
            Start();
        }

        public static KillEntryWriter Instance
        {
            get { return _instance; }
        }

        protected override int MaxEntryCount
        {
            get { return MAX_NUMBER_OF_WRITE_REQUESTS; }
        }

        protected override void WriteNextEntryToDatabase()
        {
            var killEntry = WriteQueue.Take();
            //LOG.Debug(string.Format("{0}: writing next entry to database", killEntry.CharacterId));
            try
            {
                IList<Kill> newKills;
                var start = DateTime.UtcNow;
                using (var context = new DatabaseContext())
                {
                    context.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ COMMITTED;");
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;
                    newKills = GetNewKills(context, killEntry.Kills);
                    foreach (var curKill in newKills)
                    {
                        if (LAST_KILL_IDS.Count > MAX_LAST_KILL_COUNT)
                        {
                            var id = LAST_KILL_IDS.First();
                            LAST_KILL_IDS_HASH.Remove(id);
                            LAST_KILL_IDS.RemoveFirst();
                        }
                        LAST_KILL_IDS_HASH.Add(curKill.KillID);
                        LAST_KILL_IDS.AddLast(curKill.KillID);
                        curKill.Victim.KillID = curKill.KillID;
                    }
                    if (newKills.Count < killEntry.Kills.Count)
                    {
                        LOG.Debug(string.Format("{0}: {1} kills ignored: {2} [{3}]", killEntry.CharacterId,
                            killEntry.Kills.Count - newKills.Count, killEntry.Kills.Count == 1 ? killEntry.Kills.First().KillID : -1, Thread.CurrentThread.ManagedThreadId));
                    }
                    try
                    {
                        context.Kills.AddRange(newKills);

                        context.SaveChanges();
                    }
                    catch (Exception)
                    {
                        LOG.Error("ERROR IN WRITE: " + String.Join(", ", newKills.Select(x => x.KillID))+ "    :    " + String.Join(", ", newKills.Select(x=>x.Victim.KillID)));
                        throw;
                    }
                }
                if (newKills.Any())
                {
                    var elapsedSeconds = (DateTime.UtcNow - start).TotalSeconds;
                    LOG.Debug(string.Format("{0}: {2} kills successfully written in {1}s [{3}]", killEntry.CharacterId,
                                            elapsedSeconds, newKills.Count(), Thread.CurrentThread.ManagedThreadId));
                }
            }
            catch (Exception e)
            {

                LOG.Error(string.Format("{0}: error during write to kills [{1}]", killEntry.Kills.FirstOrDefault() == null ? killEntry.CharacterId :  killEntry.Kills.First().KillID, Thread.CurrentThread.ManagedThreadId
                    ), e);
            }
        }

        private static long[] GetExistingKillIds(DatabaseContext context, IEnumerable<Kill> newKillList)
        {
            var newIds = newKillList.Select(kill => kill.KillID).ToList();
            if (!newIds.Any())
            {
                return new long[0];
            }

            return
                context.ExecuteSqlQuery<long>(string.Format("select KillID from Kills where KillID in ({0})",
                                                            string.Join(",", newIds))).ToArray();
        }

        private static IList<Kill> GetNewKills(DatabaseContext context, IList<Kill> newKills)
        {
            CreateMissingCharacterDataEntries(context, newKills);

            CreateMissingCorporationDataEntries(context, newKills);

            CreateMissingAllianceDataEntries(context, newKills);

            var x = newKills.Where(kill => !LAST_KILL_IDS_HASH.Contains(kill.KillID)).ToList();
            if (!x.Any())
            {
                return x;
            }

            var existingIds = GetExistingKillIds(context, x);
            return x.Where(kill => !existingIds.Contains(kill.KillID)).ToList();
        }

        private static void CreateMissingDataEntries(DatabaseContext context, IList<Kill> newKills, Func<Character, int> getId, Func<Character, string> getName, string type  )
        {
            var allIds =
                   newKills.SelectMany(x => x.Attackers)
                           .Select(getId)
                           .Union(newKills.Select(x => getId(x.Victim)))
                           .Distinct()
                           .ToList();

            var existingIds = GetExistingIds(context, allIds, type);
            var newIds = allIds.Except(existingIds);

            var all  =
                newKills.SelectMany(x => x.Attackers)
                        .Cast<Character>()
                        .Union(newKills.Select(x => x.Victim))
                        .Where(x => newIds.Contains(getId(x)))
                        .GroupBy(getId)
                        .Select(x => new { Id = x.Key, Name = getName(x.First()) })
                        .ToList();

            if (!all.Any())
            {
                return;
            }

            //mssql can only insert 1000 rows at maximum per call
            const int MAX_ROWS = 1000;
            for (var i = 0; i < all.Count; i += MAX_ROWS)
            {
                var cur = all.Skip(i).Take(MAX_ROWS);
                var values = string.Join(",",
                                         cur.Select(
                                             x => "(" + x.Id + ",'" + x.Name.Replace("'", "''") + "')"));
                context.ExecuteSqlCommand(string.Format("INSERT INTO dbo.{0}Data VALUES {1}", type, values));
            }
        }

        private static void CreateMissingAllianceDataEntries(DatabaseContext context, IList<Kill> newKills)
        {
            CreateMissingDataEntries(context, newKills, character => character.AllianceID, character => character.AllianceName, "Alliance");
        }


        private static void CreateMissingCorporationDataEntries(DatabaseContext context, IList<Kill> newKills)
        {
            CreateMissingDataEntries(context, newKills, character => character.CorporationID, character => character.CorporationName, "Corporation");
        }

        private static void CreateMissingCharacterDataEntries(DatabaseContext context, IList<Kill> newKills)
        {
            CreateMissingDataEntries(context, newKills, character => character.CharacterID, character => character.CharacterName, "Character");
        }

        private static IList<int> GetExistingIds(DatabaseContext context, IList<int> allIds, string type)
        {
            if (!allIds.Any())
            {
                return new List<int>();
            }
            return context.ExecuteSqlQuery<int>(
                string.Format("SELECT {0}ID FROM dbo.{0}Data WHERE {0}ID IN ({1})", type,
                              string.Join(",", allIds)));
        }
    }
}