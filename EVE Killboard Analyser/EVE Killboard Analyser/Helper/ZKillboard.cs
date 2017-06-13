using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using EVE_Killboard_Analyser.Models;
using Newtonsoft.Json;
using PLHLib;
using log4net;

namespace EVE_Killboard_Analyser.Helper
{
    public class ZKillboard : IKillboard
    {
        //private const string ZKILLBOARD_API_URL = @"https://beta.eve-kill.net/api/{0}/characterID/{1}/";
        private const string ZKILLBOARD_API_URL = @"https://zkillboard.com/api/{0}/characterID/{1}/";
        private static readonly ILog LOG = LogManager.GetLogger(typeof (ZKillboard));

        public IList<Kill> GetKills(int characterId)
        {
            var url = GetKillsUrlForCharacter(characterId);
            try
            {
                var start = DateTime.UtcNow;

                var kills = RetrieveKills(url);

                var elapsedTimeInS = (DateTime.UtcNow - start).TotalSeconds;
                LOG.Debug(string.Format("{0}: retrieved {2} kills from {3} in {1}s", characterId, elapsedTimeInS,
                                        kills.Count, url));

                return kills;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Problem accessing zKillboard under {0}", url), e);
            }
        }

        public IList<Kill> GetLosses(int characterId)
        {
            var url = GetLossesUrlForCharacter(characterId);
            try
            {
                var start = DateTime.UtcNow;

                var losses = RetrieveKills(url);

                var elapsedTimeInS = (DateTime.UtcNow - start).TotalSeconds;
                LOG.Debug(string.Format("{0}: retrieved {2} losses from {3} in {1}s", characterId,
                                        elapsedTimeInS, losses.Count, url));

                return losses;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Problem accessing zKillboard under {0}", url), e);
            }
        }

        public CharacterStatistics GetStatistics(int characterId)
        {
            var url = GetStatisticsUrlForCharacter(characterId);
            try
            {
                var start = DateTime.UtcNow;

                var statistics = RetrieveStatistics(characterId, url);

                var elapsedTimeInS = (DateTime.UtcNow - start).TotalSeconds;
                LOG.Debug(string.Format("{0}: retrieved statistics from {2} in {1}s", characterId, elapsedTimeInS,
                                        url));

                return statistics;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Problem accessing zKillboard under {0}", url), e);
            }
        }

        private static CharacterStatistics RetrieveStatistics(int characterId, string url)
        {
            var response = GetHttpGetResponseFrom(url);
            response = response.Replace("null", "0");
            if (response == "[]")
            {
                return new CharacterStatistics {CharacterID = characterId, Groups = new List<SingleStatisticsForShipType>(), Totals = new SingleStatisticsEntry()};
            }
            var statistics = JsonConvert.DeserializeObject<CharacterStatisticsJson>(response);

            return statistics.ToDBCharacterStatistics(characterId);
        }

        private static string GetStatisticsUrlForCharacter(int characterId)
        {
            return string.Format(ZKILLBOARD_API_URL, "stats", characterId);
        }

        private static IList<Kill> RetrieveKills(string url)
        {
            var response = GetHttpGetResponseFrom(url);
            try
            {
                var kills = JsonConvert.DeserializeObject<IList<Kill>>(response);
                if (kills.Contains(null))
                {
                    LOG.Warn(string.Format("Some kills in {0} could not be deserialized", url));
                    return kills.Where(kill => kill != null).ToList();
                }
                return kills;
            }
            catch (JsonReaderException e)
            {
                DumpErroneousResponse(url, e, response);
                throw;
            }
        }

        private static void DumpErroneousResponse(string url, JsonReaderException e, string response)
        {
            var dumpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "dump.txt");
            using (var fs = new StreamWriter(new FileStream(dumpPath, FileMode.Create)))
            {
                fs.Write(url);
                fs.Write("\n");
                fs.Write(e.Message);
                fs.Write("\n");
                fs.Write(response);
            }
        }

        private string GetLossesUrlForCharacter(int characterId)
        {
            return string.Format(ZKILLBOARD_API_URL, "losses", characterId);
        }

        private static string GetKillsUrlForCharacter(int characterId)
        {
            return string.Format(ZKILLBOARD_API_URL, "kills", characterId);
        }

        private static string GetHttpGetResponseFrom(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Timeout = 2500;
            request.UserAgent = "eve-plh.com ( info@eve-plh.com )";
            SetupRequestHeader(request);

            try
            {
                using (var response = request.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    var contentEncodingValue = response.Headers["Content-Encoding"] ?? "";
                    if (contentEncodingValue.ToLowerInvariant() == "gzip")
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    }
                    var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    var httpWebResponse = e.Response as HttpWebResponse;
                    if (httpWebResponse == null)
                    {
                        throw;
                    }
                    var statusCode = httpWebResponse.StatusCode;
                    if (statusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        throw new Exception("zKillboard API appears to be down, please try again later", e);
                    }
                    //too many requests
                    if ((int) statusCode == 429)
                    {
                        var attempts = e.Response.Headers["X-Bin-Attempts"];
                        var between = e.Response.Headers["X-Bin-Seconds-Between-Request"];
                        var muh = e.Response.Headers["X-Bin-Attempts-Allowed"];

                        LOG.Error(
                            string.Format(
                                "Error requesting DataEntry from {3};\n\tX-Bin-Attempts: {0}; X-Bin-Attempts-Allowed: {1}; X-Bin-Seconds-Between-Request: {2}",
                                attempts,
                                muh, between, url), e);
                    }
                }

                throw;
            }
        }

        private static void SetupRequestHeader(WebRequest request)
        {
            request.ContentType = "application/json; charset=utf-8";
            request.Headers["Accept-Encoding"] = "gzip";
        }
    }
}