using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomBook.Data.Models;

namespace RoomBook.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Migrate legacy "Member" role (from earlier versions) to "User".
        var legacyMember = await roles.FindByNameAsync("Member");
        if (legacyMember is not null)
        {
            legacyMember.Name = "User";
            legacyMember.NormalizedName = "USER";
            await roles.UpdateAsync(legacyMember);
        }

        foreach (var role in new[] { "Admin", "User" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        const string email = "admin@roombook.local";
        if (await users.FindByEmailAsync(email) is null)
        {
            var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = "System Admin" };
            var password = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Set SeedAdmin:Password in User Secrets or an environment variable before the initial startup.");
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            await users.AddToRoleAsync(admin, "Admin");
        }

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.AddRange(
                new Room { Name = "Orchid", Type = "Meeting Room", Location = "Floor 2", Capacity = 8 },
                new Room { Name = "Atlas", Type = "Conference Hall", Location = "Floor 3", Capacity = 16 },
                new Room { Name = "Studio", Type = "Studio", Location = "Floor 1", Capacity = 4 });
            await db.SaveChangesAsync();
        }
    }
}
