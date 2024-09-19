using Microsoft.EntityFrameworkCore;
using TvShowsCatalog.Web.Models.CoreModels;

namespace TvShowsCatalog.Web.Data
{
    public class ReviewContext : DbContext
    {
        public ReviewContext(DbContextOptions<ReviewContext> options)
            : base(options)
        {
        }

        public required DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("review");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TvShowUmbracoKey).HasColumnName("tvShowUmbracoKey").HasColumnType("uniqueidentifier");
                entity.Property(e => e.MemberUmbracoKey).HasColumnName("memberUmbracoKey").HasColumnType("uniqueidentifier");
                entity.Property(e => e.Rating).HasColumnName("rating").HasColumnType("int");
                entity.Property(e => e.Comment).HasColumnName("comment").HasColumnType("nvarchar(255)");
                entity.Property(e => e.CreationDate).HasColumnName("creationDate").HasColumnType("datetime");
            });
    }
}