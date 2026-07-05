namespace SpecimenCheckin.Api.Models;

public enum CheckInAction
{
    Receive,
    Flag,
    Add,
    Close
}

public class CheckInEvent
{
    public Guid Id { get; set; }
    public Guid ManifestId { get; set; }
    public Guid? SpecimenId { get; set; }
    public CheckInAction Action { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset At { get; set; } = DateTimeOffset.UtcNow;

    public Manifest Manifest { get; set; } = null!;
}
