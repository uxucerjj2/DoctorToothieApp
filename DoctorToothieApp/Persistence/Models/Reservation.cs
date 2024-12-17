using Microsoft.AspNetCore.Identity;

namespace DoctorToothieApp.Persistence.Models;

public enum ReservationStage : int
{
    LOCATION, // User
    ROOM, // User
    PATIENT, // User
    DOCTOR, // User
    PROCEDURE, // User
    DATE, // User
    REVIEW_CLIENT, // User
    SUBMITTED, // User
    REVIEW_DOCTOR, // Doctor
    ACCEPTED, // DOCTOR
    COMPLETED,  // Doctor
    CANCELED // User, Doctor
}

public class Reservation
{
    public int Id { get; set; }
    public ReservationStage Stage { get; set; }
    public int Status { get; set; }

    public AppUser CreatedBy { get; set; } = default!;
    public string CreatedById { get; set; } = "";

    public AppUser? Patient { get; set; }
    public string? PatientId { get; set; }
    public Location? Location { get; set; }
    public int? LocationId { get; set; }
    public Room? Room { get; set; }
    public int? RoomId { get; set; }
    public AppUser? Doctor { get; set; }
    public string? DoctorId { get; set; }
    public ProcedureType? ProcedureType { get; set; }
    public int? ProcedureTypeId { get; set; }
    public DateTime? Time { get; set; }
    public string? ProcedureNotes { get; set; }
}
