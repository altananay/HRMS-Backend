using Application.Abstractions;
using Application.Context;
using Application.Repositories;
using Application.Repositories.CvFiles;
using Application.Utilities.Interceptors;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Persistence.Concretes;
using Persistence.Context;
using Persistence.Repositories;
using Persistence.Repositories.File;

namespace Persistence
{
    public class AutofacServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //db context
            builder.RegisterType<MongoContext>().As<IMongoContext>().InstancePerLifetimeScope();

            //JobPosition dependencies
            builder.RegisterType<JobPositionManager>().As<IJobPositionService>().SingleInstance();
            builder.RegisterType<JobPositionWriteRepository>().As<IJobPositionWriteRepository>().SingleInstance();
            builder.RegisterType<JobPositionReadRepository>().As<IJobPositionReadRepository>().SingleInstance();
            builder.RegisterType<JobPositionDeleteRepository>().As<IJobPositionDeleteRepository>().SingleInstance();

            //Auth dependencies
            builder.RegisterType<AuthManager>().As<IAuthService>().SingleInstance();

            //Employer Auth dependencies
            builder.RegisterType<EmployerAuthManager>().As<IEmployerAuthService>().SingleInstance();

            //JobSeeker dependencies
            builder.RegisterType<JobSeekerManager>().As<IJobSeekerService>().SingleInstance();
            builder.RegisterType<JobSeekerDeleteRepository>().As<IJobSeekerDeleteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerWriteRepository>().As<IJobSeekerWriteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerReadRepository>().As<IJobSeekerReadRepository>().SingleInstance();

            //system staff dependencies
            builder.RegisterType<SystemStaffManager>().As<ISystemStaffService>().SingleInstance();
            builder.RegisterType<SystemStaffDeleteRepository>().As<ISystemStaffDeleteRepository>().SingleInstance();
            builder.RegisterType<SystemStaffReadRepository>().As<ISystemStaffReadRepository>().SingleInstance();
            builder.RegisterType<SystemStaffWriteRepository>().As<ISystemStaffWriteRepository>().SingleInstance();

            //cv dependencies
            builder.RegisterType<CvManager>().As<ICVService>().SingleInstance();
            builder.RegisterType<CvDeleteRepository>().As<ICvDeleteRepository>().SingleInstance();
            builder.RegisterType<CvReadRepository>().As<ICvReadRepository>().SingleInstance();
            builder.RegisterType<CvWriteRepository>().As<ICvWriteRepository>().SingleInstance();

            //user dependencies
            builder.RegisterType<UserManager>().As<IUserService>().SingleInstance();
            builder.RegisterType<UserDeleteRepository>().As<IUserDeleteRepository>().SingleInstance();
            builder.RegisterType<UserReadRepository>().As<IUserReadRepository>().SingleInstance();
            builder.RegisterType<UserWriteRepository>().As<IUserWriteRepository>().SingleInstance();

            //file dependencies
            builder.RegisterType<CvFileReadRepository>().As<ICvFileReadRepository>().SingleInstance();
            builder.RegisterType<CvFileWriteRepository>().As<ICvFileWriteRepository>().SingleInstance();
            builder.RegisterType<CvFileDeleteRepository>().As<ICvFileDeleteRepository>().SingleInstance();

            //employer dependencies
            builder.RegisterType<EmployerReadRepository>().As<IEmployerReadRepository>().SingleInstance();

            //
            builder.RegisterType<LogReadRepository>().As<ILogReadRepository>().SingleInstance();
            builder.RegisterType<LogManager>().As<ILogService>().SingleInstance();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();
        }
    }
}