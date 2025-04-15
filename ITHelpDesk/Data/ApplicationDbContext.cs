using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<UserPorts> UserPorts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<TechnicianGroup> TechnicianGroups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure ApplicationUser references Port, not UserPorts
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Port)
                .WithMany()
                .HasForeignKey(a => a.PortId)
                .OnDelete(DeleteBehavior.Restrict);  // Avoid cascade delete issues

            modelBuilder.Entity<UserPorts>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPorts>()
                .HasOne(up => up.Port)
                .WithMany()
                .HasForeignKey(up => up.PortId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Subcategory)
                .WithMany()
                .HasForeignKey(t => t.SubcategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Port)
                .WithMany()
                .HasForeignKey(t => t.PortId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Priority)
                .WithMany()
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
