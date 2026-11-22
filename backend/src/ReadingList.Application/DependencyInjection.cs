using Microsoft.Extensions.DependencyInjection;
using ReadingList.Application.Auth;
using ReadingList.Application.Books;

namespace ReadingList.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();

        return services;
    }
}
