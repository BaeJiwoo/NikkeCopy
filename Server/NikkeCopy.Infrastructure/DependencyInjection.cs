using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NikkeCopy.Application.Authentication;
using NikkeCopy.Infrastructure.Authentication;
using NikkeCopy.Infrastructure.Persistence;

namespace NikkeCopy.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is Missing");

            services.AddDbContext<NikkeCopyDbContext>(options => options.UseMySQL(connectionString));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

            return services;
        }
    }
}
