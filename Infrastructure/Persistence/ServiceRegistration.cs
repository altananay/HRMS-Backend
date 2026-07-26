using Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interceptors;
using Persistence.Repositories;

namespace Persistence
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:Postgres is not configured. Start the database with " +
                    "`docker compose up -d`; appsettings.Development.json already points at it.");

            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<AuditingSaveChangesInterceptor>();

            services.AddDbContext<HrmsDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(HrmsDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                });

                options.UseSnakeCaseNamingConvention();

                options.AddInterceptors(serviceProvider.GetRequiredService<AuditingSaveChangesInterceptor>());
            });

            services.AddRepositories();

            return services;
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJobSeekerRepository, JobSeekerRepository>();
            services.AddScoped<IEmployerRepository, EmployerRepository>();
            services.AddScoped<ISystemStaffRepository, SystemStaffRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            services.AddScoped<ICvRepository, CvRepository>();
            services.AddScoped<ICvFileRepository, CvFileRepository>();

            services.AddScoped<IJobPositionRepository, JobPositionRepository>();
            services.AddScoped<IJobAdvertisementRepository, JobAdvertisementRepository>();
            services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
        }
    }
}
