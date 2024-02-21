using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistance
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DataContext()
        {
            
        }


        public DbSet<TaskEntity> TaskEntities { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Define relationships and configurations here

            builder.Entity<TaskEntity>().HasKey(x => x.TaskId);
            builder.Entity<TaskCategory>().HasKey(x => x.CategoryId);
            builder.Entity<Comment>().HasKey(x => x.CommentId);
            builder.Entity<BaseEntity>().HasNoKey();

            // Relationship between BaseEntity and AppUser
            builder.Entity<BaseEntity>()
                .HasOne(b => b.CreatedByAppUser)
                .WithMany()
                .HasForeignKey(b => b.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BaseEntity>()
                .HasOne(b => b.UpdatedByAppUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BaseEntity>()
                .HasOne(b => b.DeletedByAppUser)
                .WithMany()
                .HasForeignKey(b => b.DeletedById)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship between TaskEntity and TaskCategory
            builder.Entity<TaskEntity>()
                .HasOne(t => t.TaskCategory)
                .WithMany(tc => tc.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship between TaskEntity and AppUser (AssignedByAppUser)
            builder.Entity<TaskEntity>()
                .HasOne(t => t.AssignedByAppUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedById)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship between Comment and TaskEntity
            builder.Entity<Comment>()
                .HasOne(c => c.CommentedTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship between Comment and AppUser (CommentedAppUser)
            builder.Entity<Comment>()
                .HasOne(c => c.CommentedAppUser)
                .WithMany()
                .HasForeignKey(c => c.CommentedById)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasOne(x => x.AppUser)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
