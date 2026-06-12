using AiTaskApi.Data;
using AiTaskApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// DB
var conn =
    Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? "Host=localhost;Port=5432;Database=taskdb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(conn));

// services
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<AiService>();

builder.Services.AddHttpClient<AiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

// worker
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    foreach (var job in db.AgentJobs.Where(x => x.Status == "Running"))
    {
        job.Status = "Pending";
        job.NextAttemptAt = DateTime.UtcNow;
        job.LastError = "Recovered after worker restart.";
    }

    db.SaveChanges();
}

app.Run();
