using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoctorToothieApp.Persistence.Extensions;

public static class ModelBuilderExtensionRole
{
    public static void SeedRole(this ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1E2B8D51-DA03-4920-B675-E0504ED8E7FF",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "19B13701-0451-412B-B80E-6FD559437F53"
            },
            new IdentityRole
            {
                Id = "9F753729-3A0F-4AC0-8130-11E1133A8DF6",
                Name = "Doctor",
                NormalizedName = "DOCTOR",
                ConcurrencyStamp = "1CA2811D-872D-49AD-8F4B-4364B7D23FBC"
            },
            new IdentityRole
            {
                Id = "B3CCBED4-1866-4323-A06B-ED7D3BBDB3C4",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "27D16A8F-8C32-4F46-BFEE-2CB8A3AA8B10"
            }
        );
    }
}
