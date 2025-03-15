using Microsoft.AspNetCore.Identity;
using SnapWebModels;

namespace SnapWebManager.Data;

public class SnapWebModulesSeed
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var currentModules = context.Modules.ToList();
        foreach (var module in SnapWebModule.DefaultModules)
        {
            if (currentModules.Any(m => m.Id == module.Id)) continue;

            context.Add(module);
        }

        await context.SaveChangesAsync();
    }
}