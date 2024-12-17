using DoctorToothieApp.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Persistence.Interfaces;

public interface IDbContext
{
    DbSet<ProcedureType> ProcedureTypes { get; }
    DbSet<AppUser> Users { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Location> Locations { get; }
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
