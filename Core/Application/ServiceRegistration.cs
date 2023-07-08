using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(mr => mr.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper(typeof(ServiceRegistration));
        }
    }
}
