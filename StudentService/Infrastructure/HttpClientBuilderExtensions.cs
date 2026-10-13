using Polly;

namespace StudentService.Infrastructure;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddDefaultResilience(
        this IHttpClientBuilder builder,
        int retryCount = 3,
        string? clientName = null)
    {
        return builder.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
            retryCount: retryCount,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, delay, attempt, _) =>
                Console.WriteLine(
                    $"[RETRY] {clientName ?? "client"}: attempt {attempt}, " +
                    $"delay {delay.TotalSeconds}s, " +
                    $"reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}")));
    }
}
