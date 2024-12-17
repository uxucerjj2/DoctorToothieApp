using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Controllers;

public class DoctorsNewVM
{
    [ValidateNever]
    public IList<SelectListItem> Users { get; set; } = [];

    [ValidateNever]
    public IList<SelectListItem> Locations { get; set; } = [];


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Doktor")]
    public string? DoctorId { get; set; }


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Lokalizacja")]
    public int? LocationId { get; set; }
}

public class DoctorsEditVM
{
    [ValidateNever]
    public AppUser Doctor { get; set; }


    [ValidateNever]
    public IList<SelectListItem> Locations { get; set; } = [];

    [HiddenInput]
    public string? DoctorId { get; set; }


    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Lokalizacja")]
    public int? LocationId { get; set; }


}

public class DoctorsController(IDbContext context, UserManager<AppUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var doctors = await context.Users.Include(e => e.EmployeedLocation).Where(e => e.EmployeedLocationId != null).ToListAsync();
        return View(doctors);
    }
    private async Task<List<SelectListItem>> GetUsers()
    {
        var users = new List<SelectListItem> { new SelectListItem {
            Disabled = true,
            Selected = true,
            Text = "-- Wybierz --"
        } };

        users.AddRange(await context.Users.Select(e => new SelectListItem
        {
            Text = $"{e.FirstName} {e.LastName} - {e.Email}",
            Value = e.Id.ToString()
        }).ToListAsync());

        return users;
    }

    private async Task<List<SelectListItem>> GetLocations()
    {
        var locations = new List<SelectListItem> { new SelectListItem {
            Disabled = true,
            Selected = true,
            Text = "-- Wybierz --"
        } };

        locations.AddRange(await context.Locations.Select(e => new SelectListItem
        {
            Text = $"{e.Name} - {e.Address}",
            Value = e.Id.ToString()
        }).ToListAsync());

        return locations;
    }

    public async Task<IActionResult> New()
    {
        return View(new DoctorsNewVM
        {
            Locations = await GetLocations(),
            Users = await GetUsers(),
        });
    }

    [HttpPost]
    public async Task<IActionResult> New(DoctorsNewVM vm)
    {
        if(!ModelState.IsValid)
        {
            return View(new DoctorsNewVM
            {
                DoctorId = vm.DoctorId,
                LocationId = vm.LocationId,
                Locations = await GetLocations(),
                Users = await GetUsers(),
            });
        }

        var doctor = await userManager.FindByIdAsync(vm.DoctorId);
        doctor.EmployeedLocationId = vm.LocationId;
        await userManager.AddToRoleAsync(doctor, "Doctor");


        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var doctor = await context.Users.SingleAsync(e => e.Id == id && e.EmployeedLocationId != null);
        return View(new DoctorsEditVM
        {
            Doctor = doctor,
            DoctorId = id,
            LocationId = doctor.EmployeedLocationId,
            Locations = await GetLocations()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(DoctorsEditVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View(new DoctorsEditVM
            {
                DoctorId = vm.DoctorId,
                LocationId = vm.LocationId,
                Locations = await GetLocations()
            });
        }

        var doctor = await context.Users.SingleAsync(e=>e.Id == vm.DoctorId);
        doctor.EmployeedLocationId = vm.LocationId;
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(string id)
    {
        var doctor = await userManager.FindByIdAsync(id);

        if (doctor != null)
        {
            doctor.EmployeedLocationId = null;
            await userManager.RemoveFromRoleAsync(doctor, "Doctor");
        }

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> FixRoles()
    {
        var doctors = await userManager.Users.Where(e => e.EmployeedLocationId != null).ToListAsync();

        foreach (var doctor in doctors)
        {
            await userManager.AddToRoleAsync(doctor, "Doctor");
        }

        return RedirectToAction(nameof(Index));
    }
}
