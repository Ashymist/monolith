public static class MapEndpointsHelper
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/api/storage/{*path}", async (IFormFile file, string? path) =>
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