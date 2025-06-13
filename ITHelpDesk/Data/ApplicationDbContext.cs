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
        public DbSet<TechnicianPort> TechnicianPorts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<TechnicianGroup> TechnicianGroups { get; set; }
        public DbSet<SeniorTechnician> SeniorTechnicians { get; set; }
        public DbSet<Notification> Notifications { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure ApplicationUser references Port, not UserPorts
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(a => a.Port)
                .WithMany()
                .HasForeignKey(a => a.PortId)
                .OnDelete(DeleteBehavior.Restrict);  // Avoid cascade delete issues

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
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PortId)
                .OnDelete(DeleteBehavior.Restrict); // Or your preferred behavior


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

            modelBuilder.Entity<Technician>()
                .HasMany(t => t.Tickets)
                .WithOne(t => t.AssignedTechnician)
                .HasForeignKey(t => t.AssignedTechnicianId);

            modelBuilder.Entity<Subcategory>()
      .HasMany(s => s.TechnicianGroups)
      .WithMany(t => t.Subcategories);


            modelBuilder.Entity<Ticket>()
     .HasMany(t => t.TechnicianGroups)
     .WithMany(g => g.Tickets);
        }
    }
}
