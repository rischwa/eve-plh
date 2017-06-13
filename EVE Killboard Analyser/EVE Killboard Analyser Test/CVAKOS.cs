using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EVE_Killboard_Analyser_Test
{
    [TestClass]
    public class CvaKosTest
    {
        [TestMethod]
        public void Test()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://kos.cva-eve.org/?c=koslist");

            request.UserAgent =
                "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.27 Safari/532.0 EVE-IGB";

            request.Headers["Cache-Control"] = "max-age=0";
            request.Headers["Eve-Shipname"] = "Fairy";
            request.Headers["Eve-Allianceid"] = "1988009451";
            request.Headers["Eve-Stationid"] = "61000221";
            request.Headers["Eve-Solarsystemid"] = "30003751";
            request.Headers["Eve-Corpname"] = "Rennfeuer";
            request.Headers["Eve-Constellationid"] = "20000547";
            request.Headers["Eve-Shipid"] = "1011932006262";
            request.Headers["Eve-Constellationname"] = "5-88B9";
            request.Headers["Eve-Corprole"] = "1016773386395870464";
            request.Headers["Eve-Charid"] = "738556982";
            request.Headers["Eve-Corpid"] = "322059498";
            request.Headers["Eve-Shiptypename"] = "Malediction";
            request.Headers["Eve-Shiptypeid"] = "11186";
            request.Headers["Eve-Alliancename"] = "Curatores Veritatis Alliance";
            request.Headers["Eve-Solarsystemname"] = "F-YH5B";
            request.Headers["Eve-Regionname"] = "Providence";
            request.Headers["Eve-Regionid"] = "10000047";
            request.Headers["Eve-Trusted"] = "Yes";
            request.Headers["Eve-Serverip"] = "87.237.38.200:26000";
            request.Headers["Eve-Charname"] = "Project 69";
            request.Headers["Eve-Stationname"] = "F-YH5B III - The Providence Petting Zoo";


            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.27 Safari/532.0 EVE-IGB";
            //}


            var postData = "list=-1&type=pilot";//pages 

            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                var rep =  reader.ReadToEnd();
                Console.WriteLine(rep);
            }
        }
    }
}
