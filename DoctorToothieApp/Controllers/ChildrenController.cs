using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoctorToothieApp.Controllers;

public class ChildrenNewVM
{
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Text)]
        [Display(Name = "Imie")]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Text)]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = "";

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
}

public class ChildrenEditVM
{
    [HiddenInput]
    public required string Id { get; set; }

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
    [DataType(DataType.Text)]
    [Display(Name = "Imie")]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
    [DataType(DataType.Text)]
    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = "";

    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }


    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}

[Authorize(Roles = "User")]
public class ChildrenController(IDbContext context, UserManager<AppUser> userManager) : Controller
{
    string UserID => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var children = await context.Users.Where(e => e.ParentId == UserID).ToListAsync() ?? [];

        return View(children);
    }


    public async Task<IActionResult> New()
    {
        return View(new ChildrenNewVM());
    }

    [HttpPost]
    public async Task<IActionResult> New([FromForm] ChildrenNewVM vm)
    {
        var parent = await userManager.FindByIdAsync(UserID);

        var user = Activator.CreateInstance<AppUser>();
        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.ParentId = UserID;


        await userManager.SetUserNameAsync(user, $"{parent.Email}_{vm.FirstName}_{vm.LastName}");
        if(!string.IsNullOrWhiteSpace(vm.Email))
        {
            await userManager.SetUserNameAsync(user, vm.Email);
            await userManager.SetEmailAsync(user, vm.Email);
        }
        await userManager.SetLockoutEnabledAsync(user, true);

        IdentityResult result;

        if (vm.Password != null && vm.Password == vm.ConfirmPassword)
        {
            result = await userManager.CreateAsync(user, vm.Password);
        }
        else
        {
            result = await userManager.CreateAsync(user);
        }


        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await userManager.ConfirmEmailAsync(user, token);
        await userManager.AddToRoleAsync(user, "User");

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        if (result.Errors.Any())
        {
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var child = await context.Users.SingleOrDefaultAsync(e => e.Id == id && e.ParentId == UserID);
        if (child == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ChildrenEditVM
        {
            Id = id,
            Email = child.Email,
            FirstName = child.FirstName,
            LastName = child.LastName
        });
    }
    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] ChildrenEditVM vm)
    {
        var child = await userManager.FindByIdAsync(vm.Id);
        if (child == null || child.ParentId != UserID)
        {
            return RedirectToAction(nameof(Index));
        }
        child.FirstName = vm.FirstName;
        child.LastName = vm.LastName;

        var parent = await context.Users.AsNoTracking().SingleAsync(e => e.Id == UserID);

        var result = await userManager.SetEmailAsync(child, vm.Email);
        if (result.Errors.Any())
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(nameof(Edit), vm);
        }
        if (!string.IsNullOrWhiteSpace(vm.Email))
        {
            result = await userManager.SetUserNameAsync(child, vm.Email);
            if (result.Errors.Any())
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(nameof(Edit), vm);
            }
        }else
        {
            await userManager.SetUserNameAsync(child, $"{parent.Email}_{child.FirstName}_{child.LastName}");
        }



        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Unlock(string id)
    {
        var child = await userManager.FindByIdAsync(id);
        if (child == null || child.ParentId != UserID)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(child.Email))
        {
            return RedirectToAction(nameof(Edit), new             {
                id = id,
            });
        }


        await userManager.SetLockoutEnabledAsync(child, false);


        return RedirectToAction(nameof(Index));
    }

}
