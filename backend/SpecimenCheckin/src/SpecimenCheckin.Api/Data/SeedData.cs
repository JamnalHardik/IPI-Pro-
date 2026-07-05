using SpecimenCheckin.Api.Models;

namespace SpecimenCheckin.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Labs.Any()) return;

        // Fixed GUIDs so the front-end tenant switcher can reference them
        var lab1Id = new Guid("00000000-0000-0000-0000-000000000001");
        var lab2Id = new Guid("00000000-0000-0000-0000-000000000002");

        var lab1Clinic1Id = Guid.NewGuid();
        var lab1Clinic2Id = Guid.NewGuid();
        var lab1Clinic3Id = Guid.NewGuid();
        var lab2Clinic1Id = Guid.NewGuid();
        var lab2Clinic2Id = Guid.NewGuid();

        // Labs
        db.Labs.AddRange(
            new Lab { Id = lab1Id, Name = "Central Lab" },
            new Lab { Id = lab2Id, Name = "North Lab" }
        );

        // Clinics
        db.Clinics.AddRange(
            new Clinic { Id = lab1Clinic1Id, LabId = lab1Id, Name = "Riverside Clinic", Location = "Bay 2" },
            new Clinic { Id = lab1Clinic2Id, LabId = lab1Id, Name = "Downtown Medical", Location = "Floor 1" },
            new Clinic { Id = lab1Clinic3Id, LabId = lab1Id, Name = "Suburban Health", Location = "Wing A" },
            new Clinic { Id = lab2Clinic1Id, LabId = lab2Id, Name = "Northside Clinic", Location = "Main" },
            new Clinic { Id = lab2Clinic2Id, LabId = lab2Id, Name = "East End Medical", Location = "Level 3" }
        );

        // === Manifests for Central Lab ===

        var mf42Id = Guid.NewGuid();
        CreateManifest42(db, mf42Id, lab1Id, lab1Clinic1Id);

        var mf45Id = Guid.NewGuid();
        CreateManifest45(db, mf45Id, lab1Id, lab1Clinic2Id);

        var mf41Id = Guid.NewGuid();
        CreateManifest41(db, mf41Id, lab1Id, lab1Clinic3Id);

        var mf40Id = Guid.NewGuid();
        CreateManifest40(db, mf40Id, lab1Id, lab1Clinic1Id);

        var mf39Id = Guid.NewGuid();
        CreateManifest39(db, mf39Id, lab1Id, lab1Clinic2Id);

        // === Manifests for North Lab ===

        var mf43Id = Guid.NewGuid();
        CreateManifest43(db, mf43Id, lab2Id, lab2Clinic1Id);

        var mf44Id = Guid.NewGuid();
        CreateManifest44(db, mf44Id, lab2Id, lab2Clinic2Id);

        db.SaveChanges();
    }

    private static void CreateManifest42(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        var manifest = new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0042",
            Status = ManifestStatus.Open,
            SentAt = new DateTimeOffset(2026, 6, 29, 8, 30, 0, TimeSpan.Zero)
        };

        var specimens = new List<Specimen>
        {
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42001", Patient = "J. Anderson", Site = "Blood", Provider = "Dr. Chen", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 30, 9, 15, 0, TimeSpan.Zero) },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42002", Patient = "M. Brooks", Site = "Urine", Provider = "Dr. Chen", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 30, 9, 16, 0, TimeSpan.Zero) },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42003", Patient = "T. Williams", Site = "Tissue", Provider = "Dr. Gupta", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 30, 9, 17, 0, TimeSpan.Zero) },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42004", Patient = "R. Johnson", Site = "Swab", Provider = "Dr. Chen", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 30, 9, 18, 0, TimeSpan.Zero) },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42005", Patient = "S. Lee", Site = "Blood", Provider = "Dr. Gupta", Status = SpecimenStatus.Pending },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42006", Patient = "A. Martinez", Site = "Urine", Provider = "Dr. Chen", Status = SpecimenStatus.Flagged },
            new() { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-42007", Patient = "B. Okafor", Site = "Tissue", Provider = "Dr. Gupta", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 30, 9, 20, 0, TimeSpan.Zero) }
        };

        var spec42008 = new Specimen
        {
            Id = Guid.NewGuid(),
            ManifestId = manifestId,
            Code = "SP-42008",
            Patient = "C. Nguyen",
            Site = "Swab",
            Provider = "Dr. Chen",
            Status = SpecimenStatus.Added,
            AddedAt = new DateTimeOffset(2026, 6, 30, 9, 25, 0, TimeSpan.Zero)
        };

        specimens.Add(spec42008);

        var flaggedSpec = specimens.First(s => s.Code == "SP-42006");

        db.Manifests.Add(manifest);
        db.Specimens.AddRange(specimens);

        db.Discrepancies.AddRange(
            new Discrepancy
            {
                Id = Guid.NewGuid(),
                ManifestId = manifestId,
                SpecimenId = flaggedSpec.Id,
                Type = DiscrepancyType.Missing,
                Note = $"Specimen {flaggedSpec.Code} flagged as missing — bottle not in shipment",
                Status = DiscrepancyStatus.Open,
                CreatedAt = new DateTimeOffset(2026, 6, 30, 9, 30, 0, TimeSpan.Zero)
            },
            new Discrepancy
            {
                Id = Guid.NewGuid(),
                ManifestId = manifestId,
                SpecimenId = spec42008.Id,
                Type = DiscrepancyType.OffManifest,
                Note = $"Off-manifest specimen {spec42008.Code} arrived — not on manifest",
                Status = DiscrepancyStatus.Open,
                CreatedAt = new DateTimeOffset(2026, 6, 30, 9, 25, 0, TimeSpan.Zero)
            }
        );

        db.CheckInEvents.AddRange(
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[0].Id, Action = CheckInAction.Receive, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 15, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[1].Id, Action = CheckInAction.Receive, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 16, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[2].Id, Action = CheckInAction.Receive, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 17, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[3].Id, Action = CheckInAction.Receive, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 18, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[4].Id, Action = CheckInAction.Flag, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 30, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = spec42008.Id, Action = CheckInAction.Add, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 25, 0, TimeSpan.Zero) },
            new CheckInEvent { Id = Guid.NewGuid(), ManifestId = manifestId, SpecimenId = specimens[5].Id, Action = CheckInAction.Receive, UserId = "tech1", At = new DateTimeOffset(2026, 6, 30, 9, 20, 0, TimeSpan.Zero) }
        );
    }

    private static void CreateManifest45(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0045",
            Status = ManifestStatus.Open,
            SentAt = new DateTimeOffset(2026, 6, 29, 14, 0, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-45001", Patient = "D. Kim", Site = "Blood", Provider = "Dr. Park", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 30, 8, 0, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-45002", Patient = "E. Garcia", Site = "Urine", Provider = "Dr. Park", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 30, 8, 1, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-45003", Patient = "F. Brown", Site = "Swab", Provider = "Dr. Park", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 30, 8, 2, 0, TimeSpan.Zero) }
        );
    }

    private static void CreateManifest41(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0041",
            Status = ManifestStatus.InTransit,
            SentAt = new DateTimeOffset(2026, 6, 30, 6, 0, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-41001", Patient = "G. Wilson", Site = "Blood", Provider = "Dr. Jones", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-41002", Patient = "H. Taylor", Site = "Tissue", Provider = "Dr. Jones", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-41003", Patient = "I. Thomas", Site = "Urine", Provider = "Dr. Jones", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-41004", Patient = "J. White", Site = "Swab", Provider = "Dr. Jones", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-41005", Patient = "K. Harris", Site = "Blood", Provider = "Dr. Jones", Status = SpecimenStatus.Pending }
        );
    }

    private static void CreateManifest40(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0040",
            Status = ManifestStatus.Closed,
            SentAt = new DateTimeOffset(2026, 6, 28, 10, 0, 0, TimeSpan.Zero),
            ClosedAt = new DateTimeOffset(2026, 6, 28, 16, 30, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40001", Patient = "L. Davis", Site = "Blood", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 15, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40002", Patient = "M. Clark", Site = "Urine", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 16, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40003", Patient = "N. Lewis", Site = "Tissue", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 17, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40004", Patient = "O. Hall", Site = "Swab", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 18, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40005", Patient = "P. Allen", Site = "Blood", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 19, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-40006", Patient = "Q. Young", Site = "Urine", Provider = "Dr. Smith", Status = SpecimenStatus.Received, ReceivedBy = "tech1", ReceivedAt = new DateTimeOffset(2026, 6, 28, 10, 20, 0, TimeSpan.Zero) }
        );
    }

    private static void CreateManifest39(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        var flaggedId = Guid.NewGuid();

        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0039",
            Status = ManifestStatus.ClosedWithDiscrepancy,
            SentAt = new DateTimeOffset(2026, 6, 27, 11, 0, 0, TimeSpan.Zero),
            ClosedAt = new DateTimeOffset(2026, 6, 27, 15, 45, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-39001", Patient = "R. King", Site = "Blood", Provider = "Dr. Patel", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 27, 11, 30, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-39002", Patient = "S. Wright", Site = "Urine", Provider = "Dr. Patel", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 27, 11, 31, 0, TimeSpan.Zero) },
            new Specimen { Id = flaggedId, ManifestId = manifestId, Code = "SP-39003", Patient = "T. Scott", Site = "Tissue", Provider = "Dr. Patel", Status = SpecimenStatus.Flagged },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-39004", Patient = "U. Green", Site = "Swab", Provider = "Dr. Patel", Status = SpecimenStatus.Received, ReceivedBy = "tech2", ReceivedAt = new DateTimeOffset(2026, 6, 27, 11, 33, 0, TimeSpan.Zero) }
        );

        db.Discrepancies.Add(new Discrepancy
        {
            Id = Guid.NewGuid(),
            ManifestId = manifestId,
            SpecimenId = flaggedId,
            Type = DiscrepancyType.Missing,
            Note = "Specimen SP-39003 flagged as missing — bottle broken in transit",
            Status = DiscrepancyStatus.Open,
            CreatedAt = new DateTimeOffset(2026, 6, 27, 12, 0, 0, TimeSpan.Zero)
        });
    }

    private static void CreateManifest43(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0043",
            Status = ManifestStatus.Open,
            SentAt = new DateTimeOffset(2026, 6, 29, 9, 0, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-43001", Patient = "V. Adams", Site = "Blood", Provider = "Dr. Moore", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-43002", Patient = "W. Baker", Site = "Urine", Provider = "Dr. Moore", Status = SpecimenStatus.Received, ReceivedBy = "tech3", ReceivedAt = new DateTimeOffset(2026, 6, 29, 10, 0, 0, TimeSpan.Zero) },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-43003", Patient = "X. Cooper", Site = "Tissue", Provider = "Dr. Moore", Status = SpecimenStatus.Pending }
        );
    }

    private static void CreateManifest44(AppDbContext db, Guid manifestId, Guid labId, Guid clinicId)
    {
        db.Manifests.Add(new Manifest
        {
            Id = manifestId,
            LabId = labId,
            ClinicId = clinicId,
            Code = "MF-2026-0044",
            Status = ManifestStatus.InTransit,
            SentAt = new DateTimeOffset(2026, 6, 30, 5, 30, 0, TimeSpan.Zero)
        });

        db.Specimens.AddRange(
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-44001", Patient = "Y. Turner", Site = "Blood", Provider = "Dr. Reed", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-44002", Patient = "Z. Phillips", Site = "Swab", Provider = "Dr. Reed", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-44003", Patient = "A. Evans", Site = "Urine", Provider = "Dr. Reed", Status = SpecimenStatus.Pending },
            new Specimen { Id = Guid.NewGuid(), ManifestId = manifestId, Code = "SP-44004", Patient = "B. Foster", Site = "Tissue", Provider = "Dr. Reed", Status = SpecimenStatus.Pending }
        );
    }
}
