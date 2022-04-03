using Microsoft.EntityFrameworkCore;

namespace FileBox.Models
{
    public class FileContext : DbContext
    {
        public FileContext(DbContextOptions<FileContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<FileItem> FileItems { get; set; } = null!;

        
    }
}

