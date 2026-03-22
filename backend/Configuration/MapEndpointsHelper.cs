public static class MapEndpointsHelper
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/api/storage/{*path}", async (IFormFile file, string? path, FileStorageContext context) =>
        {
            string? normalizedPath = NormalizePath(path);
            var directory = $"/app/storage/{normalizedPath}";

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
    }

    public static string NormalizePath(string? path)
    {
        if(string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.TrimStart('/').TrimEnd('/');
    }

}