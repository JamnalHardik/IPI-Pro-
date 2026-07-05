using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpecimenCheckin.Api.Data;
using SpecimenCheckin.Api.DTOs;
using SpecimenCheckin.Api.Models;

namespace SpecimenCheckin.Api.Tests;

public class TenantIsolationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TenantIsolationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb"));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                SeedIntegrationTestData(db);
            });
        });
    }

    private void SeedIntegrationTestData(AppDbContext db)
    {
        if (db.Labs.Any()) return;

        var lab1Id = Guid.NewGuid();
        var clinicId = Guid.NewGuid();

        db.Labs.Add(new Lab { Id = lab1Id, Name = "Lab A" });
        db.Clinics.Add(new Clinic { Id = clinicId, LabId = lab1Id, Name = "Clinic A", Location = "A" });

        var manifestA1 = new Manifest
        {
            Id = Guid.NewGuid(),
            LabId = lab1Id,
            ClinicId = clinicId,
            Code = "MF-A-001",
            Status = ManifestStatus.Open,
            SentAt = DateTimeOffset.UtcNow
        };
        db.Manifests.Add(manifestA1);

        db.Specimens.Add(new Specimen
        {
            Id = Guid.NewGuid(),
            ManifestId = manifestA1.Id,
            Code = "SP-A-01",
            Patient = "Patient A1",
            Site = "Blood",
            Provider = "Dr. A",
            Status = SpecimenStatus.Pending
        });

        db.SaveChanges();
    }

    private HttpClient CreateClient(string tenantId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        client.DefaultRequestHeaders.Add("X-User-Id", "test_user");
        return client;
    }

    [Fact]
    public async Task MissingTenantHeader_Returns400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "test_user");

        var response = await client.GetAsync("/api/manifests");
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DifferentTenant_Returns404_ForOthersManifests()
    {
        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        var lab1Manifest = db.Manifests.First(m => m.LabId == db.Labs.First(l => l.Name == "Lab A").Id);

        var client = CreateClient(Guid.NewGuid().ToString());
        var response = await client.GetAsync($"/api/manifests/{lab1Manifest.Id}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CorrectTenant_ReturnsManifests()
    {
        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        var labA = db.Labs.First(l => l.Name == "Lab A");

        var client = CreateClient(labA.Id.ToString());

        var response = await client.GetAsync("/api/manifests");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var manifests = await response.Content.ReadFromJsonAsync<List<ManifestListItemDto>>();
        Assert.NotNull(manifests);
        Assert.NotEmpty(manifests);
        Assert.All(manifests, m => Assert.Equal("MF-A-001", m.Code));
    }

    [Fact]
    public async Task CrossTenant_ReceiveSpecimen_Returns404()
    {
        var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        var labAManifest = db.Manifests.First();
        var labASpecimen = db.Specimens.First(s => s.ManifestId == labAManifest.Id);

        var client = CreateClient(Guid.NewGuid().ToString());

        var response = await client.PostAsync(
            $"/api/manifests/{labAManifest.Id}/specimens/{labASpecimen.Id}/receive", null);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
