using DoctorToothieApp.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorToothieApp.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder
            .HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId);

        builder
            .HasOne(e => e.EmployeedLocation)
            .WithMany(e => e.Employees)
            .HasForeignKey(e => e.EmployeedLocationId);

        builder
            .HasMany(e => e.Reservations)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId);
    }
}
