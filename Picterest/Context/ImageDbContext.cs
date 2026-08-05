using Microsoft.EntityFrameworkCore;
using Picterest.DbModels;

namespace Picterest.Context
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
        { }

        public DbSet<Image> Images => Set<Image>();
        public DbSet<Picterest.DbModels.File> Files => Set<Picterest.DbModels.File>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Image Entity Configuration
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(100)");
                entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description").HasColumnType("varchar(500)");
                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasColumnType("boolean").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()").HasColumnName("updated_at");
                entity.Property(e => e.RestoredAt).HasColumnName("restored_at");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
                entity.HasOne(f=>f.Files)
                      .WithOne(i=>i.Image)
                      .HasForeignKey<Image>(i=>i.FileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //File Entity Configuration
            modelBuilder.Entity<Picterest.DbModels.File>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasColumnName("file_name").HasColumnType("varchar(100)");
                entity.Property(e => e.ObjectKey).IsRequired().HasColumnName("object_key").HasColumnType("varchar(500)");
                entity.Property(e => e.Bucket).IsRequired().HasColumnName("bucket").HasColumnType("varchar(100)");
                entity.Property(e => e.ContentType).IsRequired().HasColumnName("content_type").HasColumnType("varchar(100)");
                entity.Property(e => e.Size).IsRequired().HasColumnName("size");
                entity.Property(e => e.cleanupStatus).IsRequired().HasColumnName("clean_up_status");
                entity.Property(e => e.DeleteAttempts).HasColumnName("delete_attempts");
                entity.Property(e => e.LastDeleteError).HasColumnName("last_delete_error");
                entity.Property(e => e.StorageDeletedAt).HasColumnName("storage_deleted_at");
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("NOW()").HasColumnName("uploaded_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()").HasColumnName("updated_at");
                entity.HasIndex(e=>e.ObjectKey).IsUnique();
                entity.HasIndex(e => e.Bucket);
            });

            //User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(100)");
                entity.Property(e => e.Email).IsRequired().HasColumnName("email").HasColumnType("varchar(100)");
                entity.Property(e => e.AvatarUrl).IsRequired().HasColumnName("avatar_url").HasColumnType("varchar(500)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()").HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()").HasColumnName("updated_at");
                entity.Property(e => e.GithubId).IsRequired().HasColumnName("github_id");
                entity.Property(e => e.GithubUserName).IsRequired().HasColumnName("github_user_name").HasColumnType("varchar(100)");
                entity.HasIndex(e => e.GithubId).IsUnique();
                entity.HasIndex(e => e.GithubUserName).IsUnique();
            });


        }


    }
}
