using Microsoft.AspNetCore.Identity;

namespace DoctorToothieApp.Persistence.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;


    public int? EmployeedLocationId { get; set; }
    public Location? EmployeedLocation { get; set; }


    public string? ParentId { get; set; }
    public AppUser? Parent { get; set; }
    public IList<AppUser> Children { get; set; } = [];
    public IList<Reservation> Reservations { get; set; } = [];
}
