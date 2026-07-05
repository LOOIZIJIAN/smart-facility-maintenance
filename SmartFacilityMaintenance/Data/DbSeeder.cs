using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartFacilityMaintenance.Models;

namespace SmartFacilityMaintenance.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, "admin@campus.edu", "Admin@123", "System Administrator", Roles.Admin, "ICT Services", "ADM-001");
        var staff = await EnsureUserAsync(userManager, "staff@campus.edu", "Staff@123", "Nurul Aisyah", Roles.Staff, "Faculty of Computing", "STF-1001");
        var tech = await EnsureUserAsync(userManager, "tech@campus.edu", "Tech@123", "Ravi Kumar", Roles.Maintenance, "Facilities Department", "MNT-2001");
        var student = await EnsureUserAsync(userManager, "student@campus.edu", "Student@123", "Lee Wei Ming", Roles.Student, "BCSI", "I25000001");

        if (!await context.Buildings.AnyAsync())
        {
            context.Buildings.AddRange(
                new Building { Name = "Block A - Main", Code = "A" },
                new Building { Name = "Block B - Engineering", Code = "B" },
                new Building { Name = "Block C - Computing", Code = "C" },
                new Building { Name = "Library", Code = "LIB" },
                new Building { Name = "Student Hostel", Code = "HST" });
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Furniture" },
                new Category { Name = "Projector" },
                new Category { Name = "Lighting" },
                new Category { Name = "Air-Conditioning" },
                new Category { Name = "Internet" },
                new Category { Name = "Others" });
            await context.SaveChangesAsync();
        }

        if (!await context.MaintenanceRequests.AnyAsync())
        {
            var buildings = await context.Buildings.ToListAsync();
            var categories = await context.Categories.ToListAsync();

            Building GetBuilding(string code) => buildings.First(x => x.Code == code);
            Category GetCategory(string name) => categories.First(x => x.Name == name);

            var now = DateTime.UtcNow;

            var samples = new List<MaintenanceRequest>
            {
                new()
                {
                    Title = "Projector not turning on in C-305",
                    Description = "The ceiling projector in room C-305 does not power on. Class affected.",
                    Category = GetCategory("Projector"), Building = GetBuilding("C"), RoomLocation = "Level 3, Room C-305",
                    Priority = RequestPriority.High, Status = RequestStatus.Completed,
                    ReportedById = student.Id, AssignedToId = tech.Id,
                    CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-18), CompletedAt = now.AddDays(-18)
                },
                new()
                {
                    Title = "Air-conditioning leaking water in Library",
                    Description = "The AC unit near the reading area is dripping water onto the floor.",
                    Category = GetCategory("Air-Conditioning"), Building = GetBuilding("LIB"), RoomLocation = "Ground floor reading area",
                    Priority = RequestPriority.High, Status = RequestStatus.InProgress,
                    ReportedById = staff.Id, AssignedToId = tech.Id,
                    CreatedAt = now.AddDays(-6), UpdatedAt = now.AddDays(-2)
                },
                new()
                {
                    Title = "Flickering lights in Block A corridor",
                    Description = "Several ceiling lights flicker along the Level 2 corridor.",
                    Category = GetCategory("Lighting"), Building = GetBuilding("A"), RoomLocation = "Level 2 corridor",
                    Priority = RequestPriority.Medium, Status = RequestStatus.Assigned,
                    ReportedById = student.Id, AssignedToId = tech.Id,
                    CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-3)
                },
                new()
                {
                    Title = "Broken chair in B-210",
                    Description = "A student chair has a broken backrest and is unsafe to use.",
                    Category = GetCategory("Furniture"), Building = GetBuilding("B"), RoomLocation = "Room B-210",
                    Priority = RequestPriority.Low, Status = RequestStatus.Submitted,
                    ReportedById = student.Id,
                    CreatedAt = now.AddDays(-1), UpdatedAt = now.AddDays(-1)
                },
                new()
                {
                    Title = "No Wi-Fi in Student Hostel common area",
                    Description = "Wi-Fi has been down in the hostel common area since yesterday.",
                    Category = GetCategory("Internet"), Building = GetBuilding("HST"), RoomLocation = "Common area, Level 1",
                    Priority = RequestPriority.High, Status = RequestStatus.Completed,
                    ReportedById = student.Id, AssignedToId = tech.Id,
                    CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-27), CompletedAt = now.AddDays(-27)
                }
            };

            context.MaintenanceRequests.AddRange(samples);
            await context.SaveChangesAsync();

            var first = samples[0];
            context.RequestActivityLogs.AddRange(
                new RequestActivityLog
                {
                    MaintenanceRequestId = first.Id, PerformedById = student.Id,
                    Action = "Created", NewStatus = RequestStatus.Submitted,
                    Notes = "Request submitted.", Timestamp = first.CreatedAt
                },
                new RequestActivityLog
                {
                    MaintenanceRequestId = first.Id, PerformedById = tech.Id,
                    Action = "StatusChanged", OldStatus = RequestStatus.Submitted, NewStatus = RequestStatus.Completed,
                    Notes = "Projector lamp replaced and tested.", Timestamp = first.CompletedAt ?? now
                });
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string role,
        string? department, string? staffStudentId)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Department = department,
                StaffStudentId = staffStudentId
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}
