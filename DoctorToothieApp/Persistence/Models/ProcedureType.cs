namespace DoctorToothieApp.Persistence.Models;

public class ProcedureType
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int BasePrice { get; set; }

}
