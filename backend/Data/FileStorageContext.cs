using Microsoft.EntityFrameworkCore;

public class FileStorageContext : DbContext
{
    public FileStorageContext(DbContextOptions<FileStorageContext> options)
        : base(options)
    {
    }

    public DbSet<StoredFile> Files { get; set; }
}