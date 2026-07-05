namespace SpecimenCheckin.Api.Models;

public enum DiscrepancyType
{
    Missing,
    OffManifest
}

public enum DiscrepancyStatus
{
    Open,
    Resolved
}

public class Discrepancy
{
    public Guid Id { get; set; }
    public Guid ManifestId { get; set; }
    public Guid? SpecimenId { get; set; }
    public DiscrepancyType Type { get; set; }
    public string Note { get; set; } = string.Empty;
    public DiscrepancyStatus Status { get; set; } = DiscrepancyStatus.Open;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Manifest Manifest { get; set; } = null!;
    public Specimen? Specimen { get; set; }
}
