using Contracts;
using Humanizer;
using SessionService.Persistence;
namespace SessionService;

using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/session")]
public sealed class SessionController(ISessionRepository repo) : ControllerBase
{
    private readonly ISessionRepository _repo = repo;

    [HttpGet("active")]
    public async Task<ActionResult<SessionDto>> GetActive(
        [FromQuery] string studentId, CancellationToken ct)
    {
        var session = await _repo.GetActiveAsync(studentId, ct);
        if (session is null) return NotFound();
        return Ok(ToDto(session));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDto>> GetById(Guid id, CancellationToken ct)
    {
        var session = await _repo.GetByIdAsync(id, ct);
        if (session is null) return NotFound();
        return Ok(ToDto(session));
    }

    [HttpPost]
    public async Task<ActionResult<SessionDto>> Create(
        [FromBody] CreateSessionRequest req, CancellationToken ct)
    {
        var session = await _repo.CreateAsync(req.StudentId, ct);
        return Ok(ToDto(session));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateSessionRequest req, CancellationToken ct)
    {
        await _repo.UpdateProgressAsync(ToEntity(id, req), ct);
        return NoContent();
    }

    private static SessionDto ToDto(SessionEntity s) => new(
        s.Id, s.StudentId, s.SessionSeed,
        s.CurrentDay, s.Utility, s.Skips, s.Expelled, s.Finished);

    private static SessionEntity ToEntity(Guid id, UpdateSessionRequest s) => new()
    {
        Id = id,
        CurrentDay = s.CurrentDay,
        Utility = s.Utility,
        Skips = s.Skips,
        Expelled = s.Expelled,
        Finished = s.Finished
    };
}
