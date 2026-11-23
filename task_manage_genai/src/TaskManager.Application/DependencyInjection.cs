using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;

namespace TaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateUserUseCase>();
        services.AddScoped<CreateTaskUseCase>();
        services.AddScoped<ListTasksUseCase>();
        services.AddScoped<GetTaskUseCase>();
        services.AddScoped<UpdateTaskUseCase>();
        services.AddScoped<DeleteTaskUseCase>();
        return services;
    }
}
