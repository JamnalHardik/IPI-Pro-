using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpecimenCheckin.Api.Data;
using SpecimenCheckin.Api.DTOs;
using SpecimenCheckin.Api.Models;
using SpecimenCheckin.Api.Services;

namespace SpecimenCheckin.Api.Tests;

public class ManifestServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ManifestService _service;
    private readonly Guid _lab1Id = Guid.NewGuid();
    private readonly Guid _lab2Id = Guid.NewGuid();
    private readonly Guid _clinicId = Guid.NewGuid();

    public ManifestServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _service = new ManifestService(_db, new LoggerFactory().CreateLogger<ManifestService>());

        SeedTestData();
    }

    private void SeedTestData()
    {
        _db.Labs.AddRange(
            new Lab { Id = _lab1Id, Name = "Lab 1" },
            new Lab { Id = _lab2Id, Name = "Lab 2" }
        );

        _db.Clinics.Add(new Clinic { Id = _clinicId, LabId = _lab1Id, Name = "Test Clinic", Location = "Bay 1" });

        var manifestId = Guid.NewGuid();
        _db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = _lab1Id,
            ClinicId = _clinicId,
            Code = "MF-TEST-001",
            Status = ManifestStatus.Open,
            SentAt = DateTimeOffset.UtcNow
        });

        _db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-001", Patient = "P1", Site = "Blood", Provider = "Dr. A", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-002", Patient = "P2", Site = "Urine", Provider = "Dr. A", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-003", Patient = "P3", Site = "Swab", Provider = "Dr. A", Status = SpecimenStatus.Pending }
        );

        _db.SaveChanges();
    }

    [Fact]
    public async Task GetManifests_ReturnsOnlyOwnLabData()
    {
        var lab1Manifests = await _service.GetManifestsForLabAsync(_lab1Id);
        Assert.NotEmpty(lab1Manifests);

        var lab2Manifests = await _service.GetManifestsForLabAsync(_lab2Id);
        Assert.Empty(lab2Manifests);
    }

    [Fact]
    public async Task GetManifestDetail_WrongTenant_ReturnsNull()
    {
        var manifest = _db.Manifests.First();

        var wrongTenant = await _service.GetManifestDetailAsync(_lab2Id, manifest.Id);
        Assert.Null(wrongTenant);

        var correctTenant = await _service.GetManifestDetailAsync(_lab1Id, manifest.Id);
        Assert.NotNull(correctTenant);
        Assert.Equal("MF-TEST-001", correctTenant.Code);
    }

    [Fact]
    public async Task ReceiveSpecimen_Idempotent_DoesNotCorruptCounts()
    {
        var manifest = _db.Manifests.First();
        var specimen = _db.Specimens.First(s => s.Status == SpecimenStatus.Pending && s.ManifestId == manifest.Id);

        var result1 = await _service.ReceiveSpecimenAsync(_lab1Id, manifest.Id, specimen.Id, "tech1");
        Assert.NotNull(result1);
        Assert.Equal("Received", result1.Specimens.First(s => s.Id == specimen.Id).Status);

        var result2 = await _service.ReceiveSpecimenAsync(_lab1Id, manifest.Id, specimen.Id, "tech1");
        Assert.NotNull(result2);
        Assert.Equal("Received", result2.Specimens.First(s => s.Id == specimen.Id).Status);

        var count = _db.CheckInEvents.Count(e => e.ManifestId == manifest.Id && e.SpecimenId == specimen.Id && e.Action == CheckInAction.Receive);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task FlagSpecimen_CreatesDiscrepancy()
    {
        var manifest = _db.Manifests.First();
        var specimen = _db.Specimens.First(s => s.Status == SpecimenStatus.Pending && s.ManifestId == manifest.Id);

        var result = await _service.FlagSpecimenAsync(_lab1Id, manifest.Id, specimen.Id, "tech1");
        Assert.NotNull(result);

        var updatedSpecimen = result.Specimens.First(s => s.Id == specimen.Id);
        Assert.Equal("Flagged", updatedSpecimen.Status);

        Assert.NotEmpty(result.Discrepancies);
        Assert.Contains(result.Discrepancies, d => d.Type == "Missing");
    }

    [Fact]
    public async Task AddOffManifest_CreatesDiscrepancy()
    {
        var manifest = _db.Manifests.First();

        var request = new AddOffManifestRequest("SP-999", "Off Patient", "Blood", "Dr. X", "Arrived without manifest entry");

        var result = await _service.AddOffManifestSpecimenAsync(_lab1Id, manifest.Id, request, "tech1");
        Assert.NotNull(result);

        var addedSpec = result.Specimens.First(s => s.Code == "SP-999");
        Assert.Equal("Added", addedSpec.Status);

        Assert.Contains(result.Discrepancies, d => d.Type == "OffManifest" && d.SpecimenCode == "SP-999");
    }

    [Fact]
    public async Task CloseManifest_RejectsUnreconciled()
    {
        var manifest = _db.Manifests.First();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CloseManifestAsync(_lab1Id, manifest.Id, "tech1"));
    }

    [Fact]
    public async Task CloseManifest_SucceedsWhenReconciled()
    {
        var manifest = _db.Manifests.First();
        var specimens = _db.Specimens.Where(s => s.ManifestId == manifest.Id).ToList();

        foreach (var s in specimens)
            await _service.ReceiveSpecimenAsync(_lab1Id, manifest.Id, s.Id, "tech1");

        var result = await _service.CloseManifestAsync(_lab1Id, manifest.Id, "tech1");
        Assert.NotNull(result);
        Assert.Equal("Closed", result.Status);
        Assert.True(result.IsReconciled);
    }

    [Fact]
    public async Task ReceiveSpecimen_CrossTenant_ReturnsNull()
    {
        var manifest = _db.Manifests.First();
        var specimen = _db.Specimens.First(s => s.ManifestId == manifest.Id);

        var result = await _service.ReceiveSpecimenAsync(_lab2Id, manifest.Id, specimen.Id, "hacker");
        Assert.Null(result);
    }

    [Fact]
    public async Task CloseManifest_CrossTenant_ReturnsNull()
    {
        var manifest = _db.Manifests.First();

        var result = await _service.CloseManifestAsync(_lab2Id, manifest.Id, "hacker");
        Assert.Null(result);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
