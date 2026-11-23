using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.CurrentUser;
using TaskManager.Api.Errors;
using TaskManager.Application;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TaskManager")
    ?? throw new InvalidOperationException("Connection string 'TaskManager' is not configured.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Malformed / unreadable request bodies: same ProblemDetails shape as every other error.
        options.InvalidModelStateResponseFactory = context =>
        {
            const string code = "Request.Invalid";
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Title = code,
                Detail = "The request body is missing or malformed.",
                Status = StatusCodes.Status400BadRequest,
            };
            problem.Extensions["code"] = code;
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        };
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, HeaderCurrentUserContext>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

// DomainException → 400 (DomainExceptionHandler); anything else → 500 ProblemDetails, no internals.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
