namespace SpecimenCheckin.Api.Models;

public enum SpecimenStatus
{
    Pending,
    Received,
    Flagged,
    Added
}

public class Specimen
{
    public Guid Id { get; set; }
    public Guid ManifestId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Patient { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public SpecimenStatus Status { get; set; } = SpecimenStatus.Pending;
    public string? ReceivedBy { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public DateTimeOffset? AddedAt { get; set; }

    public Manifest Manifest { get; set; } = null!;
    public ICollection<Discrepancy> Discrepancies { get; set; } = new List<Discrepancy>();
}
