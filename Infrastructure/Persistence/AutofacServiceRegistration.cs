using Application.Abstractions;
using Application.Context;
using Application.Repositories;
using Autofac;
using Castle.DynamicProxy;
using Persistence.Concretes;
using Persistence.Context;
using Persistence.Repositories;

namespace Persistence
{
    public class AutofacServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //db context
            builder.RegisterType<MongoContext>().As<IMongoContext>().SingleInstance();

            //JobPosition dependencies
            builder.RegisterType<JobPositionManager>().As<IJobPositionService>().SingleInstance();
            builder.RegisterType<JobPositionWriteRepository>().As<IJobPositionWriteRepository>().SingleInstance();
            builder.RegisterType<JobPositionReadRepository>().As<IJobPositionReadRepository>().SingleInstance();
            builder.RegisterType<JobPositionDeleteRepository>().As<IJobPositionDeleteRepository>().SingleInstance();

            //Auth dependencies
            builder.RegisterType<AuthManager>().As<IAuthService>().SingleInstance();

            //JobSeeker dependencies
            builder.RegisterType<JobSeekerManager>().As<IJobSeekerService>().SingleInstance();
            builder.RegisterType<JobSeekerDeleteRepository>().As<IJobSeekerDeleteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerWriteRepository>().As<IJobSeekerWriteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerReadRepository>().As<IJobSeekerReadRepository>().SingleInstance();
            
            //var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            //builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
            //    .EnableInterfaceInterceptors(new ProxyGenerationOptions()
            //    {
            //        Selector = new AspectInterceptorSelector()
            //    }).SingleInstance();
        }
    }
}