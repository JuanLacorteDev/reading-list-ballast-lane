using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Api.Common.Authentication;
using ReadingList.Api.Common.Errors;
using ReadingList.Application.Books;
using ReadingList.Domain.Common.Enums;

namespace ReadingList.Books.Api.Controllers;

/// <summary>
/// CRUD for the signed-in user's books. The owner always comes from the token, never from the request.
/// </summary>
[Authorize]
[Route("api/books")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ReadingStatus? status, CancellationToken cancellationToken)
    {
        var result = await _bookService.ListAsync(User.GetUserId(), status, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.GetByIdAsync(User.GetUserId(), id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<BookResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.CreateAsync(User.GetUserId(), request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error!);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateAsync(User.GetUserId(), id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteAsync(User.GetUserId(), id, cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }
}
