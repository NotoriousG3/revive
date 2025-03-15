using Microsoft.AspNetCore.Identity;

namespace SnapWebManager.Data;

public enum Roles
{
    Admin,
    User
}

public class IdentitySeed
{
    private static readonly string _defaultAdminPassword = "Passw0rd!01";

    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.CreateScope();
        var roleManager = context.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.User.ToString()));
    }

    public static async Task CreateDefaultAdmin(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.CreateScope();
        var roleManager = context.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = context.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        //Seed Default User
        var defaultUser = new IdentityUser
        {
            UserName = "default",
            Email = "default@snapweb.com",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, _defaultAdminPassword);
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.User.ToString());
            }
        }
    }
}