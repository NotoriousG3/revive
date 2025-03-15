using Microsoft.EntityFrameworkCore;

namespace TaskBoard.Data;

public class LogEntryCleaner
{
    public static async Task RemoveEntriesWithNoMatchingWorkId(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var entriesToRemove = context.LogEntries.Include(e => e.Work).Where(e => e.Work == null);
        
        if (entriesToRemove.Count() == 0) return;

        context.RemoveRange(entriesToRemove);
        await context.SaveChangesAsync();
    }
}