using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Remp.Common.Constants;

namespace Remp.DataAccess.Data;

public class DbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbSeeder> _logger;
    public DbSeeder(RoleManager<IdentityRole> roleManager,ILogger<DbSeeder> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
    }
    private async Task SeedRolesAsync()
    {
        var rolesToSeed = new[]
        {
            Roles.PhotographyCompany,
            Roles.Agent
        };
    

        foreach (var roleName in rolesToSeed)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                    "Role '{RoleName}' seeded successfully.", roleName);
                }
                else
                {
                    var errors = string.Join(", ",
                        result.Errors.Select(e => e.Description));

                    _logger.LogError(
                        "Failed to seed role '{RoleName}'. Errors: {Errors}",
                        roleName, errors);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Role '{RoleName}' already exists. Skipping.", roleName);
            }
        }
    }

}