using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecimenCheckin.Api.Data;

namespace SpecimenCheckin.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TenantsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<object>>> GetTenants()
    {
        var tenants = await _db.Labs
            .Where(l => l.IsActive)
            .Select(l => new
            {
                id = l.Id.ToString(),
                name = l.Name
            })
            .ToListAsync();

        return Ok(tenants);
    }
}
