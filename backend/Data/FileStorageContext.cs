using Microsoft.EntityFrameworkCore;

public class FileStorageContext : DbContext
{
    public FileStorageContext(DbContextOptions<FileStorageContext> options)
        : base(options)
    {
    }

    public DbSet<StoredFile> Files { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<ShareLink> ShareLinks { get; set; }
    public DbSet<Session> Sessions { get; set; }
}