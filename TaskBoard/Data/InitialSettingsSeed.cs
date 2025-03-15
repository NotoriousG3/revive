using TaskBoard.Models;

namespace TaskBoard.Data;

public class InitialSettingsSeed
{
    public static async Task CreateSettings(IServiceProvider serviceProvider)
    {
        // Create our settings record in case remote settings don't work
        var settings = new AppSettings();
        var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.AppSettings.Count() > 0) return;
        context.AppSettings.Add(settings);
        await context.SaveChangesAsync();
    }
}