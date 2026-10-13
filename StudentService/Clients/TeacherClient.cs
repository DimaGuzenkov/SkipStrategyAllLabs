using Contracts;
namespace StudentService.Clients;

using System.Net.Http.Json;
using Contracts;
using StudentService.Core;

public sealed class TeacherClient(IHttpClientFactory factory) : ITeacher
{
    private readonly IHttpClientFactory _factory = factory;

    public async Task<bool> WasAskedAsync(Guid sessionId, int day, Subject subject, CancellationToken ct)
    {
        var http = _factory.CreateClient($"teacher-{subject}");
        var req = new AskRequest(sessionId, day);
        var response = await http.PostAsJsonAsync("/api/ask", req, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AskResponse>(ct);
        return result!.Asked;
    }

}
