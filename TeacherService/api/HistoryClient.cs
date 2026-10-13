using TeacherService.Core;
namespace TeacherService.api;

using Contracts;
using MassTransit;


public sealed class HistoryClient(HttpClient http, IPublishEndpoint publishEndpoint) : IHistory
{
    private readonly HttpClient _http = http;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<DaySnapshot> GetDayAsync(Guid sessionId, int day, CancellationToken ct)
    {
        var response = await _http.GetAsync($"/api/day/{day}?sessionId={sessionId}", ct);
        response.EnsureSuccessStatusCode();
        var dsr = await response.Content.ReadFromJsonAsync<DaySnapshotResponse>(ct);
        return new DaySnapshot(dsr?.Mask ?? 0);
    }

    public async Task RecordAsync(Guid sessionId, int day, Subject subject, bool asked, CancellationToken ct)
    {
        var req = new RecordAskedRequest(sessionId, day, (int)subject, asked);
        await _publishEndpoint.Publish(req, ct);
    }
}