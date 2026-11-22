using ReadingList.Api.Common.Hosting;
using ReadingList.Application;
using ReadingList.Infrastructure.Persistence.DI;
using ReadingList.Infrastructure.Persistence.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.PrepareDatabaseAsync(app.Configuration);

app.UseApiDefaults();

app.Run();
