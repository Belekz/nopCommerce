using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;

namespace Nop.Plugin.Payments.MollieByQuims.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        // Example for v4.30? 

        //public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        //{
        //    //builder.RegisterType<CustomService>().As<ICustomAttributeService>().InstancePerLifetimeScope();

        //    //builder.RegisterType<CustomModelFactory>().As<ICustomModelFactory>().InstancePerLifetimeScope();
        //}

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
                //builder.RegisterType<CustomService>().As<ICustomAttributeService>().InstancePerLifetimeScope();

               //builder.RegisterType<CustomModelFactory>().As<ICustomModelFactory>().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}
