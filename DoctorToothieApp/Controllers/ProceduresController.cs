using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DoctorToothieApp.Persistence.Interfaces;
using DoctorToothieApp.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Controllers;

public class ProceduresNewVM
{
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Nazwa")]
    public string Title { get; set; } = default!;
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Opis")]
    public string Description { get; set; } = default!;
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Cena pod.")]
    public int BasePrice { get; set; }
}

public class ProceduresEditVM
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Nazwa")]
    public string Title { get; set; } = default!;
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Opis")]
    public string Description { get; set; } = default!;
    [Required(ErrorMessage = "Pole {0} jest puste")]
    [DisplayName("Cena pod.")]
    public int BasePrice { get; set; }
}

public class ProceduresController(IDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var procedures = (await context.ProcedureTypes.ToListAsync()) ?? [];

        return View(procedures);
    }

    public async Task<IActionResult> New()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> New([FromForm] ProceduresNewVM vm)
    {
        if(!ModelState.IsValid)
        {
            return View(nameof(New), vm);
        }

        context.ProcedureTypes.Add(new ProcedureType
        {
            BasePrice = vm.BasePrice,
            Title = vm.Title,
            Description = vm.Description,
        });

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var proc = await context.ProcedureTypes.SingleAsync(e => e.Id == id);
        return View(new ProceduresEditVM
        {
            Id = proc.Id,
            Title = proc.Title,
            Description = proc.Description,
            BasePrice = proc.BasePrice,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] ProceduresEditVM vm)
    {
        if(!ModelState.IsValid)
        {
            return View(nameof(Edit), vm);
        }


        var proc = await context.ProcedureTypes.SingleAsync(e => e.Id == vm.Id);

        proc.BasePrice = vm.BasePrice;
        proc.Title = vm.Title;
        proc.Description = vm.Description;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
