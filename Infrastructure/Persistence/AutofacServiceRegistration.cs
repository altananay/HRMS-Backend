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
using Persistence.Rules;

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
            builder.RegisterType<JobPositionBusinessRules>().SingleInstance();

            //Auth dependencies
            builder.RegisterType<AuthManager>().As<IAuthService>().SingleInstance();

            //Employer Auth dependencies
            builder.RegisterType<EmployerAuthManager>().As<IEmployerAuthService>().SingleInstance();

            //JobSeeker dependencies
            builder.RegisterType<JobSeekerManager>().As<IJobSeekerService>().SingleInstance();
            builder.RegisterType<JobSeekerDeleteRepository>().As<IJobSeekerDeleteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerWriteRepository>().As<IJobSeekerWriteRepository>().SingleInstance();
            builder.RegisterType<JobSeekerReadRepository>().As<IJobSeekerReadRepository>().SingleInstance();
            builder.RegisterType<JobSeekerBusinessRules>().SingleInstance();

            //system staff dependencies
            builder.RegisterType<SystemStaffManager>().As<ISystemStaffService>().SingleInstance();
            builder.RegisterType<SystemStaffDeleteRepository>().As<ISystemStaffDeleteRepository>().SingleInstance();
            builder.RegisterType<SystemStaffReadRepository>().As<ISystemStaffReadRepository>().SingleInstance();
            builder.RegisterType<SystemStaffWriteRepository>().As<ISystemStaffWriteRepository>().SingleInstance();
            builder.RegisterType<SystemStaffBusinessRules>().SingleInstance();

            //cv dependencies
            builder.RegisterType<CvManager>().As<ICVService>().SingleInstance();
            builder.RegisterType<CvDeleteRepository>().As<ICvDeleteRepository>().SingleInstance();
            builder.RegisterType<CvReadRepository>().As<ICvReadRepository>().SingleInstance();
            builder.RegisterType<CvWriteRepository>().As<ICvWriteRepository>().SingleInstance();
            builder.RegisterType<CvBusinessRules>().SingleInstance();

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
            builder.RegisterType<EmployerBusinessRules>().SingleInstance();

            //logs dependencies
            builder.RegisterType<LogReadRepository>().As<ILogReadRepository>().SingleInstance();
            builder.RegisterType<LogManager>().As<ILogService>().SingleInstance();

            //contacts dependencies
            builder.RegisterType<ContactManager>().As<IContactService>().SingleInstance();
            builder.RegisterType<ContactReadRepository>().As<IContactReadRepository>().SingleInstance();
            builder.RegisterType<ContactWriteRepository>().As<IContactWriteRepository>().SingleInstance();
            builder.RegisterType<ContactDeleteRepository>().As<IContactDeleteRepository>().SingleInstance();
            builder.RegisterType<ContactBusinessRules>().SingleInstance();

            //job applications
            builder.RegisterType<JobApplicationManager>().As<IJobApplicationService>().SingleInstance();
            builder.RegisterType<JobApplicationReadRepository>().As<IJobApplicationReadRepository>().SingleInstance();
            builder.RegisterType<JobApplicationWriteRepository>().As<IJobApplicationWriteRepository>().SingleInstance();
            builder.RegisterType<JobApplicationDeleteRepository>().As<IJobApplicationDeleteRepository>().SingleInstance();
            builder.RegisterType<JobApplicationBusinessRules>().SingleInstance();

            //job advertisements
            builder.RegisterType<JobAdvertisementManager>().As<IJobAdvertisementService>().SingleInstance();
            builder.RegisterType<JobAdvertisementReadRepository>().As<IJobAdvertisementReadRepository>().SingleInstance();
            builder.RegisterType<JobAdvertisementWriteRepository>().As<IJobAdvertisementWriteRepository>().SingleInstance();
            builder.RegisterType<JobAdvertisementDeleteRepository>().As<IJobAdvertisementDeleteRepository>().SingleInstance();
            builder.RegisterType<JobAdvertisementBusinessRules>().SingleInstance();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();
        }
    }
}