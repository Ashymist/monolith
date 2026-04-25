using System.IO.Compression;
using System.Security.Claims;
using FileTypeChecker;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
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
            string directory = $"{realpath}{(string.IsNullOrEmpty(normalizedPath) ? string.Empty : "/" + normalizedPath)}";

            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fullFilePath = Path.Combine(directory, file.FileName);

            if(!File.Exists(fullFilePath))
            {
                using var filestream = File.Create(fullFilePath);
                await file.CopyToAsync(filestream);
                filestream.Close();

                using var fileTypeStream = File.OpenRead(fullFilePath);

                string type;
                try
                {
                    type = FileTypeValidator.GetFileType(fileTypeStream).Name;
                }
                catch
                {
                    try
                    {
                        type = Path.GetExtension(fullFilePath);
                    }
                    catch
                    {
                        type = "File";
                    }
                    
                }
                
                

                StoredFile storedFile = new StoredFile
                {
                    FilePath = Path.Combine(normalizedPath, file.FileName),
                    Type = type,
                    ByteSize = file.Length,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Name = file.FileName
                };

                context.Files.Add(storedFile);
                await context.SaveChangesAsync();

                return Results.Ok();
            }
            return Results.BadRequest("File already exists at given filepath.");

        }).DisableAntiforgery().RequireAuthorization("Administrator");

        

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

        app.MapPost("/api/logout", async(HttpContext context) =>
        {
            await context.SignOutAsync();
        }).RequireAuthorization("Administrator");

        app.MapGet("/api/storage/{*path}", async(string? path, HttpRequest req, FileStorageContext context) =>
        {
            string? searchQuery = req.Query["query"];
            string? normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}{(string.IsNullOrEmpty(normalizedPath) ? string.Empty : "/" + normalizedPath)}";

            if (File.Exists(fullpath))
            {
                if(!string.IsNullOrEmpty(searchQuery)) return Results.BadRequest("Query parameter is not allowed when requesting a file.");
                StoredFile? result = await context.Files.FirstOrDefaultAsync(f => f.FilePath == normalizedPath);
                if(result == null) return Results.NotFound(); // TODO : orphan file alert
                FileDto fileDto = new FileDto
                {
                    Reference = $"/api/storage/{result.FilePath}",
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
                    .Where(f => f.FilePath.StartsWith(normalizedPath) && f.Name.Contains(searchQuery))
                    .Select(f => new FileDto
                    {
                        Reference = $"/api/storage/{f.FilePath}",
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
                    .Where(f => f.FilePath.StartsWith(normalizedPath))
                    .Select(f => new FileDto
                    {
                        Reference = $"/api/storage/{f.FilePath}",
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
        }).RequireAuthorization("Administrator");

        

        app.MapGet("/api/download/storage/{*path}", async(string? path, FileStorageContext context, HttpResponse res, HttpContext httpContext) =>
        {
            var syncIOFeature = httpContext.Features.Get<IHttpBodyControlFeature>();
            syncIOFeature.AllowSynchronousIO = true;

            string normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}{(string.IsNullOrEmpty(normalizedPath) ? string.Empty : "/" + normalizedPath)}";


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
        }).RequireAuthorization("Administrator");

        
        app.MapDelete("/api/storage/{*path}", async (string? path, FileStorageContext context) =>
        {
            string normalizedPath = NormalizePath(path);
            string fullpath = $"{realpath}{(string.IsNullOrEmpty(normalizedPath) ? string.Empty : "/" + normalizedPath)}";

            if (File.Exists(fullpath))
            {
                
                File.Delete(fullpath);
                context.Remove(context.Files.FirstOrDefault(f => f.FilePath.StartsWith(normalizedPath) && f.Name == Path.GetFileName(normalizedPath)));
                context.SaveChanges();
                return Results.Ok();
            } else if (Directory.Exists(fullpath))
            {
                
                if (!string.IsNullOrEmpty(normalizedPath)) {
                    context.RemoveRange(context.Files.Where(f=>f.FilePath.StartsWith(normalizedPath)));
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
        }).RequireAuthorization("Administrator");

        
        app.MapPatch("api/storage/{*path}", async (string? path, FileStorageContext context, HttpRequest req) =>
        {
            if(string.IsNullOrEmpty(path)) return Results.BadRequest("Can't change the root folder");
            string normalizedPath = NormalizePath(path);

            string fullpath = $"{realpath}{(string.IsNullOrEmpty(normalizedPath) ? string.Empty : "/" + normalizedPath)}";
            string? newPathQuery = req.Query["newpath"];
            string? newNameQuery = req.Query["newname"];
            if(string.IsNullOrEmpty(newNameQuery) && string.IsNullOrEmpty(newPathQuery)) return Results.BadRequest("A newname or newpath query must be present in the request");
            string? normalizedNewPathQuery = NormalizePath(newPathQuery);
            string? filename = Path.GetFileName(fullpath);
            string? relativePath = NormalizePath(Path.GetDirectoryName(normalizedPath));
            
            if (File.Exists(fullpath))
            {
                string newPath = string.Empty;
                if (!string.IsNullOrEmpty(newPathQuery)) newPath = Path.Combine(newPath,normalizedNewPathQuery);
                else newPath = Path.Combine(newPath,relativePath);
                if (!string.IsNullOrEmpty(newNameQuery)) newPath = Path.Combine(newPath,newNameQuery);
                else newPath = Path.Combine(newPath,filename);
                if (!string.IsNullOrEmpty(Path.GetDirectoryName(newPath)))
                {
                    if(!Directory.Exists(Path.GetDirectoryName(newPath))) Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                    File.Move(fullpath,newPath); 
                } else
                {
                    File.Move(fullpath,Path.Combine(realpath, newNameQuery));
                }
                


                //string referenceQuery = $"/storage/{normalizedPath}";
                var file = context.Files.FirstOrDefault(f => f.FilePath == normalizedPath);
                if(file == null) return Results.NotFound();
                file.FilePath = $"{(string.IsNullOrEmpty(normalizedNewPathQuery) ? (string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath + "/") : normalizedNewPathQuery + "/")}{(string.IsNullOrEmpty(newNameQuery) ? filename : newNameQuery)}";
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

                var files = context.Files.Where(f => f.FilePath.StartsWith(normalizedPath));
                foreach(var file in files)
                {
                    file.FilePath= $"{(string.IsNullOrEmpty(normalizedNewPathQuery) ? 
                    (string.IsNullOrEmpty(relativePath) ? string.Empty : relativePath + "/") 
                    : normalizedNewPathQuery + "/")}{(string.IsNullOrEmpty(newNameQuery) ? filename : newNameQuery)}/{file.Name}";
                }
                context.SaveChanges();

                return Results.Ok();

            }
            return Results.NotFound();
        }).RequireAuthorization("Administrator");
        
    }

    public static string NormalizePath(string? path)
    {
        if(string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.TrimStart('/').TrimEnd('/');
    }

}