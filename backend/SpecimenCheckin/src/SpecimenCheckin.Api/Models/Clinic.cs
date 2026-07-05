namespace SpecimenCheckin.Api.Models;

public class Clinic
{
    public Guid Id { get; set; }
    public Guid LabId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Lab Lab { get; set; } = null!;
    public ICollection<Manifest> Manifests { get; set; } = new List<Manifest>();
}
