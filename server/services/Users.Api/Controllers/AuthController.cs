using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Users.Api.Contracts;
using Users.Api.Data;
using Users.Api.Models;

namespace Users.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserDb db,
    IPasswordHasher<AppUser> passwordHasher,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>Authenticates user and issues a JWT.</summary>
    [HttpPost("login", Name = "Login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(
            item => item.Username == request.Username,
            cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password) == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Rejected login for {Username}", request.Username);
            return Unauthorized();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };
        claims.AddRange(user.Permissions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? "dev-only-signing-key-change-this-1234567890"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuer = configuration["Jwt:Issuer"] ?? "todo-users";
        var audience = configuration["Jwt:Audience"] ?? "todo-app";
        var expires = DateTime.UtcNow.AddMinutes(30);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        logger.LogInformation("User {Username} logged in", user.Username);
        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Username,
            user.Role,
            user.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries),
            new DateTimeOffset(expires, TimeSpan.Zero)));
    }
}