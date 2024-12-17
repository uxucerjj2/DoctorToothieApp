using System.Security.Claims;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Controllers;

public class AppointmentsIndexVM
{
    public IList<Reservation> Current { get; set; } = [];
    public IList<Reservation> ToAccept { get; set; } = [];

    public IList<Reservation> History { get; set; } = [];

    public string StageToString(ReservationStage stage)
    {
        return stage switch
        {
            ReservationStage.COMPLETED => "Zakończony",
            _ => Enum.GetName(stage)!,
        };
    }
}

public class AppointmentsController(IDbContext context) : Controller
{
    string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var reservations = context.Reservations
            .Include(e=>e.Location)
            .Include(e => e.Patient)
            .Include(e => e.Room)
            .Include(e => e.ProcedureType)
            .Where(e => e.DoctorId == UserId);

        var current = await reservations.Where(e => e.Stage == ReservationStage.ACCEPTED).ToListAsync();
        var toAccept = await reservations.Where(e => 
                e.Stage == ReservationStage.SUBMITTED || e.Stage == ReservationStage.REVIEW_DOCTOR
            ).ToListAsync();
        var history = await reservations.Where(e =>
                e.Stage == ReservationStage.COMPLETED || e.Stage == ReservationStage.CANCELED
            ).ToListAsync();

        return View(new AppointmentsIndexVM
        {
            Current = current,
            ToAccept = toAccept,
            History = history
        });
    }

    public async Task<IActionResult> Review(int id)
    {
        var res = await context.Reservations
            .Include(e => e.Location)
            .Include(e => e.Patient)
            .Include(e => e.Room)
            .Include(e => e.ProcedureType)
            .SingleAsync(e => e.Id == id);
        res.Stage = ReservationStage.REVIEW_DOCTOR;
        await context.SaveChangesAsync();

        return View(res);
    }

    [HttpPost]
    public async Task<IActionResult> Accept([FromForm] Reservation r)
    {
        var res = await context.Reservations.SingleAsync(e => e.Id == r.Id);
        res.ProcedureNotes = r.ProcedureNotes;
        res.Time = ((DateTime)r.Time!).ToUniversalTime();
        res.Stage = ReservationStage.ACCEPTED;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Cancel(int id)
    {
        var res = await context.Reservations.SingleAsync(e => e.Id == id);
        res.Stage = ReservationStage.CANCELED;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Done(int id)
    {
        var res = await context.Reservations.SingleAsync(e => e.Id == id);
        res.Stage = ReservationStage.COMPLETED;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}
