using Contracts;
namespace StudentService.Clients;

using System.Net;
using System.Net.Http.Json;
using StudentService.Core;


public interface ISessionService
{
    Task<SemesterState> LoadSessionStateAsync(CancellationToken ct);
    Task SaveSessionAsync(SemesterState state, CancellationToken ct);
}


public sealed class SessionClient(
    IHttpClientFactory factory,
    StartupOptions options
) : ISessionService
{
    private readonly HttpClient _http = factory.CreateClient("session");

    private async Task<SessionDto?> TryGetActiveAsync(string studentId, CancellationToken ct)
    {
        var response = await _http.GetAsync($"/api/session/active?studentId={studentId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SessionDto>(ct);
    }

    private async Task<SessionDto> CreateAsync(string studentId, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync("/api/session",
            new CreateSessionRequest(studentId), ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SessionDto>(ct))!;
    }

    public async Task SaveSessionAsync(SemesterState state, CancellationToken ct)
    {
        var response = await _http.PutAsJsonAsync(
            $"/api/session/{state.SessionId}", state.ToReq(), ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SemesterState> LoadSessionStateAsync(CancellationToken ct)
    {
        var session = options.ResumeIfPossible 
                                    ? await TryGetActiveAsync(options.StudentId, ct) 
                                    : null;
        session ??= await CreateAsync(options.StudentId, ct);
        var state = new SemesterState();
        state.Restore(session);
        return state;
    }

}
