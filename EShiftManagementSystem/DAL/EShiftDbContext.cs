using Microsoft.EntityFrameworkCore;
using EShiftManagementSystem.Models;


namespace EShiftManagementSystem.DAL
{
    public class EShiftDbContext : DbContext
    {
        public EShiftDbContext(DbContextOptions<EShiftDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Load> Loads { get; set; }
        public DbSet<TransportUnit> TransportUnits { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer configuration
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Job configuration
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Customer)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CustomerId);

            // Load configuration
            modelBuilder.Entity<Load>()
                .HasOne(l => l.Job)
                .WithMany(j => j.Loads)
                .HasForeignKey(l => l.JobId);

            modelBuilder.Entity<Load>()
                .HasOne(l => l.TransportUnit)
                .WithMany(t => t.Loads)
                .HasForeignKey(l => l.TransportUnitId);

            // Decimal precision
            modelBuilder.Entity<Job>()
             .Property(j => j.Cost)
             .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Load>()
                .Property(l => l.Weight)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Load>()
                .Property(l => l.Volume)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransportUnit>()
                .Property(t => t.MaxWeight)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransportUnit>()
                .Property(t => t.MaxVolume)
                .HasColumnType("decimal(10,2)");
         }

        }
    }