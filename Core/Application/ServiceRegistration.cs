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

                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
                configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

            services.AddScoped<BusinessRules>();

            services.AddScoped<CandidateAccessPolicy>();

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
