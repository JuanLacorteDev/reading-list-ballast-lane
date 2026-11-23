using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new SqliteConnectionProvider(connectionString));
        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<IUserRepository, SqliteUserRepository>();
        services.AddScoped<ITaskRepository, SqliteTaskRepository>();
        return services;
    }
}
