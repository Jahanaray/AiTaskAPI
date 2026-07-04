using AiTaskApi.Data;
using AiTaskApi.Services;
using AiTaskApi.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Controllers
// ----------------------
builder.Services.AddControllers();

// ----------------------
// Database (PostgreSQL)
// ----------------------
var conn =
    Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn)
);

// ----------------------
// Services
// ----------------------
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<AiService>();
builder.Services.AddSingleton<RabbitMqService>();


//// ----------------------
//// Worker (IMPORTANT FIX)
//// ----------------------
//builder.Services.AddHostedService<Worker>();

// ----------------------
// HTTP Client (AI service)
// ----------------------
builder.Services.AddHttpClient<AiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});


// ----------------------
// CORS (Production-ready)
// ----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173", // Vite dev
                "http://localhost:3000"  // optional React dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// ----------------------
// JWT Authentication
// ----------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException(
            "JWT signing key is not configured. Set the JWT_SECRET environment variable or Jwt:Secret configuration value.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ----------------------
// Swagger
// ----------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ----------------------
// Middleware pipeline
// ----------------------
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ----------------------
// AUTO MIGRATION
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

app.Run();

public partial class Program { }
