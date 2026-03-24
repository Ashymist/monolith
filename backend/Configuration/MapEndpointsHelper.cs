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

                string reference = $"/storage/{normalizedPath}/{file.FileName}";

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
    }

    public static string NormalizePath(string? path)
    {
        if(string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.TrimStart('/').TrimEnd('/');
    }

}