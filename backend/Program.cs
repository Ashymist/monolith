using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string realpath;

if(builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<FileStorageContext>(options =>
        options.UseNpgsql("Host=localhost;Port=5450;Database=monolitdb;Username=admin;Password=secret;"));
    realpath = Path.Combine(Environment.GetEnvironmentVariable("HOME"),"monolith/storage");
}
else
{
    builder.Services.AddDbContext<FileStorageContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString_monolith")));
        realpath = "/app/storage";
}

var app = builder.Build();


app.MapEndpoints(realpath);

app.MigrateDb();

app.Run();
