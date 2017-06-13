using System;
using System.Reflection;
using System.Web;
using EVE_Killboard_Analyser;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Helper.AnalysisProvider;
using EVE_Killboard_Analyser.Helper.TagCreator;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace EVE_Killboard_Analyser
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load(Assembly.GetExecutingAssembly());

            kernel.Bind<IKillboard>().To<ZKillboard>();

            kernel.Bind<ITagCreator>().To<CynoTagCreator>();
            kernel.Bind<ITagCreator>().To<CarebearTagCreator>();
            kernel.Bind<ITagCreator>().To<GankerTagCreator>();
            kernel.Bind<ITagCreator>().To<CapitalCharTagCreator>();
            kernel.Bind<ITagCreator>().To<ECMTagCreator>();
            kernel.Bind<ITagCreator>().To<SmartbomberTag>();
            kernel.Bind<ITagCreator>().To<OffGridBoosterTagCreator>();

            kernel.Bind<IAnalysisProvider>().To<FavouriteShips>();
            kernel.Bind<IAnalysisProvider>().To<AvgShipCountOnRecentKills>();
            kernel.Bind<IAnalysisProvider>().To<OutOfAllianceAssociations>();
            kernel.Bind<IAnalysisProvider>().To<OutOfCorporationAssociations>();
        }        
    }
}
