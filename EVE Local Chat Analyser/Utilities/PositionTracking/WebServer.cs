using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using EveLocalChatAnalyser.Properties;
using NHttp;

namespace EveLocalChatAnalyser.Utilities.PositionTracking
{
    public class WebServer : INotifyPropertyChanged
    {
        private const string RESPONSE = @"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html>
<head>
<title>PLH Position Tracker</title>
{0}
</head>
{1}
</html>
";
        public const int DEFAULT_PORT = 42523;
        private static HttpServer _server;
        private bool _hasReceivedPositionUpdate;
        public SystemChanged SystemChanged;
        private readonly IDictionary<string, string> _lastSystemOfCharacter = new Dictionary<string, string>();
        private static readonly WebServer WS=new WebServer();

        public static WebServer Instance
        {
            get { return WS; }
        }


        public bool HasReceivedPositionUpdate
        {
            get { return _hasReceivedPositionUpdate; }
            private set
            {
                if (value.Equals(_hasReceivedPositionUpdate))
                {
                    return;
                }
                _hasReceivedPositionUpdate = value;
                OnPropertyChanged();
            }
        }

        public static string Url
        {
            get { return string.Format("http://localhost:{0}", DEFAULT_PORT); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Start()
        {
            if (_server == null)
            {
                _server = new HttpServer
                          {
                              EndPoint = new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT)
                          };
                _server.RequestReceived += OnRequestReceived;

                _server.Start();
            }
        }

        private void OnRequestReceived(object sender, HttpRequestEventArgs e)
        {
            if (e.Request.Path.StartsWith("/update"))
            {
                PositionUpdate(e);
                return;
            }
            IntialPageLoad(e);
        }

        private void PositionUpdate(HttpRequestEventArgs e)
        {
            using (var writer = new StreamWriter(e.Response.OutputStream))
            {
                writer.Write(!HasTrust(e) ? "no trust" : "ok");
            }
            var solarSystem = e.Request.Params["HTTP_EVE_SOLARSYSTEMNAME"];
            var characterName = e.Request.Params["HTTP_EVE_CHARNAME"];

            string lastSystemOfCharacter;
            _lastSystemOfCharacter.TryGetValue(characterName, out lastSystemOfCharacter);

            //tODO store position by char
            if (!string.IsNullOrEmpty(solarSystem) && solarSystem != lastSystemOfCharacter)
            {
                HasReceivedPositionUpdate = true;

                var s = SystemChanged;
                if (s != null)
                {
                    s.
                    Invoke(characterName, lastSystemOfCharacter, solarSystem);
                }

                _lastSystemOfCharacter[characterName] = solarSystem;
            }
        }

        private static void IntialPageLoad(HttpRequestEventArgs e)
        {
            using (var writer = new StreamWriter(e.Response.OutputStream))
            {
                var head = @"
<script type=""text/javascript"">
var wasOk = false;
function load(){
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange=function()
    {
        if (xmlHttp.readyState==4 && xmlHttp.status==200)
        {
            if(xmlHttp.responseText != 'ok'){
                wasOk = false;
                if (xmlHttp.responseText == 'no trust') {
                    document.body.innerHTML = ""Please trust this site in the in game browser and reload the page, otherwise EVE PLH won't be able to know your system."";            
                } else {
                    document.body.innerHTML = 'An unknown response was received from EVE PLH';
                }
            } else {
                if (!wasOk) {
                    document.body.innerHTML = ""UPDATE: Make sure you have the character selected in the settings for tracking. Otherwise this won't work.<br/>EVE PLH now receives your position, you can minimize this window by double clicking on the title bar."";
                }
                wasOk = true;
                window.setTimeout(load, 1000);
            }
            document.title = 'PLH Position Tracker';
        } else {
            document.body.innerHTML = 'No connection to EVE PLH, please start EVE PLH and reload this page<br/>' + xmlHttp.statusText;
            document.title = 'PLH Position Tracker (no connection)';
            wasOk = false;
        }
   }
   var r = Math.random();
   xmlHttp.open( 'GET', 'http://localhost:42523/update?r=' + r, true );
   xmlHttp.send();
}
    
</script>";
                var body = @"<body onload=""CCPEVE.requestTrust('http://localhost:42523');window.setTimeout(load, 1000);"">
You can minimize this window after trusting the website by double clicking on the title bar.
<p>
EVE PLH need the trust to get the system you are in, no information about you is transmitted over the internet!
</p>
</body>";
                writer.Write(RESPONSE, head, body);
            }
        }

        private static bool HasTrust(HttpRequestEventArgs e)
        {
            return "yes".Equals(e.Request.Params["HTTP_EVE_TRUSTED"], StringComparison.InvariantCultureIgnoreCase);
        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
