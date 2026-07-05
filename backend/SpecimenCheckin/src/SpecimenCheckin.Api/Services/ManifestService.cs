using Microsoft.EntityFrameworkCore;
using SpecimenCheckin.Api.Data;
using SpecimenCheckin.Api.DTOs;
using SpecimenCheckin.Api.Models;

namespace SpecimenCheckin.Api.Services;

public class ManifestService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ManifestService> _logger;

    public ManifestService(AppDbContext db, ILogger<ManifestService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<ManifestListItemDto>> GetManifestsForLabAsync(Guid labId, string? statusFilter = null)
    {
        var query = _db.Manifests
            .Include(m => m.Clinic)
            .Include(m => m.Specimens)
            .Where(m => m.LabId == labId);

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ManifestStatus>(statusFilter, true, out var parsed))
        {
            query = query.Where(m => m.Status == parsed);
        }

        return await query.Select(m => new ManifestListItemDto(
            m.Id,
            m.Code,
            m.Status.ToString(),
            m.Clinic.Name,
            m.Clinic.Location,
            m.SentAt,
            m.Specimens.Count,
            m.Specimens.Count(s => s.Status == SpecimenStatus.Received || s.Status == SpecimenStatus.Added),
            m.Specimens.Count(s => s.Status == SpecimenStatus.Flagged),
            m.Specimens.All(s => s.Status == SpecimenStatus.Received
                || s.Status == SpecimenStatus.Flagged
                || s.Status == SpecimenStatus.Added)
        )).ToListAsync();
    }

    public async Task<ManifestDetailDto?> GetManifestDetailAsync(Guid labId, Guid manifestId)
    {
        var manifest = await _db.Manifests
            .Include(m => m.Clinic)
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies).ThenInclude(d => d.Specimen)
            .FirstOrDefaultAsync(m => m.Id == manifestId);

        if (manifest == null || manifest.LabId != labId)
            return null;

        return MapToDetail(manifest);
    }

    public async Task<ManifestDetailDto?> ReceiveSpecimenAsync(Guid labId, Guid manifestId, Guid specimenId, string userId)
    {
        var manifest = await _db.Manifests
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies).ThenInclude(d => d.Specimen)
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.Id == manifestId);

        if (manifest == null || manifest.LabId != labId)
            return null;

        var specimen = manifest.Specimens.FirstOrDefault(s => s.Id == specimenId);
        if (specimen == null)
            return null;

        if (manifest.Status != ManifestStatus.Open)
            throw new InvalidOperationException("Cannot receive specimens on a manifest that is not Open.");

        if (specimen.Status == SpecimenStatus.Received || specimen.Status == SpecimenStatus.Added)
        {
            _logger.LogInformation("Specimen {SpecimenId} already received - idempotent skip", specimenId);
            return MapToDetail(manifest);
        }

        specimen.Status = SpecimenStatus.Received;
        specimen.ReceivedBy = userId;
        specimen.ReceivedAt = DateTimeOffset.UtcNow;

        _db.CheckInEvents.Add(new CheckInEvent
        {
            ManifestId = manifestId,
            SpecimenId = specimenId,
            Action = CheckInAction.Receive,
            UserId = userId
        });

        if (manifest.Status == ManifestStatus.InTransit)
            manifest.Status = ManifestStatus.Open;

        await _db.SaveChangesAsync();

        return MapToDetail(manifest);
    }

    public async Task<ManifestDetailDto?> FlagSpecimenAsync(Guid labId, Guid manifestId, Guid specimenId, string userId)
    {
        var manifest = await _db.Manifests
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies).ThenInclude(d => d.Specimen)
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.Id == manifestId);

        if (manifest == null || manifest.LabId != labId)
            return null;

        var specimen = manifest.Specimens.FirstOrDefault(s => s.Id == specimenId);
        if (specimen == null)
            return null;

        if (manifest.Status != ManifestStatus.Open)
            throw new InvalidOperationException("Cannot flag specimens on a manifest that is not Open.");

        specimen.Status = SpecimenStatus.Flagged;

        _db.Discrepancies.Add(new Discrepancy
        {
            ManifestId = manifestId,
            SpecimenId = specimenId,
            Type = DiscrepancyType.Missing,
            Note = $"Specimen {specimen.Code} flagged as missing by {userId}",
            Status = DiscrepancyStatus.Open
        });

        _db.CheckInEvents.Add(new CheckInEvent
        {
            ManifestId = manifestId,
            SpecimenId = specimenId,
            Action = CheckInAction.Flag,
            UserId = userId
        });

        await _db.SaveChangesAsync();

        return MapToDetail(manifest);
    }

    public async Task<ManifestDetailDto?> AddOffManifestSpecimenAsync(Guid labId, Guid manifestId, AddOffManifestRequest request, string userId)
    {
        var manifest = await _db.Manifests
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies).ThenInclude(d => d.Specimen)
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.Id == manifestId);

        if (manifest == null || manifest.LabId != labId)
            return null;

        if (manifest.Status != ManifestStatus.Open)
            throw new InvalidOperationException("Cannot add specimens to a manifest that is not Open.");

        var specimen = new Specimen
        {
            Id = Guid.NewGuid(),
            ManifestId = manifestId,
            Code = request.Code,
            Patient = request.Patient,
            Site = request.Site,
            Provider = request.Provider,
            Status = SpecimenStatus.Added,
            AddedAt = DateTimeOffset.UtcNow
        };

        _db.Specimens.Add(specimen);

        _db.Discrepancies.Add(new Discrepancy
        {
            ManifestId = manifestId,
            SpecimenId = specimen.Id,
            Type = DiscrepancyType.OffManifest,
            Note = request.Note ?? $"Off-manifest specimen {request.Code} added by {userId}",
            Status = DiscrepancyStatus.Open
        });

        _db.CheckInEvents.Add(new CheckInEvent
        {
            ManifestId = manifestId,
            SpecimenId = specimen.Id,
            Action = CheckInAction.Add,
            UserId = userId
        });

        await _db.SaveChangesAsync();

        return MapToDetail(manifest);
    }

    public async Task<ManifestDetailDto?> CloseManifestAsync(Guid labId, Guid manifestId, string userId)
    {
        var manifest = await _db.Manifests
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies).ThenInclude(d => d.Specimen)
            .Include(m => m.Clinic)
            .FirstOrDefaultAsync(m => m.Id == manifestId);

        if (manifest == null || manifest.LabId != labId)
            return null;

        if (manifest.Status != ManifestStatus.Open)
            throw new InvalidOperationException("Can only close a manifest that is Open.");

        var allHandled = manifest.Specimens.All(s =>
            s.Status == SpecimenStatus.Received ||
            s.Status == SpecimenStatus.Flagged ||
            s.Status == SpecimenStatus.Added);

        if (!allHandled)
            throw new InvalidOperationException("Cannot close manifest: not all specimens are reconciled. Pending specimens remain.");

        var hasOpenDiscrepancies = manifest.HasOpenDiscrepancies();
        manifest.Status = hasOpenDiscrepancies
            ? ManifestStatus.ClosedWithDiscrepancy
            : ManifestStatus.Closed;

        manifest.ClosedAt = DateTimeOffset.UtcNow;

        _db.CheckInEvents.Add(new CheckInEvent
        {
            ManifestId = manifestId,
            Action = CheckInAction.Close,
            UserId = userId
        });

        await _db.SaveChangesAsync();

        return MapToDetail(manifest);
    }

    private static ManifestDetailDto MapToDetail(Manifest m)
    {
        return new ManifestDetailDto(
            m.Id,
            m.Code,
            m.Status.ToString(),
            m.Clinic.Name,
            m.Clinic.Location,
            m.SentAt,
            m.ClosedAt,
            m.Specimens.Count,
            m.Specimens.Count(s => s.Status == SpecimenStatus.Received || s.Status == SpecimenStatus.Added),
            m.Specimens.Count(s => s.Status == SpecimenStatus.Pending),
            m.Specimens.Count(s => s.Status == SpecimenStatus.Flagged),
            m.Specimens.All(s =>
                s.Status == SpecimenStatus.Received ||
                s.Status == SpecimenStatus.Flagged ||
                s.Status == SpecimenStatus.Added),
            m.Specimens.Select(s => new SpecimenDto(
                s.Id, s.Code, s.Patient, s.Site, s.Provider,
                s.Status.ToString(), s.ReceivedBy, s.ReceivedAt
            )).ToList(),
            m.Discrepancies.Select(d => new DiscrepancyDto(
                d.Id, d.Type.ToString(), d.Note, d.Status.ToString(),
                d.CreatedAt, d.Specimen?.Code
            )).ToList()
        );
    }
}
