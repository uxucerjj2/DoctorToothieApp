namespace DoctorToothieApp.Persistence.Models;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = default!;

    public int ParentId { get; set; }
    public Location Parent { get; set; } = default!;
}