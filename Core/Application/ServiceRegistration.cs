using System.Reflection;
using Application.Common.Behaviors;
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

                // Order matters and is outermost-first: validate before doing any work, and keep
                // logging outside validation so rejected requests are still recorded.
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
                configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Picked up by ValidationBehavior. Replaces RegisterValidatorsFromAssemblyContaining
            // being called twice in Program.cs with two validators from the same assembly.
            services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

            // Removed: services.AddScoped<IHttpContextAccessor, HttpContextAccessor>().
            // That overrode the singleton registered by AddHttpContextAccessor() in Program.cs.
            // Program.cs now calls AddHttpContextAccessor() once and CurrentUserService consumes it.

            services.AddAutoMapper(typeof(ServiceRegistration));

            return services;
        }
    }
}
