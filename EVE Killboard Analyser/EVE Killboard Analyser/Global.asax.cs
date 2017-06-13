using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Validation;
using EVE_Killboard_Analyser.Controllers;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.AnalysisProvider;
using EVE_Killboard_Analyser.Helper.Gatecamp;
using EVE_Killboard_Analyser.Helper.TagCreator;
using PLHLib;
using log4net;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.WebApi.Filter;

namespace EVE_Killboard_Analyser
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class KillEntries
    {
        public int CharacterId { get; set; }
        public IList<Kill> Kills { get; set; }
    }

    public class WebApiApplication : HttpApplication
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (WebApiApplication));
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure(); 

            //GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver();
            SetupDatabase();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            try
            {
                //TODO per DI das zeug reinladen
                var gateCampDetector = new GateCampDetector(new SqliteStargateLocationRepository());
                GateCampsWebSocketHandler.GateCampDetector = gateCampDetector;

                var gcd = new GateCampDetectionService(
                    gateCampDetector,
                    new GateCampDifferenceDetector());

                Task.Factory.StartNew(gcd.Start, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() => { ZKillboardRedisqClient.Start(gcd); }, TaskCreationOptions.LongRunning);
            }
            catch (Exception e)
            {
                LOG.Error("error", e);
            }
            //Task.Factory.StartNew(ZKillboardStompFeedConsumer.Start, TaskCreationOptions.LongRunning);
            LOG.Info("Application started successfully");
        }

     

        protected void Application_End()
        {
            LOG.Info("Shutting down");
        }
        

        private static void SetupDatabase()
        {
            Database.SetInitializer(new DatabaseContextInitializer());
        }
    }
}

