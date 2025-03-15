using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard;

public class PhoneNumberManager
{
    private readonly ApplicationDbContext _context;
    private readonly Utilities _utilities;

    public PhoneNumberManager(ApplicationDbContext context, Utilities utilities)
    {
        _context = context;
        _utilities = utilities;
    }

    public async Task Delete(PhoneListModel phone)
    {
        _context.Remove(phone);
        await _context.SaveChangesAsync();
    }

    public async Task<int> Count()
    {
        if (_context.PhoneList == null)
            return 0;

        return (await _context.PhoneList.ToListAsync()).Count;
    }

    public PhoneListModel PickRandom()
    {
        List<PhoneListModel> phoneNumbers = GetPhoneNumbers();


        return phoneNumbers[_utilities.RandomInt(0, _context.PhoneList!.Count())];
    }

    public List<PhoneListModel> GetPhoneNumbers()
    {
        if (_context.PhoneList == null)
            throw new Exception("PhoneNumberManager.GetPhoneNumbers is null parse it properly");

        return _context.PhoneList.ToArray().ToList();
    }

    public static PhoneNumberManager FromServiceProvider(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<PhoneNumberManager>();
    }
}