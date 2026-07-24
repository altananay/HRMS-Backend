using Application.Abstractions;
using Application.Context;
using Application.Repositories;
using Application.Repositories.CvFiles;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Concretes;
using Persistence.Context;
using Persistence.Repositories;
using Persistence.Repositories.File;
using Persistence.Rules;

namespace Persistence
{
    /// <summary>
    /// Registers managers, repositories and business rules in the built-in container.
    /// </summary>
    /// <remarks>
    /// Replaces AutofacServiceRegistration. Three things changed beyond the container swap:
    ///
    /// 1. <b>Lifetimes.</b> Everything there was <c>.SingleInstance()</c> — managers, repositories,
    ///    business rules and MongoContext alike — while TokenHandler was registered <c>AddScoped</c>
    ///    in the MS container, making it a captive dependency with a mutable
    ///    <c>_accessTokenExpiration</c> field shared across concurrent requests. Everything here is
    ///    scoped, which is also what the EF Core DbContext arriving in Phase 3 requires.
    ///
    /// 2. <b>No interception.</b> The Autofac module ended with a blanket
    ///    <c>RegisterAssemblyTypes(...).AsImplementedInterfaces().EnableInterfaceInterceptors(...)</c>.
    ///    That scan, not the ~90 explicit lines above it, was what actually wired the aspects — and
    ///    since last-registration-wins, it silently overrode them. Cross-cutting concerns are now
    ///    MediatR pipeline behaviors.
    ///
    /// 3. <b>Everything is explicit.</b> EmployerManager, EmployerWriteRepository,
    ///    EmployerDeleteRepository and SystemStaffAuthManager were never registered by name — they
    ///    resolved only because of that trailing scan. Relying on a scan to cover gaps in an explicit
    ///    list is how those gaps stay invisible.
    /// </remarks>
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            // MongoClient is thread-safe and intended to be shared, so the context stays a singleton.
            // Replaced by a scoped HrmsDbContext in Phase 3.
            services.AddSingleton<IMongoContext, MongoContext>();

            services.AddManagers();
            services.AddRepositories();
            services.AddBusinessRules();

            return services;
        }

        private static void AddManagers(this IServiceCollection services)
        {
            services.AddScoped<IJobPositionService, JobPositionManager>();
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<IEmployerAuthService, EmployerAuthManager>();
            services.AddScoped<ISystemStaffAuthService, SystemStaffAuthManager>();
            services.AddScoped<IJobSeekerService, JobSeekerManager>();
            services.AddScoped<ISystemStaffService, SystemStaffManager>();
            services.AddScoped<ICVService, CvManager>();
            services.AddScoped<IUserService, UserManager>();
            services.AddScoped<ILogService, LogManager>();
            services.AddScoped<IContactService, ContactManager>();
            services.AddScoped<IJobApplicationService, JobApplicationManager>();
            services.AddScoped<IJobAdvertisementService, JobAdvertisementManager>();
            services.AddScoped<IEmployerService, EmployerManager>();
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IJobPositionReadRepository, JobPositionReadRepository>();
            services.AddScoped<IJobPositionWriteRepository, JobPositionWriteRepository>();
            services.AddScoped<IJobPositionDeleteRepository, JobPositionDeleteRepository>();

            services.AddScoped<IJobSeekerReadRepository, JobSeekerReadRepository>();
            services.AddScoped<IJobSeekerWriteRepository, JobSeekerWriteRepository>();
            services.AddScoped<IJobSeekerDeleteRepository, JobSeekerDeleteRepository>();

            services.AddScoped<ISystemStaffReadRepository, SystemStaffReadRepository>();
            services.AddScoped<ISystemStaffWriteRepository, SystemStaffWriteRepository>();
            services.AddScoped<ISystemStaffDeleteRepository, SystemStaffDeleteRepository>();

            services.AddScoped<ICvReadRepository, CvReadRepository>();
            services.AddScoped<ICvWriteRepository, CvWriteRepository>();
            services.AddScoped<ICvDeleteRepository, CvDeleteRepository>();

            services.AddScoped<IUserReadRepository, UserReadRepository>();
            services.AddScoped<IUserWriteRepository, UserWriteRepository>();
            services.AddScoped<IUserDeleteRepository, UserDeleteRepository>();

            services.AddScoped<ICvFileReadRepository, CvFileReadRepository>();
            services.AddScoped<ICvFileWriteRepository, CvFileWriteRepository>();
            services.AddScoped<ICvFileDeleteRepository, CvFileDeleteRepository>();

            services.AddScoped<IEmployerReadRepository, EmployerReadRepository>();
            services.AddScoped<IEmployerWriteRepository, EmployerWriteRepository>();
            services.AddScoped<IEmployerDeleteRepository, EmployerDeleteRepository>();

            services.AddScoped<IContactReadRepository, ContactReadRepository>();
            services.AddScoped<IContactWriteRepository, ContactWriteRepository>();
            services.AddScoped<IContactDeleteRepository, ContactDeleteRepository>();

            services.AddScoped<IJobApplicationReadRepository, JobApplicationReadRepository>();
            services.AddScoped<IJobApplicationWriteRepository, JobApplicationWriteRepository>();
            services.AddScoped<IJobApplicationDeleteRepository, JobApplicationDeleteRepository>();

            services.AddScoped<IJobAdvertisementReadRepository, JobAdvertisementReadRepository>();
            services.AddScoped<IJobAdvertisementWriteRepository, JobAdvertisementWriteRepository>();
            services.AddScoped<IJobAdvertisementDeleteRepository, JobAdvertisementDeleteRepository>();

            services.AddScoped<ILogReadRepository, LogReadRepository>();

            // JobApplicationBusinessRules injects the CONCRETE JobApplicationReadRepository rather
            // than its interface, so the concrete type needs its own registration. Fixed in Phase 3
            // when the rules move to Application and can only see interfaces.
            services.AddScoped<JobApplicationReadRepository>();
        }

        private static void AddBusinessRules(this IServiceCollection services)
        {
            // Registered as concrete types: the *BusinessRules classes have no interfaces and are
            // injected by concrete type into the managers.
            services.AddScoped<JobPositionBusinessRules>();
            services.AddScoped<JobSeekerAuthBusinessRules>();
            services.AddScoped<JobSeekerBusinessRules>();
            services.AddScoped<SystemStaffBusinessRules>();
            services.AddScoped<CvBusinessRules>();
            services.AddScoped<EmployerBusinessRules>();
            services.AddScoped<ContactBusinessRules>();
            services.AddScoped<JobApplicationBusinessRules>();
            services.AddScoped<JobAdvertisementBusinessRules>();
        }
    }
}
