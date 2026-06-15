using Microsoft.EntityFrameworkCore;
using DemoApp.Web.Models;

namespace DemoApp.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<Menu>().ToTable("Menus");

            // Conversión de enums a string
            /*modelBuilder.Entity<Candidate>()
                .Property(c => c.Status)
                .HasConversion<string>();
            */

            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
                {
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                }

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin",
                    IsActive = true,
                    InsUser = "system",
                    InsDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdUser = null,
                    UpdDate = null
                }
            );

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    UserId = 1,
                    UserName = "demo",
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "demo@mrsolucionesint.com",
                    RoleId = 1,
                    IsAdmin = true,
                    IsActive = true,
                    InsUser = "system",
                    InsDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdUser = null,
                    UpdDate = null,
                    Role = null!
                }
            );
        }
    }
}
