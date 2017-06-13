using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Http;
using EVE_Killboard_Analyser.Helper;
using log4net;

namespace EVE_Killboard_Analyser.Controllers
{
    public class MessageOfTheDayController : ApiController
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(MessageOfTheDayController));
        public MessageOfTheDay Get()
        {
            LOGGER.Debug(string.Format("MotD request from {0} received", Request.GetClientIp()));
            using (var context = new DatabaseContext())
            {
                return context.MotDs.OrderByDescending(x => x.MessageNumber).First();
            }
            return new MessageOfTheDay
            {
                Text = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard\sa200\sl276\slmult1\lang9\f0\fs24
2014-08-10: Added 'Possible Booster' and 'Offgrid Booster' tags. Heuristics aren't finalized yet.\line If you find characters with tags that do not fit, please contact me.\line\line
2014-08-09: The EVE PLH database backend was changed.\line Everything should be running normal again,\line and you should experience a nice speedup in kb analysis retrieval.\line\line
The speedup allows for more up to date data,\line so that the kb analysis results you see should always be the current data.
}",
                MessageNumber = 118
            };
        }

    }

    public class MessageOfTheDay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageNumber { get; set; }
        public string Text { get; set; }
    }
}
