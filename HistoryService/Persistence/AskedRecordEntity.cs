namespace HistoryService.Persistence;

public sealed class AskedRecordEntity
{
    public Guid SessionId { get; set; }
    public int Day { get; set; }
    public byte Mask { get; set; }
    public byte RecordedMask { get; set; }
}