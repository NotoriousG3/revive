using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models;

public class ProxyGroup
{
    [Key]
    public long Id { get; set; }

    public string Name { get; set; }
    public ProxyType ProxyType { get; set; } = ProxyType.Sticky;
    public virtual ICollection<Proxy> Proxies { get; set; }

    public static async Task<ProxyGroup?> GetFromDatabase(long proxyGroupId, IServiceProvider serviceProvider)
    {
        await using var context = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.ProxyGroups.FindAsync(proxyGroupId);
    }
}