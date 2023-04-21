using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(mr => mr.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        }
    }
}
