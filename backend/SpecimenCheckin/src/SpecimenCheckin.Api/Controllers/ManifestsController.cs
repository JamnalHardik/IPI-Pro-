using Microsoft.AspNetCore.Mvc;
using SpecimenCheckin.Api.DTOs;
using SpecimenCheckin.Api.Middleware;
using SpecimenCheckin.Api.Services;

namespace SpecimenCheckin.Api.Controllers;

[ApiController]
[Route("api/manifests")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 422)]
public class ManifestsController : ControllerBase
{
    private readonly ManifestService _service;
    private readonly ILogger<ManifestsController> _logger;

    public ManifestsController(ManifestService service, ILogger<ManifestsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManifestListItemDto>>> GetManifests([FromQuery] string? status = null)
    {
        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var manifests = await _service.GetManifestsForLabAsync(tenantId, status);
        return Ok(manifests);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ManifestDetailDto>> GetManifest(Guid id)
    {
        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var manifest = await _service.GetManifestDetailAsync(tenantId, id);

        if (manifest == null)
            return NotFound(CreateProblem(404, "Manifest not found",
                $"No manifest with id '{id}' found for this tenant."));

        return Ok(manifest);
    }

    [HttpPost("{id:guid}/specimens/{sid:guid}/receive")]
    public async Task<ActionResult<ManifestDetailDto>> ReceiveSpecimen(Guid id, Guid sid)
    {
        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var userId = HttpContext.GetUserId();

        try
        {
            var result = await _service.ReceiveSpecimenAsync(tenantId, id, sid, userId);
            if (result == null)
                return NotFound(CreateProblem(404, "Not found", "Manifest or specimen not found for this tenant."));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(CreateProblem(422, "Invalid operation", ex.Message));
        }
    }

    [HttpPost("{id:guid}/specimens/{sid:guid}/flag")]
    public async Task<ActionResult<ManifestDetailDto>> FlagSpecimen(Guid id, Guid sid)
    {
        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var userId = HttpContext.GetUserId();

        try
        {
            var result = await _service.FlagSpecimenAsync(tenantId, id, sid, userId);
            if (result == null)
                return NotFound(CreateProblem(404, "Not found", "Manifest or specimen not found for this tenant."));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(CreateProblem(422, "Invalid operation", ex.Message));
        }
    }

    [HttpPost("{id:guid}/specimens")]
    public async Task<ActionResult<ManifestDetailDto>> AddOffManifestSpecimen(Guid id, [FromBody] AddOffManifestRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateProblem(400, "Validation error", "Invalid request body."));

        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var userId = HttpContext.GetUserId();

        try
        {
            var result = await _service.AddOffManifestSpecimenAsync(tenantId, id, request, userId);
            if (result == null)
                return NotFound(CreateProblem(404, "Not found", "Manifest not found for this tenant."));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(CreateProblem(422, "Invalid operation", ex.Message));
        }
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<ManifestDetailDto>> CloseManifest(Guid id)
    {
        var tenantId = Guid.Parse(HttpContext.GetTenantId());
        var userId = HttpContext.GetUserId();

        try
        {
            var result = await _service.CloseManifestAsync(tenantId, id, userId);
            if (result == null)
                return NotFound(CreateProblem(404, "Not found", "Manifest not found for this tenant."));

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(CreateProblem(422, "Invalid operation", ex.Message));
        }
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail) => new()
    {
        Type = $"https://errors.specimencheckin/{status}",
        Title = title,
        Status = status,
        Detail = detail,
        Instance = null
    };
}
