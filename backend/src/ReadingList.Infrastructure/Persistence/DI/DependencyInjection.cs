using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Infrastructure.Persistence.Books;
using ReadingList.Infrastructure.Persistence.Users;
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

            services.AddScoped<IUserRepository, UserRespository>();
            services.AddScoped<IBookRepository, BookRespository>();

            return services;
        }
    }
}
