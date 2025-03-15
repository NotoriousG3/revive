using TaskBoard.Models;

namespace TaskBoard;

public class EmailAddressManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Utilities _utilities;

    public EmailAddressManager(IServiceScopeFactory scopeFactory, Utilities utilities)
    {
        _scopeFactory = scopeFactory;
        _utilities = utilities;
    }

    public async Task Delete(EmailListModel email)
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Remove(email);
        await context.SaveChangesAsync();
    }

    public async Task<int> Count()
    {
        await using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if(context.EmailList == null)
            return 0;

        return context.EmailList.Count();
    }

    public EmailListModel PickRandom()
    {
        using var context = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.EmailList == null)
            throw new Exception("EmailList is null");

        return context.EmailList.AsParallel().PickRandom();
    }
    
    public async Task<EmailListModel> PopRandom()
    {
        var email = PickRandom();
        await Delete(email);
        return email;
    }
}