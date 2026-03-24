using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

public static class MapEndpointsHelper
{
    public static void MapEndpoints(this WebApplication app, string realpath)
    {
        app.MapPost("/api/storage/{*path}", async (IFormFile file, string? path, FileStorageContext context) =>
        {
            string? normalizedPath = NormalizePath(path);
            var directory = $"{realpath}/{normalizedPath}";

            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fullFilePath = Path.Combine(directory, file.FileName);

            if(!File.Exists(fullFilePath))
            {
                using var filestream = File.Create(fullFilePath);
                await file.CopyToAsync(filestream);

                string reference = $"/storage/{(string.IsNullOrEmpty(normalizedPath) ? "" : normalizedPath + "/")}{file.FileName}";

                StoredFile storedFile = new StoredFile
                {
                    Reference = reference,
                    Type = file.ContentType,
                    ByteSize = file.Length,
                    LastUpdated = DateTime.UtcNow,
                    Name = file.FileName
                };

                context.Files.Add(storedFile);
                await context.SaveChangesAsync();

                return Results.Ok();
            }
            return Results.Conflict();

        }).DisableAntiforgery();

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
    }

    public static string NormalizePath(string? path)
    {
        if(string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.TrimStart('/').TrimEnd('/');
    }

}