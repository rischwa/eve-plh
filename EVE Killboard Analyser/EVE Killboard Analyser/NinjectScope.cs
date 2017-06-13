//using System;
//using System.Collections.Generic;
//using System.Web.Http.Dependencies;
//using Ninject;

//namespace EVE_Killboard_Analyser
//{
//    public class NinjectScope : IDependencyScope
//    {
//        private readonly IKernel _kernel;

//        public NinjectScope(IKernel kernel)
//        {
//            _kernel = kernel;
//        }

//        public void Dispose()
//        {
//        }

//        public object GetService(Type serviceType)
//        {
//            return _kernel.Get(serviceType);
//        }

//        public IEnumerable<object> GetServices(Type serviceType)
//        {
//            return _kernel.GetAll(serviceType);
//        }
//    }
//}