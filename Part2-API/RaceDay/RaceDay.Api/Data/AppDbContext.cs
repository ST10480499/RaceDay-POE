using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

// This class is the link between the C# models and the SQL Server database.
// Entity Framework uses it to create the tables and to read and save data.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Each DbSet becomes one table in the database
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    // This method sets the rules for each table, matching the Part 1 ERD and SQL script
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.RoleID);
            entity.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.RoleName).IsUnique();

            // The two roles are added automatically when the database is created
            entity.HasData(
                new Role { RoleID = 1, RoleName = "Organiser" },
                new Role { RoleID = 2, RoleName = "Participant" });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserID);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);

            // Two users cannot register with the same email
            entity.HasIndex(u => u.Email).IsUnique();

            // Each user has one role, and a role that is in use cannot be deleted
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventID);
            entity.Property(e => e.EventName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.EventDate).HasColumnType("date");
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Distance).HasColumnType("decimal(6,2)");
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(20);

            // Every event belongs to the organiser who created it
            entity.HasOne(e => e.Organiser)
                  .WithMany()
                  .HasForeignKey(e => e.OrganiserID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryID);
            entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(50);

            // Deleting an event also deletes its categories
            entity.HasOne(c => c.Event)
                  .WithMany(e => e.Categories)
                  .HasForeignKey(c => c.EventID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentID);

            // A participant can only enrol once in the same category
            entity.HasIndex(e => new { e.ParticipantID, e.CategoryID }).IsUnique();

            entity.HasOne(e => e.Participant)
                  .WithMany()
                  .HasForeignKey(e => e.ParticipantID)
                  .OnDelete(DeleteBehavior.Restrict);

            // Deleting a category also deletes its enrolments
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Enrolments)
                  .HasForeignKey(e => e.CategoryID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(r => r.ResultID);

            // One enrolment can only have one result
            entity.HasIndex(r => r.EnrolmentID).IsUnique();

            entity.HasOne(r => r.Enrolment)
                  .WithOne(e => e.Result)
                  .HasForeignKey<Result>(r => r.EnrolmentID)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}