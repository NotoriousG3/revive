using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class AppSettingsLoader
{
    private readonly ApplicationDbContext _context;
    
    public AppSettingsLoader() {}

    public AppSettingsLoader(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<AppSettings> Load()
    {
        if (_context.AppSettings != null)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null) return new AppSettings();

            AppSettings.Validate(settings);

            await _context.Entry(settings).Collection(s => s.EnabledModules).LoadAsync();
            return settings;
        }

        var cleanSettings = new AppSettings();
        AppSettings.Validate(cleanSettings);
        return cleanSettings;
    }
}