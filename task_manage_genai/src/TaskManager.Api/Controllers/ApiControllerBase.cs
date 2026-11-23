using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Errors;
using TaskManager.Application.Common.Results;

namespace TaskManager.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Translates an expected use-case failure into a ProblemDetails response.</summary>
    protected IActionResult Problem(Error error) => ErrorProblemDetails.ToActionResult(HttpContext, error);
}
