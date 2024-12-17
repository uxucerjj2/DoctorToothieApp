using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using DoctorToothieApp.Validations;
using DoctorToothieApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using DoctorToothieApp.Persistence.Interfaces;
using DoctorToothieApp.Persistence.Models;

namespace DoctorToothieApp.Controllers;

public class ReserveDateVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }

    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Preferowany dzień")]
    [DataType(DataType.Date)]
    [DateRangeFromNow(false, ErrorMessage = "Data musi być późniejsza niż dzisiaj!")]
    public DateTime Time { get; set; }
}

public class ReserveLocationVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }
    [ValidateNever]
    public IList<SelectListItem> Locations { get; set; } = [];


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Lokalizacja")]
    public int? LocationId { get; set; } = null;
}

public class ReserveRoomVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }
    [ValidateNever]
    public IList<SelectListItem> Rooms { get; set; } = [];


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Pokój")]
    public int? RoomId { get; set; } = null;
}
public class ReservePatientVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }
    [ValidateNever]
    public IList<SelectListItem> Patients { get; set; } = [];


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Pacjent")]
    public string? PatientId { get; set; } = null;
}
public class ReserveDoctorVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }
    [ValidateNever]
    public IList<SelectListItem> Doctors { get; set; } = [];

    [Required(ErrorMessage = "Pole {0} jest wymagane")]
    [DisplayName("Doktor")]
    public string? DoctorId { get; set; } = null;
}

public class ReserveProcedureVM
{
    [ValidateNever]
    public Reservation Reservation { get; set; }
    [ValidateNever]
    public IList<SelectListItem> Procedures { get; set; } = [];


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Procedura")]
    public int? ProcedureId { get; set; } = null;
}

public class ReserveIndexVM
{
    public IList<Reservation> Current { get; set; } = [];
    public IList<Reservation> Ended { get; set; } = [];

    public Reservation? InProgress { get; set; } = null;

}

[Authorize]
[Controller]
public class ReserveController(IDbContext context) : Controller
{
    string UserID => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    readonly IList<ReservationStage> Completed = new List<ReservationStage> { 
        ReservationStage.COMPLETED, ReservationStage.CANCELED, ReservationStage.REVIEW_DOCTOR, ReservationStage.SUBMITTED, ReservationStage.ACCEPTED };

    public async Task<Reservation> GetReservation()
    {
        return await context.Reservations.Where(u => u.CreatedById == UserID).SingleAsync(e => !Completed.Contains(e.Stage));
    }

    public async Task UpdateReservation(ReservationStage stage)
    {
        var reservation = await GetReservation();
        reservation.Stage = stage;
        await context.SaveChangesAsync();
    }

    public async Task<IActionResult> Index()
    {
        var reservetions = context.Reservations
            .Include(e => e.Patient)
            .Include(e => e.Doctor)
            .Include(e=>e.Location)
            .Include(e => e.Room)
            .Include(e => e.ProcedureType)
            .Where(e => e.CreatedById == UserID || e.PatientId == UserID);

        var ended = await reservetions.Where(e => new List<ReservationStage> { ReservationStage.CANCELED, ReservationStage.COMPLETED }.Contains(e.Stage)).ToListAsync();
        var current = await reservetions.Where(e =>
            new List<ReservationStage> {
                    ReservationStage.REVIEW_DOCTOR,
                    ReservationStage.SUBMITTED,
                    ReservationStage.ACCEPTED
                }.Contains(e.Stage)
            ).ToListAsync();
        var inprogress = await reservetions.SingleOrDefaultAsync(e =>

            !new List<ReservationStage> { 
                ReservationStage.CANCELED, 
                ReservationStage.COMPLETED, 
                ReservationStage.REVIEW_DOCTOR, 
                ReservationStage.SUBMITTED,
                ReservationStage.ACCEPTED
            }.Contains(e.Stage)

        );

        return View(new ReserveIndexVM { Ended = ended, Current = current, InProgress = inprogress});
    }

    public async Task<IActionResult> Cancel()
    {
        var re =await  GetReservation();
        context.Reservations.Remove(re);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<ReserveLocationVM> fillLocationVM(ReserveLocationVM? vm)
    {
        var res = context.Reservations.Where(u => u.CreatedById == UserID);
        var inProgress = await res.SingleOrDefaultAsync(e => !Completed.Contains(e.Stage));

        var reservation = inProgress ?? new Reservation
        {
            Stage = ReservationStage.LOCATION,
            CreatedById = UserID
        };

        if (inProgress == null)
        {
            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();
        }

        var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = null,
                Text = "-- Wybierz --",
                Disabled = true,
                Selected = true
            }
        };
        list.AddRange(await context.Locations.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = e.Name
        }).ToListAsync());

        vm ??= new ReserveLocationVM
        {
            Reservation = reservation,
        };

        vm.Locations = list;    
        vm.Reservation = reservation;

        return vm;
    }

    [HttpGet]
    public async Task<IActionResult> Location()
    {
        return View(await fillLocationVM(null));
    }

    [HttpPost]
    public async Task<IActionResult> Location(ReserveLocationVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Location), await fillLocationVM(vm));
        }


        var rev = await GetReservation();
        rev.LocationId = vm.LocationId;
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.ROOM);
        return RedirectToAction(nameof(Room));

    }

    private async Task<ReserveRoomVM> FillRoomVM(ReserveRoomVM? vm)
    {
        var res = context.Reservations.Where(u => u.CreatedById == UserID);
        var inProgress = await res.SingleAsync(e => !Completed.Contains(e.Stage));

        var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = null,
                Text = "-- Wybierz --",
                Disabled = true,
                Selected = true
            }
        };
        list.AddRange(await context.Rooms.Where(e => e.ParentId == inProgress.LocationId).Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = e.Number
        }).ToListAsync());

        vm ??= new ReserveRoomVM
        {
            Reservation = inProgress,
        };

        vm.Rooms = list;
        vm.Reservation = inProgress;

        return vm;
    }

    public async Task<IActionResult> Room()
    {
        return View(await FillRoomVM(null));
    }
    [HttpPost]
    public async Task<IActionResult> Room(ReserveRoomVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Room), await FillRoomVM(vm));
        }

        var rev = await GetReservation();
        rev.RoomId = vm.RoomId;
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.PATIENT);
        return RedirectToAction(nameof(Patient));

    
    }

    private async Task<ReservePatientVM> FillPatienVM(ReservePatientVM? vm)
    {
        var res = context.Reservations.Where(u => u.CreatedById == UserID);
        var inProgress = await res.SingleAsync(e => !Completed.Contains(e.Stage));

        var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = null,
                Text = "-- Wybierz --",
                Disabled = true,
                Selected = true
            }
        };
        {
            var uu = await context.Users.SingleAsync(e => e.Id == UserID);
            list.Add(new SelectListItem
            {
                Value = uu.Id,
                Text = $"{uu.FirstName} {uu.LastName} - {uu.Email}"
            });
        }
        list.AddRange(await context.Users.Where(e => e.ParentId == UserID).Select(e => new SelectListItem
        {

            Value = e.Id,
            Text = $"{e.FirstName} {e.LastName} - {e.Email}"
        }).ToListAsync());

        vm ??= new ReservePatientVM
        {
            Reservation = inProgress,
            Patients = list
        };
        vm.Reservation = inProgress;
        vm.Patients = list;

        return vm;
    }

    public async Task<IActionResult> Patient()
    {
        return View(await FillPatienVM(null));
    }
    [HttpPost]
    public async Task<IActionResult> Patient(ReservePatientVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Patient), await FillPatienVM(vm));
        }

        var rev = await GetReservation();
        rev.PatientId = vm.PatientId;
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.DOCTOR);
        return RedirectToAction(nameof(Doctor));
    }

    private async Task<ReserveDoctorVM> FillDoctorVM(ReserveDoctorVM? vm)
    {
        var res = context.Reservations.Where(u => u.CreatedById == UserID);
        var inProgress = await res.SingleAsync(e => !Completed.Contains(e.Stage));

        var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = null,
                Text = "-- Wybierz --",
                Disabled = true,
                Selected = true
            }
        };
        list.AddRange(await context.Users.Where(e => e.EmployeedLocationId == inProgress.LocationId).Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = $"{e.FirstName} {e.LastName}"
        }).ToListAsync());

        vm ??= new ReserveDoctorVM
        {
            Reservation = inProgress,
            Doctors = list
        };
        vm.Reservation = inProgress;
        vm.Doctors = list;

        return vm;
    }

    public async Task<IActionResult> Doctor()
    {
        return View(await FillDoctorVM(null));
    }
    [HttpPost]
    public async Task<IActionResult> Doctor(ReserveDoctorVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Doctor), await FillDoctorVM(vm));
        }

        var rev = await GetReservation();
        rev.DoctorId = vm.DoctorId;
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.PROCEDURE);
        return RedirectToAction(nameof(Procedure));
    }

    private async Task<ReserveProcedureVM> FillProcedureVM(ReserveProcedureVM? vm)
    {
        var res = context.Reservations.Where(u => u.CreatedById == UserID);
        var inProgress = await res.SingleAsync(e => !Completed.Contains(e.Stage));

        var list = new List<SelectListItem>
        {
            new SelectListItem
            {
                Value = null,
                Text = "-- Wybierz --",
                Disabled = true,
                Selected = true
            }
        };
        list.AddRange(await context.ProcedureTypes.Select(e => new SelectListItem
        {
            Value = e.Id.ToString(),
            Text = $"{e.Description} - {e.BasePrice}"
        }).ToListAsync());

        vm ??= new ReserveProcedureVM
        {
            Reservation = inProgress,
            Procedures = list
        };
        vm.Reservation = inProgress;
        vm.Procedures = list;

        return vm;
    }

    public async Task<IActionResult> Procedure()
    {
        return View(await FillProcedureVM(null));
    }
    [HttpPost]
    public async Task<IActionResult> Procedure(ReserveProcedureVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Procedure), await FillProcedureVM(vm));
        }

        var rev = await GetReservation();
        rev.ProcedureTypeId = vm.ProcedureId;
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.DATE);
        return RedirectToAction(nameof(Date));
    }


    public async Task<IActionResult> Date()
    {
        var rev = await GetReservation();
        return View(new ReserveDateVM { Reservation = rev, Time = DateTime.UtcNow});
    }
    [HttpPost]
    public async Task<IActionResult> Date(ReserveDateVM vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Reservation = await GetReservation();
            return View(nameof(Date), vm);
        }

        var rev = await GetReservation();
        rev.Time = vm.Time.ToUniversalTime();
        await context.SaveChangesAsync();

        await UpdateReservation(ReservationStage.REVIEW_CLIENT);
        return RedirectToAction(nameof(Review));
    }


    public async Task<IActionResult> Review()
    {
        var vm = await context.Reservations
            .Include(e => e.ProcedureType)
            .Include(e => e.Patient)
            .Include(e => e.Location)
            .Include(e => e.Room)
            .Include(e => e.Doctor)
            .Where(u => u.CreatedById == UserID).SingleAsync(e => !Completed.Contains(e.Stage));
        return View(vm);
    }

    public async Task<IActionResult> Reserve()
    {
        await UpdateReservation(ReservationStage.SUBMITTED);

        return RedirectToAction(nameof(Index));
    }

}
