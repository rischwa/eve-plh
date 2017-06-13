using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Microsoft.Web.WebSockets;
using PLHLib;

namespace EVE_Killboard_Analyser.Controllers
{
    public class GateCampsV1Controller : ApiController
    {
        public HttpResponseMessage Get()
        {
            HttpContext.Current.AcceptWebSocketRequest(new GateCampsWebSocketHandler());
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}
