using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Controllers;


public class UserEntry
{
    public required AppUser User { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class RolesEditUserVM
{
    [HiddenInput]
    public required string UserId { get; set; }
    [ValidateNever]
    public required List<string> AviableRoles { get; set; }

    [Required(ErrorMessage = "Pole {0} jest wymagane")]
    [DisplayName("Role")]
    public List<string> Roles { get; set; } = [];
}

[Authorize(Roles = "Admin")]
public class RolesController(IDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<UserEntry> users = (await context.Users.Select(e => new UserEntry { User = e }).ToListAsync()) ?? [];


        foreach (var user in users)
        {
            user.Roles = [.. (await userManager.GetRolesAsync(user.User))];
        }

        return View(users);
    }

    public async Task<IActionResult> EditUser(string user)
    {
        var roles = (await roleManager.Roles.Select(e => e.Name!).ToListAsync()) ?? [];

        return View(new RolesEditUserVM { AviableRoles = roles, UserId = user});
    }

    [HttpPost]
    public async Task<IActionResult> EditUser([FromForm] RolesEditUserVM vm)
    {
        if(!ModelState.IsValid)
        {
            vm.AviableRoles = (await roleManager.Roles.Select(e => e.Name!).ToListAsync()) ?? [];
            return View(nameof(EditUser), vm);
        }

        var roles = (await roleManager.Roles.Select(e => e.Name!).ToListAsync()) ?? [];
        var user = await userManager.FindByIdAsync(vm.UserId)!;
        foreach (var role in roles)
        {
            await userManager.RemoveFromRoleAsync(user!, role);
        }
        foreach (var role in vm.Roles)
        {
            await userManager.AddToRoleAsync(user!, role);
        }
        
        return RedirectToAction(nameof(Index));
    }
}
