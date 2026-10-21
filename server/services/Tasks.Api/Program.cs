using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Tasks.Api.Data;
using Tasks.Api.Infrastructure.Messaging;
using Tasks.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-signing-key-change-this-1234567890";

if (jwtKey.Length < 32 ||
    (builder.Environment.IsProduction() && jwtKey == "dev-only-signing-key-change-this-1234567890"))
{
    throw new InvalidOperationException(
        "Configure Jwt:Key with a unique secret of at least 32 characters before starting this service.");
}

// Register application services and infrastructure.
builder.Services.AddDbContext<TaskDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Tasks") ?? "Data Source=tasks.db"));
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"] ?? "localhost:6379,abortConnect=false,connectTimeout=1000,syncTimeout=1500,asyncTimeout=1500"));
builder.Services.AddSingleton<IEventPublisher, RabbitEventPublisher>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddHttpContextAccessor();
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
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("tasks.read", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "tasks.read", "tasks.write"));
    options.AddPolicy("tasks.write", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "tasks.write"));
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

await TaskDbInitializer.InitializeAsync(app.Services);

// Configure middleware pipeline and map controllers.
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/", () => TypedResults.Redirect("/swagger/index.html"));

app.Run();
