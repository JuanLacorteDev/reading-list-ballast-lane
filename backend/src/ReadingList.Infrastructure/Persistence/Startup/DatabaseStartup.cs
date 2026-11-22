using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Infrastructure.Persistence.Seed;

namespace ReadingList.Infrastructure.Persistence.Startup;

public static class DatabaseStartup
{
    /// <summary>
    /// Creates the schema (idempotent) and, when "Database:SeedDemoData" is true, the demo user and books.
    /// </summary>
    public static async Task PrepareDatabaseAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await services.GetRequiredService<DatabaseInitializer>().InitializeAsync(cancellationToken);

        if (!configuration.GetValue("Database:SeedDemoData", defaultValue: false))
            return;

        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync(cancellationToken);
    }
}
