using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Users.Api.Data;
using Users.Api.Infrastructure.Messaging;
using Users.Api.Models;


var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-signing-key-change-this-1234567890";

if (jwtKey.Length < 32 ||
    (builder.Environment.IsProduction() && jwtKey == "dev-only-signing-key-change-this-1234567890"))
{
    throw new InvalidOperationException(
        "Configure Jwt:Key with a unique secret of at least 32 characters before starting this service.");
}

// Register persistence, identity, and hosted services.
builder.Services.AddDbContext<UserDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Users") ?? "Data Source=users.db"));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddHostedService<EventAuditWorker>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "todo-users",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "todo-app",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("users.admin", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin")));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new()
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new()
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

await UserDbInitializer.InitializeAsync(app.Services, app.Configuration, app.Environment);

// Configure middleware pipeline and map controllers.
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
