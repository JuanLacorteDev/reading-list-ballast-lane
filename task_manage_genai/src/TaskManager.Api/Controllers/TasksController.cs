using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts;
using TaskManager.Api.CurrentUser;
using TaskManager.Application.Tasks;

namespace TaskManager.Api.Controllers;

[Route("api/tasks")]
[RequireUserIdHeader]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public sealed class TasksController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        CreateTaskRequest request, [FromServices] CreateTaskUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, TaskResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromServices] ListTasksUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value.Select(TaskResponse.From)) : Problem(result.Error!);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, [FromServices] GetTaskUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(TaskResponse.From(result.Value)) : Problem(result.Error!);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id, UpdateTaskRequest request, [FromServices] UpdateTaskUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request.ToCommand(), cancellationToken);
        return result.IsSuccess ? Ok(TaskResponse.From(result.Value)) : Problem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromServices] DeleteTaskUseCase useCase, CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }
}
