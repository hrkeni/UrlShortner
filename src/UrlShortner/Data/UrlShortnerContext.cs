using Microsoft.EntityFrameworkCore;
using UrlShortner.Models;

namespace UrlShortner.Data
{
    public class UrlShortnerContext : DbContext
    {
        public UrlShortnerContext(DbContextOptions<UrlShortnerContext> options)
            : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var shortenedUrlEntity = modelBuilder
                .Entity<ShortenedUrl>()
                .ToTable("shortened_urls");
            
            shortenedUrlEntity.HasKey(s => s.Id);

            shortenedUrlEntity.HasIndex(s => s.Slug).IsUnique();
        }
    }
}
