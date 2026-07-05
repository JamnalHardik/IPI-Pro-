using Microsoft.EntityFrameworkCore;
using SpecimenCheckin.Api.Data;
using SpecimenCheckin.Api.Middleware;
using SpecimenCheckin.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ManifestService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Startup");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (db.Database.IsRelational())
            db.Database.Migrate();
        SeedData.Initialize(db);
        logger.LogInformation("Database migrated and seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database not available — running in degraded mode. Start PostgreSQL and restart the API.");
    }
}

app.UseCors("AllowSpecificOrigin");
app.UseTenantMiddleware();
app.MapControllers();

app.Run();

public partial class Program { }
