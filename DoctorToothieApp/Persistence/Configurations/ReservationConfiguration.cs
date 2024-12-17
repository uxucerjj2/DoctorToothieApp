using DoctorToothieApp.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorToothieApp.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder
           .HasOne(e => e.Location)
           .WithMany();

        builder
            .HasOne(e => e.Doctor)
            .WithMany();

        builder
            .HasOne(e => e.Room)
            .WithMany();

        builder
            .HasOne(e => e.ProcedureType)
            .WithMany();
    }
}
