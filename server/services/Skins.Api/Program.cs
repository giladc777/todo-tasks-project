using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Skins.Api.Data;
using Skins.Api.Infrastructure.Messaging;
using Skins.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-signing-key-change-this-1234567890";

if (jwtKey.Length < 32 ||
    (builder.Environment.IsProduction() && jwtKey == "dev-only-signing-key-change-this-1234567890"))
{
    throw new InvalidOperationException(
        "Configure Jwt:Key with a unique secret of at least 32 characters before starting this service.");
}

// Register application services and middleware dependencies.
builder.Services.AddDbContext<SkinDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Skins") ?? "Data Source=skins.db"));
builder.Services.AddSingleton<ISkinEvents, SkinEvents>();
builder.Services.AddScoped<ISkinService, SkinService>();
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
{
    options.AddPolicy("skins.read", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "skins.read", "skins.write"));
    options.AddPolicy("skins.write", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "skins.write"));
});
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

await SkinDbInitializer.InitializeAsync(app.Services);

// Configure request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
