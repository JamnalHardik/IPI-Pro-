namespace SpecimenCheckin.Api.Models;

public enum ManifestStatus
{
    InTransit,
    Open,
    Closed,
    ClosedWithDiscrepancy
}

public class Manifest
{
    public Guid Id { get; set; }
    public Guid LabId { get; set; }
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = string.Empty;
    public ManifestStatus Status { get; set; } = ManifestStatus.InTransit;
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public Lab Lab { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public ICollection<Specimen> Specimens { get; set; } = new List<Specimen>();
    public ICollection<Discrepancy> Discrepancies { get; set; } = new List<Discrepancy>();
    public ICollection<CheckInEvent> CheckInEvents { get; set; } = new List<CheckInEvent>();

    public bool HasOpenDiscrepancies() =>
        Discrepancies.Any(d => d.Status == DiscrepancyStatus.Open);
}
