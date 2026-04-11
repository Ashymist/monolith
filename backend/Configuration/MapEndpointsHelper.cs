using System.IO.Compression;
using System.Security.Claims;
using FileTypeChecker;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

public static class MapEndpointsHelper
{
    public static void MapEndpoints(this WebApplication app, string realpath)
    {
        app.MapPost("/api/storage/{*path}", async (IFormFile file, string? path, FileStorageContext context) =>
        {
            string normalizedPath = NormalizePath(path);
            string directory = $"{realpath}/{normalizedPath}";

            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fullFilePath = Path.Combine(directory, file.FileName);

            if(!File.Exists(fullFilePath))
            {
                using var filestream = File.Create(fullFilePath);
                await file.CopyToAsync(filestream);

                // string reference = $"/storage/{(string.IsNullOrEmpty(normalizedPath) ? "" : normalizedPath + "/")}{file.FileName}";

                filestream.Seek(0, SeekOrigin.Begin);

                StoredFile storedFile = new StoredFile
                {
                    FilePath = fullFilePath,
                    Type = FileTypeValidator.GetFileType(filestream).Name,
                    ByteSize = file.Length,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Name = file.FileName
                };

                context.Files.Add(storedFile);
                await context.SaveChangesAsync();

                return Results.Ok();
            }
            return Results.Conflict();

        }).DisableAntiforgery();

        

        app.MapPost("/api/login", async ([FromBody]PasswordDto passwordDto, FileStorageContext context, HttpContext httpContext) =>
        {
            
            string hashedPasswordStored = context.Settings.FirstOrDefault().VaultPasswordHashed;

            if (string.IsNullOrEmpty(passwordDto.Password))
            {
                return Results.Unauthorized();     
            }

            if(BC.Verify(passwordDto.Password, hashedPasswordStored))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties();
                authProperties.Items.Add("IP", httpContext.Connection.RemoteIpAddress.ToString());
                authProperties.Items.Add("User-Agent", httpContext.Request.Headers["User-Agent"]);


                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                return Results.Ok();
            } 
            return Results.Unauthorized();
        });

        /*

        app.MapGet("/api/storage/{*path}", async(string? path, HttpRequest req, FileStorageContext context) =>
        {
            string? searchQuery = req.Query["query"];
            string? normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}/{normalizedPath}";

            if (File.Exists(fullpath))
            {
                if(!string.IsNullOrEmpty(searchQuery)) return Results.BadRequest("Query parameter is not allowed when requesting a file.");
                StoredFile? result = await context.Files.FirstOrDefaultAsync(f => f.Reference == $"/storage/{normalizedPath}");
                if(result == null) return Results.NotFound();
                FileDto fileDto = new FileDto
                {
                    Reference = result.Reference,
                    Type = result.Type,
                    ByteSize = result.ByteSize,
                    LastUpdated = result.LastUpdated,
                    Name = result.Name
                };
                return Results.Ok(fileDto);
            }

            if (Directory.Exists(fullpath))
            {
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    var fileDtos = await context.Files
                    .Where(f => f.Reference.StartsWith($"/storage/{normalizedPath}") && f.Name.Contains(searchQuery))
                    .Select(f => new FileDto
                    {
                        Reference = f.Reference,
                        Type = f.Type,
                        ByteSize = f.ByteSize,
                        LastUpdated = f.LastUpdated,
                        Name = f.Name
                    })
                    .ToListAsync();
                    return Results.Ok(fileDtos);
                }
                else
                {
                    var fileDtos = await context.Files
                    .Where(f => f.Reference.StartsWith($"/storage/{normalizedPath}"))
                    .Select(f => new FileDto
                    {
                        Reference = f.Reference,
                        Type = f.Type,
                        ByteSize = f.ByteSize,
                        LastUpdated = f.LastUpdated,
                        Name = f.Name
                    })
                    .ToListAsync();
                    return Results.Ok(fileDtos);
                }
            }

            return Results.NotFound();
        });

        app.MapGet("/api/download/storage/{*path}", async(string? path, FileStorageContext context, HttpResponse res) =>
        {
            string normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}/{normalizedPath}";


            if(File.Exists(fullpath))
            {
                res.Headers["X-Accel-Redirect"] = $"/internal-storage/{normalizedPath}";
                res.Headers["Content-Disposition"] = $"attachment; filename=\"{Path.GetFileName(fullpath)}\"";
                return Results.Empty;
            } else if (Directory.Exists(fullpath))
            {
                res.Headers["Content-Type"] = "application/zip";
                res.Headers["Content-Disposition"] = $"attachment; filename=\"{Path.GetFileName(fullpath)}.zip\"";

                using var zip = new ZipArchive(res.Body, ZipArchiveMode.Create, leaveOpen:true);

                foreach(var file in Directory.GetFiles(fullpath, "*", SearchOption.AllDirectories))
                {
                    var entryName = Path.GetRelativePath(fullpath, file);
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Fastest);
                    await using var entryStream = entry.Open();
                    await using var fileStream = File.OpenRead(file);
                    await fileStream.CopyToAsync(entryStream);
                }
                return Results.Empty;
            }

            return Results.NotFound();
        });

        app.MapDelete("/api/storage/{*path}", async (string? path, FileStorageContext context) =>
        {
            string normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}/{normalizedPath}";

            if (File.Exists(fullpath))
            {
                
                File.Delete(fullpath);
                context.Remove(context.Files.FirstOrDefault(f => f.Reference.StartsWith($"/storage/{Path.GetDirectoryName(normalizedPath)}") && f.Name == Path.GetFileName(normalizedPath)));
                context.SaveChanges();
                return Results.Ok();
            } else if (Directory.Exists(fullpath))
            {
                
                if (!string.IsNullOrEmpty(normalizedPath)) {
                    context.RemoveRange(context.Files.Where(f=>f.Reference.StartsWith($"/storage/{normalizedPath}/")));
                    Directory.Delete(fullpath, true);
                }
                else {
                    context.RemoveRange(context.Files);
                    var files = Directory.GetFiles(realpath, "*", SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        File.Delete(file);
                    }
                    context.SaveChanges();
                    return Results.Ok();
                }
            }

            return Results.NotFound();
        });

        app.MapPatch("api/storage/{*path}", async (string? path, FileStorageContext context, HttpRequest req) =>
        {
            if(string.IsNullOrEmpty(path)) return Results.BadRequest("Can't change the root folder");
            string normalizedPath = NormalizePath(path);

            string fullpath = $"{realpath}/{normalizedPath}";
            string? newPathQuery = req.Query["newpath"];
            string? newNameQuery = req.Query["newname"];
            string? normalizedNewPathQuery = NormalizePath(newPathQuery);
            string? filename = Path.GetFileName(fullpath);
            string? relativePath = NormalizePath(Path.GetDirectoryName(normalizedPath));
            
            if (File.Exists(fullpath))
            {
                string newPath = $"{realpath}/";
                if (!string.IsNullOrEmpty(newPathQuery)) newPath = Path.Combine(newPath,normalizedNewPathQuery);
                else newPath = Path.Combine(newPath,relativePath);
                if (!string.IsNullOrEmpty(newNameQuery)) newPath = Path.Combine(newPath,newNameQuery);
                else newPath = Path.Combine(newPath,filename);

                if(!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Move(fullpath,newPath);


                string referenceQuery = $"/storage/{normalizedPath}";
                var file = context.Files.FirstOrDefault(f => f.Reference == referenceQuery);
                file.Reference = $"/storage/{(string.IsNullOrEmpty(normalizedNewPathQuery) ? (string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath + "/") : normalizedNewPathQuery + "/")}{(string.IsNullOrEmpty(newNameQuery) ? filename : newNameQuery)}";
                file.Name = string.IsNullOrEmpty(newNameQuery) ? filename : newNameQuery;
                file.LastUpdated = DateTime.UtcNow;
                context.SaveChanges();
                return Results.Ok();
            } else if (Directory.Exists(fullpath))
            {
                string? fodlerName = Path.GetFileName(fullpath);
                string newPath = $"{realpath}/";
                if (!string.IsNullOrEmpty(newPathQuery)) newPath = Path.Combine(newPath,normalizedNewPathQuery);
                else newPath = Path.Combine(newPath,relativePath);

                if (!string.IsNullOrEmpty(newNameQuery)) newPath = Path.Combine(newPath,newNameQuery);
                else newPath = Path.Combine(newPath,filename);

                if(!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                Directory.Move(fullpath,newPath);

                var files = context.Files.Where(f => f.Reference.StartsWith($"/storage/{normalizedPath}"));
                foreach(var file in files)
                {
                    file.Reference = $"/storage/{(string.IsNullOrEmpty(normalizedNewPathQuery) ? 
                    (string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath + "/") 
                    : normalizedNewPathQuery + "/")}{(string.IsNullOrEmpty(newNameQuery) ? filename : newNameQuery)}/{file.Name}";
                }
                context.SaveChanges();

                return Results.Ok();

            }
            return Results.NotFound();
        });
        */
    }

    public static string NormalizePath(string? path)
    {
        if(string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.TrimStart('/').TrimEnd('/');
    }

}