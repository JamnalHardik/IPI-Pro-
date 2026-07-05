namespace SpecimenCheckin.Api.Models;

public class Lab
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    public ICollection<Manifest> Manifests { get; set; } = new List<Manifest>();
}
