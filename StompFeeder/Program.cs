using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PetaPoco;
using PLHLib;
using StompDotNet;
using StompDotNet.Message.Client;
using StompDotNet.Message.Server;

namespace StompFeeder
{
    public class ZKBKill
    {
        [JsonIgnore]
        public long killIDLong { get; set; }

        public ZKBKill(dynamic kill)
        {
            killID = kill.KillID.ToString(CultureInfo.InvariantCulture);
            solarSystemID = kill.SolarSystemID.ToString(CultureInfo.InvariantCulture);
            killTime = kill.KillTime.ToString("yyyy-MM-dd HH:mm:ss");
            moonId = kill.MoonID.ToString(CultureInfo.InvariantCulture);


            killIDLong = kill.KillID;
            //attackers = kill.Attackers.Select(x => new ZKBAttacker(x))
            //    .ToList();

            //victim = new ZKBVictim(kill.Victim);
            //items = kill.Items.Select(x => new ZKBItem(x))
            //    .ToList();
            //TODO set other stuff
        }

        public string killID { get; set; }

        public string solarSystemID { get; set; }

        public string killTime { get; set; }

        public string moonId { get; set; }

        public virtual IList<ZKBAttacker> attackers { get; set; }

        public virtual ZKBVictim victim { get; set; }

        public virtual IList<ZKBItem> items { get; set; }
    }

    public class ZKBItem
    {
        [JsonIgnore]
        public long killID { get; set; }


        public ZKBItem(dynamic item)
        {
            typeID = item.TypeID.ToString(CultureInfo.InvariantCulture);
            flag = item.Flag.ToString(CultureInfo.InvariantCulture);
            qtyDropped = item.QtyDropped.ToString(CultureInfo.InvariantCulture);
            qtyDestroyed = item.QtyDestroyed.ToString(CultureInfo.InvariantCulture);
            singleton = item.Singleton.ToString(CultureInfo.InvariantCulture);

            killID = item.KillID;
        }

        public string typeID { get; set; }

        public string flag { get; set; }

        public string qtyDropped { get; set; }

        public string qtyDestroyed { get; set; }

        public string singleton { get; set; }
    }


    public class ZKBVictim 
    {
        [JsonIgnore]
        public long killID { get; set; }


        public ZKBVictim(dynamic v)
        {
            shipTypeID = v.ShipTypeID.ToString(CultureInfo.InvariantCulture);
            damageTaken = v.DamageTaken.ToString(CultureInfo.InvariantCulture);

            characterID = v.CharacterID.ToString(CultureInfo.InvariantCulture);
            characterName = v.CharacterName;

            allianceID = v.AllianceID.ToString(CultureInfo.InvariantCulture);
            allianceName = v.AllianceName;

            corporationID = v.CorporationID.ToString(CultureInfo.InvariantCulture);
            corporationName = v.CorporationName;

            factionID = v.FactionID.ToString(CultureInfo.InvariantCulture);
            factionName = v.FactionName;

            killID = v.KillID;
        }

        public string shipTypeID { get; set; }

        public string damageTaken { get; set; }

        public string characterID { get; set; }

        public string characterName { get; set; }

        public string corporationID { get; set; }

        public string corporationName { get; set; }

        public string allianceID { get; set; }

        public string allianceName { get; set; }

        public string factionID { get; set; }

        public string factionName { get; set; }
    }

    public class ZKBAttacker
    {
        [JsonIgnore]
        public long killID { get; set; }

        public ZKBAttacker(dynamic a) 
        {
            securityStatus = a.SecurityStatus.ToString(CultureInfo.InvariantCulture);
            damageDone = a.DamageDone.ToString(CultureInfo.InvariantCulture);
            finalBlow = a.FinalBlow ? "1" : "0";
            weaponTypeID = a.WEaponTypeID.ToString(CultureInfo.InvariantCulture);
            shipTypeID = a.ShipTypeID.ToString(CultureInfo.InvariantCulture);

            characterID = a.CharacterID.ToString(CultureInfo.InvariantCulture);
            characterName = a.CharacterName;

            allianceID = a.AllianceID.ToString(CultureInfo.InvariantCulture);
            allianceName = a.AllianceName;

            corporationID = a.CorporationID.ToString(CultureInfo.InvariantCulture);
            corporationName = a.CorporationName;

            factionID = a.FactionID.ToString(CultureInfo.InvariantCulture);
            factionName = a.FactionName;

            killID = a.KillID;
        }
        public string securityStatus { get; set; }

        public string damageDone { get; set; }

        public string finalBlow { get; set; }

        public string weaponTypeID { get; set; }

        public string shipTypeID { get; set; }

        public string characterID { get; set; }

        public string characterName { get; set; }

        public string corporationID { get; set; }

        public string corporationName { get; set; }

        public string allianceID { get; set; }

        public string allianceName { get; set; }

        public string factionID { get; set; }

        public string factionName { get; set; }
    }

    internal class Program
    {

        private static bool stop = false;
        private static void Main(string[] args)
        {
            using (var _client = new Client("eve-kill.net", 61613, new Client.Authentication("rischwa", "6364d3f0f495b6ab9dcf8d3b5c6e0b01"))) { 
                _client.ErrorMessageReceived += ClientOnErrorMessageReceived;
            _client.Disconnected += ClientOnDisconnected;
            

            const string LASTIDWRITTEN_TXT = "lastIdWritten.txt";
            var line = File.Exists(LASTIDWRITTEN_TXT) ? File.ReadLines(LASTIDWRITTEN_TXT).FirstOrDefault() : null;

            long lastId = long.Parse(line ?? "0");
                var count = 0;
                using (
                    var db =
                        new Database(
                            "Data Source=localhost;Initial Catalog=extract;MultipleActiveResultSets=true;User ID=killboard;Password=!K1llb0ard!",
                            "System.Data.SqlClient"))
                {
                    int i = 0;
                    while (!stop)
                    {
                        var start = DateTime.UtcNow;
                        var kills = db.Fetch<dynamic>("SELECT TOP(250) * FROM kills WHERE KillID > @0 ORDER BY KillID ASC", lastId)
                            .Select(x => new ZKBKill(x))
                            .ToArray();

                        if (!kills.Any())
                        {
                            Console.WriteLine("Done");
                            Console.ReadLine();
                            return;
                        }

                        var killIds = string.Join(",", kills.Select(x => x.killID));

                        var victims = db.Fetch<dynamic>(string.Format("SELECT * FROM victims WHERE KillID IN ({0})", killIds))
                            .Select(x => new ZKBVictim(x))
                            .ToLookup(x => x.killID, x => x);

                        var attackers = db.Fetch<dynamic>(string.Format("SELECT * FROM attackers WHERE KillID IN ({0})", killIds))
                            .Select(x => new ZKBAttacker(x))
                            .ToLookup(x => x.killID, x => x);

                        var items = db.Fetch<dynamic>(string.Format("SELECT * FROM items WHERE KillID IN ({0})", killIds))
                            .Select(x => new ZKBItem(x))
                            .ToLookup(x => x.killID, x => x);

                        foreach (var curKill in kills)
                        {
                            if (stop)
                            {
                                Console.ReadLine();
                                return;
                            }
                            curKill.items = items[curKill.killIDLong].ToList();
                            curKill.attackers = attackers[curKill.killIDLong].ToList();
                            curKill.victim = victims[curKill.killIDLong].FirstOrDefault();
                            _client.Send("/topic/kills", "application / json", JsonConvert.SerializeObject(curKill));
                        }

                        count += 250;


                        var lastIdStr = kills.Last()
                            .killID;
                        lastId = long.Parse(lastIdStr);

                        File.WriteAllText(LASTIDWRITTEN_TXT, lastIdStr);
                        Console.WriteLine("took " + (DateTime.UtcNow - start).TotalSeconds + "s count: " + count);
                    }
                }
            }
        }

        private static void ClientOnDisconnected(Exception e)
        {
            Console.Error.WriteLine(e.Message);
            stop = true;
        }

        private static void ClientOnErrorMessageReceived(ErrorMessage errormessage)
        {
            Console.Error.WriteLine(errormessage.HeaderMessage);
            Console.Error.WriteLine(errormessage.BodyMessage);
            stop = true;
        }
    }
}
