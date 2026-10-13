using StudentService.Clients;
namespace StudentService.Core;

public sealed class SemesterLoopService(
    SemesterOrchestrator orchestrator,
    ISessionService sessionService,
    IHostApplicationLifetime lifetime,
    IHistory history,
    ILogger<SemesterLoopService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        {
            var state = await sessionService.LoadSessionStateAsync(ct);
            var days = await history.GetAllDaysAsync(state.SessionId, ct);
            orchestrator.SetHistory(state, days);
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(0.5));

        while (await timer.WaitForNextTickAsync(ct))
        {
            var state = await orchestrator.RunDayAsync(ct);
            await sessionService.SaveSessionAsync(state, ct);
            PrintStatus(state);

            if (state.Finished)
            {
                logger.LogInformation("Семестр завершён: utility={U}, expelled={E}",
                    state.Utility, state.Expelled);
                lifetime.StopApplication();
                return;
            }
        }
    }

    private static void PrintStatus(SemesterState state)
    {
        Console.WriteLine(
            $"Day {state.Day,3}: utility={state.Utility,6}, " +
            $"avg/day={state.AveragePerDay,6:F2}, skips={state.Skips}, " +
            $"expelled={state.Expelled}");
    }
}
