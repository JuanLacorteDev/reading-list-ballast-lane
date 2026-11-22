using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReadingList.Api.Common.Authentication;
using ReadingList.Api.Common.Errors;
using ReadingList.Api.Common.OpenApi;
using Scalar.AspNetCore;

namespace ReadingList.Api.Common.Hosting;

/// <summary>
/// Cross-cutting setup shared by the Identity and Books APIs, so both behave the same way.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "Frontend";

    public static WebApplicationBuilder AddApiDefaults(this WebApplicationBuilder builder)
    {
        builder.ResolveSqlitePath();

        builder.Services
            .AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<DomainExceptionHandler>();

        builder.Services.AddJwtAuthentication(builder.Configuration);

        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(o => o.AddPolicy(FrontendCorsPolicy, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        builder.Services.AddOpenApi(o => o.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

        return builder;
    }

    public static WebApplication UseApiDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseCors(FrontendCorsPolicy);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Both APIs share one SQLite file. A relative "Data Source" is resolved against the content root,
    /// so the file is the same no matter which folder the API is started from.
    /// </summary>
    private static void ResolveSqlitePath(this WebApplicationBuilder builder)
    {
        const string key = "ConnectionStrings:Default";

        var connectionString = builder.Configuration[key];
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var csb = new SqliteConnectionStringBuilder(connectionString);
        if (csb.DataSource is ":memory:" || Path.IsPathRooted(csb.DataSource))
            return;

        csb.DataSource = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, csb.DataSource));
        builder.Configuration[key] = csb.ToString();
    }
}
