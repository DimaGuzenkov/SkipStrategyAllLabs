namespace HistoryService;

using Contracts;
using HistoryService.Persistence;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public sealed class HistoryController(Service history) : ControllerBase
{
    [HttpGet("day/{day:int}")]
    public async Task<ActionResult<DaySnapshotResponse>> GetDay(
        int day, [FromQuery] Guid sessionId, CancellationToken ct)
    {
        try
        {
            var mask = await history.GetDayMaskAsync(sessionId, day, ct);
            return Ok(new DaySnapshotResponse(mask));
        }
        catch (DayNotClosedException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "Day not closed");
        }
    }

    [HttpGet("session/{sessionId:guid}/days")]
    public async Task<ActionResult<SessionDaysResponse>> GetSessionDays(
        Guid sessionId, CancellationToken ct)
    {
        var days = await history.GetAllClosedDaysAsync(sessionId, ct);
        var result = days
            .Select(r => new DayRecord(r.Day, r.Mask))
            .ToList();
        return Ok(new SessionDaysResponse(result));
    }
}
