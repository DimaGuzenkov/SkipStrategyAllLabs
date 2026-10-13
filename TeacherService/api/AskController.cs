using TeacherService.Core;
namespace TeacherService.api;

using Contracts;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api")]
public sealed class AskController(ITeacher teacher) : ControllerBase
{
    private readonly ITeacher _teacher = teacher;

    [HttpPost("ask")]
    public async Task<ActionResult<AskResponse>> Ask(
        [FromBody] AskRequest req, CancellationToken ct)
    {
        var asked = await _teacher.WasAskedAsync(req.SessionId, req.Day, ct);
        return Ok(new AskResponse(asked));
    }
}
