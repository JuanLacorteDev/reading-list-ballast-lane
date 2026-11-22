using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Abstractions.Security;
using ReadingList.Infrastructure.Persistence.Books;
using ReadingList.Infrastructure.Persistence.Seed;
using ReadingList.Infrastructure.Persistence.Users;
using ReadingList.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Infrastructure.Persistence.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
            services.AddSingleton<DatabaseInitializer>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<DemoDataSeeder>();

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            services.AddSingleton<ITokenService, JwtTokenService>();

            return services;
        }
    }
}
