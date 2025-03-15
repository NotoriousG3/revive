using Microsoft.EntityFrameworkCore;

namespace SnapWebManager.Data;

public static class MigrationApplier
{
    public static WebApplication MigrateDatabase<T>(this WebApplication webHost) where T : ApplicationDbContext
    {
        using (var scope = webHost.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var db = services.GetRequiredService<T>();
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }

        return webHost;
    }
}