using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skins.Api.Contracts;
using Skins.Api.Services;

namespace Skins.Api.Controllers;

[ApiController]
[Route("api/skins")]
public sealed class SkinController(ISkinService skinService) : ControllerBase
{
    /// <summary>Lists available skins.</summary>
    [HttpGet(Name = "ListSkins")]
    [Authorize(Policy = "skins.read")]
    [ProducesResponseType(typeof(IReadOnlyList<SkinResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SkinResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await skinService.ListAsync(cancellationToken));

    /// <summary>Gets the active skin.</summary>
    [HttpGet("active", Name = "GetActiveSkin")]
    [Authorize(Policy = "skins.read")]
    [ProducesResponseType(typeof(SkinResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SkinResponse>> GetActive(CancellationToken cancellationToken) =>
        Ok(await skinService.ActiveAsync(cancellationToken));

    /// <summary>Creates a skin.</summary>
    [HttpPost(Name = "CreateSkin")]
    [Authorize(Policy = "skins.write")]
    [ProducesResponseType(typeof(SkinResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<SkinResponse>> Create(
        SkinRequest request,
        CancellationToken cancellationToken)
    {
        var skin = await skinService.SaveAsync(null, request, cancellationToken);
        return Created($"/api/skins/{skin!.Id}", skin);
    }

    /// <summary>Updates a skin.</summary>
    [HttpPut("{id:guid}", Name = "UpdateSkin")]
    [Authorize(Policy = "skins.write")]
    [ProducesResponseType(typeof(SkinResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkinResponse>> Update(
        Guid id,
        SkinRequest request,
        CancellationToken cancellationToken)
    {
        var skin = await skinService.SaveAsync(id, request, cancellationToken);
        return skin is null ? NotFound() : Ok(skin);
    }

    /// <summary>Activates a skin.</summary>
    [HttpPut("{id:guid}/activate", Name = "ActivateSkin")]
    [Authorize(Policy = "skins.write")]
    [ProducesResponseType(typeof(SkinResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkinResponse>> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var skin = await skinService.ActivateAsync(id, cancellationToken);
        return skin is null ? NotFound() : Ok(skin);
    }
}