namespace DoctorToothieApp.Persistence.Models;

public class Location
{

    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;

    public string? Image { get; set; } = default!;
    public IList<Room> Rooms { get; set; } = [];
    public IList<AppUser> Employees { get; set; } = [];

}