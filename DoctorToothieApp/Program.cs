using DoctorToothieApp.Persistence;
using DoctorToothieApp.Persistence.Models;
using DoctorToothieApp.Persistence.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddScoped<IDbContext,ApplicationDbContext>();
builder.Services.AddDbContext<ApplicationDbContext>(
    //options => options.UseSqlServer(connectionString)
    options => options.UseNpgsql(connectionString)
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();
builder.Services.AddIdentity<AppUser, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = true
)
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders().AddApiEndpoints();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthorization();


//app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
