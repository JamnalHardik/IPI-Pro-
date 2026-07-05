using SpecimenCheckin.Api.Models;

namespace SpecimenCheckin.Api.DTOs;

public record ManifestListItemDto(
    Guid Id,
    string Code,
    string Status,
    string ClinicName,
    string ClinicLocation,
    DateTimeOffset SentAt,
    int TotalExpected,
    int TotalReceived,
    int TotalFlagged,
    bool IsReconciled
);

public record ManifestDetailDto(
    Guid Id,
    string Code,
    string Status,
    string ClinicName,
    string ClinicLocation,
    DateTimeOffset SentAt,
    DateTimeOffset? ClosedAt,
    int TotalExpected,
    int TotalReceived,
    int TotalPending,
    int TotalFlagged,
    bool IsReconciled,
    List<SpecimenDto> Specimens,
    List<DiscrepancyDto> Discrepancies
);

public record SpecimenDto(
    Guid Id,
    string Code,
    string Patient,
    string Site,
    string Provider,
    string Status,
    string? ReceivedBy,
    DateTimeOffset? ReceivedAt
);

public record DiscrepancyDto(
    Guid Id,
    string Type,
    string Note,
    string Status,
    DateTimeOffset CreatedAt,
    string? SpecimenCode
);

public record AddOffManifestRequest(
    string Code,
    string Patient,
    string Site,
    string Provider,
    string? Note
);
