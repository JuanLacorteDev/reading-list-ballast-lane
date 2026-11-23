using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts;
using TaskManager.Application.Users;

namespace TaskManager.Api.Controllers;

/// <summary>Minimal user creation for development and API testing only.</summary>
[Route("api/users")]
public sealed class UsersController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateUserRequest request, [FromServices] CreateUserUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new CreateUserCommand(request.Name), cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new CreateUserResponse(result.Value))
            : Problem(result.Error!);
    }
}
