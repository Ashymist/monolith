using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FileStorageContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString_monolith")));


var app = builder.Build();


app.MapEndpoints();

app.MigrateDb();

app.Run();
