using Contracts;
using HistoryService.Persistence;
using MassTransit;

namespace HistoryService;

public sealed class RecordAskedConsumer(Service history, ILogger<RecordAskedConsumer> logger)
    : IConsumer<RecordAskedRequest>
{
    public async Task Consume(ConsumeContext<RecordAskedRequest> context)
    {
        var m = context.Message;

        try
        {
            await history.RecordAsync(
                m.SessionId, m.Day, m.Subject, m.Asked, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to record: session={Session}, day={Day}, subject={Subject}",
                m.SessionId, m.Day, m.Subject);
            throw;
        }
    }
}
