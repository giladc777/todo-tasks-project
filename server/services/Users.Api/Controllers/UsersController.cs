using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Api.Contracts;
using Users.Api.Data;
using Users.Api.Models;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "users.admin")]
public sealed class UsersController(UserDb db, IPasswordHasher<AppUser> passwordHasher) : ControllerBase
{
    /// <summary>Lists users.</summary>
    [HttpGet(Name = "ListUsers")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking()
            .Select(user => new UserResponse(
                user.Id,
                user.Username,
                user.Role,
                user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>Creates user and assigns permissions.</summary>
    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (username.Length < 3 || request.Password.Length < 12)
        {
            return JsonString("Username must contain at least 3 characters and password at least 12 characters.",
                StatusCodes.Status400BadRequest);
        }

        if (request.Role is not ("User" or "Admin"))
        {
            return JsonString("Role must be User or Admin.", StatusCodes.Status400BadRequest);
        }

        if (await db.Users.AnyAsync(
                user => user.Username.ToLower() == username.ToLower(),
                cancellationToken))
        {
            return JsonString("Username already exists.", StatusCodes.Status409Conflict);
        }

        var user = new AppUser
        {
            Username = username,
            Role = request.Role,
            Permissions = string.Join(',', request.Permissions.Distinct(StringComparer.Ordinal))
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Role,
            user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries));
        return Created($"/api/users/{user.Id}", response);
    }

    /// <summary>Updates user permissions.</summary>
    [HttpPut("{id:guid}/permissions", Name = "UpdatePermissions")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdatePermissions(
        Guid id,
        PermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Permissions = string.Join(',', request.Permissions.Distinct(StringComparer.Ordinal));
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new UserResponse(
            user.Id,
            user.Username,
            user.Role,
            user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)));
    }

    private JsonResult JsonString(string value, int statusCode) =>
        new(value)
        {
            ContentType = "application/json",
            StatusCode = statusCode
        };
}