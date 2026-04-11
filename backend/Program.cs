using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BC = BCrypt.Net.BCrypt;

var builder = WebApplication.CreateBuilder(args);

string realpath;

if(builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<FileStorageContext>(options =>
        options.UseNpgsql("Host=localhost;Port=6450;Database=monolitdb;Username=admin;Password=secret;"));
    realpath = Path.Combine(Environment.GetEnvironmentVariable("HOME"),"monolith/storage");
}
else
{
    builder.Services.AddDbContext<FileStorageContext>(options =>
        options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString_monolith")));
    realpath = "/app/storage";
}

builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>,PostConfigureCookieOptions>();

builder.Services.AddSingleton<ITicketStore, SessionStore>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        
    });



var CookiePolicyOptions = new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Strict,
    };


var app = builder.Build();

app.MapEndpoints(realpath);

app.UseAuthentication();

app.UseCookiePolicy(CookiePolicyOptions);

app.MigrateDb();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileStorageContext>();
    if (!context.Settings.Any())
    {
        context.Settings.Add(new Settings{ 
            Id = 1, 
            VaultName = "vault", 
            VaultPasswordHashed = BC.HashPassword(Environment.GetEnvironmentVariable("DEFAULT_PASSWORD"))
            });
        context.SaveChanges();
    }
}

app.Run();
