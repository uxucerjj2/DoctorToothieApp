using System.Reflection;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Extensions;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options), IDbContext
{
    public DbSet<ProcedureType> ProcedureTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Room>()
            .HasOne(e => e.Parent)
            .WithMany(e => e.Rooms)
            .HasForeignKey(e => e.ParentId);

        builder.SeedRole();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

}
