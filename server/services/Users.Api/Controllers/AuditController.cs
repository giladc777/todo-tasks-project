using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Api.Contracts;
using Users.Api.Data;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Policy = "users.admin")]
public sealed class AuditController(UserDb db) : ControllerBase
{
    /// <summary>Views recent service event audit.</summary>
    [HttpGet(Name = "GetAudit")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditEventResponse>>> Get(CancellationToken cancellationToken)
    {
        var events = await db.AuditEvents.AsNoTracking()
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .Take(100)
            .Select(auditEvent => new AuditEventResponse(
                auditEvent.Id,
                auditEvent.Type,
                auditEvent.Message,
                auditEvent.OccurredAt))
            .ToListAsync(cancellationToken);
        return Ok(events);
    }
}