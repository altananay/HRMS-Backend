using System.Reflection;
using Application.Abstractions.Services;
using Application.Common.Behaviors;
using Application.Rules;
using Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var applicationAssembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(applicationAssembly);

                // Outermost first: logging wraps everything so rejected requests are still recorded,
                // and validation runs before the handler does any work.
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
                configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

            services.AddScoped<BusinessRules>();

            // The managers live here now rather than in Persistence, so Application owns both the
            // I*Service contracts and their implementations, and the database provider stays behind
            // the repository interfaces.
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<IContactService, ContactManager>();
            services.AddScoped<IJobPositionService, JobPositionManager>();
            services.AddScoped<IEmployerService, EmployerManager>();
            services.AddScoped<IJobSeekerService, JobSeekerManager>();
            services.AddScoped<ISystemStaffService, SystemStaffManager>();
            services.AddScoped<IUserService, UserManager>();
            services.AddScoped<ICvService, CvManager>();
            services.AddScoped<ICvFileService, CvFileManager>();
            services.AddScoped<IJobAdvertisementService, JobAdvertisementManager>();
            services.AddScoped<IJobApplicationService, JobApplicationManager>();

            return services;
        }
    }
}
