//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Web.Http.Dependencies;
//using System.Web.Http.Validation;
//using EVE_Killboard_Analyser.Helper;
//using EVE_Killboard_Analyser.Helper.AnalysisProvider;
//using EVE_Killboard_Analyser.Helper.TagCreator;
//using Ninject;
//using Ninject.Web.WebApi.Filter;

//namespace EVE_Killboard_Analyser
//{
//    public class NinjectResolver : IDependencyResolver
//    {
//        private readonly StandardKernel _kernel;

//        public NinjectResolver()
//        {
//            _kernel = new StandardKernel();

        

//            _kernel.Load(Assembly.GetExecutingAssembly());
            
//            _kernel.Bind<IKillboard>().To<ZKillboard>();
            
//            _kernel.Bind<ITagCreator>().To<CynoTagCreator>();
//            _kernel.Bind<ITagCreator>().To<CarebearTagCreator>();
//            _kernel.Bind<ITagCreator>().To<GankerTagCreator>();
//            _kernel.Bind<ITagCreator>().To<CapitalCharTagCreator>();
//            _kernel.Bind<ITagCreator>().To<ECMTagCreator>();
//            _kernel.Bind<ITagCreator>().To<SmartbomberTag>();
//            _kernel.Bind<ITagCreator>().To<OffGridBoosterTagCreator>();

//            _kernel.Bind<IAnalysisProvider>().To<FavouriteShips>();
//            _kernel.Bind<IAnalysisProvider>().To<AvgShipCountOnRecentKills>();
//            _kernel.Bind<IAnalysisProvider>().To<OutOfAllianceAssociations>();
//            _kernel.Bind<IAnalysisProvider>().To<OutOfCorporationAssociations>();
//        }

//        public void Dispose()
//        {
//            _kernel.Dispose();
//        }

//        public object GetService(Type serviceType)
//        {
//            return _kernel.TryGet(serviceType);
//        }

//        public IEnumerable<object> GetServices(Type serviceType)
//        {
//            return _kernel.GetAll(serviceType);
//        }

//        public IDependencyScope BeginScope()
//        {
//            return new NinjectScope(_kernel);
//        }
//    }
//}