using System.Net.Http.Json;
using Contracts;
using StudentService.Core;
namespace StudentService.Clients;

public sealed class HistoryClient(IHttpClientFactory factory) : IHistory
{
    private readonly HttpClient _http = factory.CreateClient("history");

    public async Task<List<DaySnapshot>> GetAllDaysAsync(Guid sessionId, CancellationToken ct)
    {
        var response = await _http.GetAsync($"/api/session/{sessionId}/days", ct);
        response.EnsureSuccessStatusCode();
        var resp = await response.Content.ReadFromJsonAsync<SessionDaysResponse>(ct);
        return [.. resp.Days.Select(d => new DaySnapshot(d.Mask))];
    }
}
